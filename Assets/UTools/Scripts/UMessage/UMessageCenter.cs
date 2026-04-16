using System;
using System.Collections.Generic;
using UnityEngine;

namespace UTools
{
    public interface IMessageSubscription : IDisposable
    {
    }

    public class UMessageCenter
    {
        private static UMessageCenter _instance;

        private readonly Dictionary<Type, ISubscriberCollection> _subscribers = new();
        private readonly object _gate = new();

        public static UMessageCenter Instance => _instance ??= new UMessageCenter();

        private UMessageCenter()
        {
        }

        public IMessageSubscription Subscribe<T>(Action<T> handler, bool replayPending = true)
        {
            if (handler == null)
            {
                throw new ArgumentNullException(nameof(handler));
            }

            List<T> pendingMessages = null;

            lock (_gate)
            {
                SubscriberCollection<T> collection = GetOrCreateCollection<T>();
                collection.CleanupDeadSubscribers();
                collection.Add(handler);

                if (replayPending)
                {
                    pendingMessages = collection.DrainPendingMessages();
                }
            }

            if (pendingMessages != null)
            {
                foreach (T message in pendingMessages)
                {
                    InvokeHandlerSafely(handler, message);
                }
            }

            return new MessageSubscription(() => Unsubscribe(handler));
        }

        public void Unsubscribe<T>(Action<T> handler)
        {
            if (handler == null)
            {
                return;
            }

            lock (_gate)
            {
                if (_subscribers.TryGetValue(typeof(T), out ISubscriberCollection collection))
                {
                    SubscriberCollection<T> typedCollection = (SubscriberCollection<T>)collection;
                    typedCollection.Remove(handler);
                    if (typedCollection.IsEmpty)
                    {
                        _subscribers.Remove(typeof(T));
                    }
                }
            }
        }

        public void Publish<T>(T message, bool cacheIfNoSubscribers = true)
        {
            Action<T>[] handlers;

            lock (_gate)
            {
                SubscriberCollection<T> collection = GetOrCreateCollection<T>();
                collection.CleanupDeadSubscribers();
                if (!collection.HasSubscribers)
                {
                    if (cacheIfNoSubscribers)
                    {
                        collection.Enqueue(message);
                    }

                    return;
                }

                handlers = collection.GetActiveHandlers();
            }

            foreach (Action<T> handler in handlers)
            {
                InvokeHandlerSafely(handler, message);
            }
        }

        public void Clear()
        {
            lock (_gate)
            {
                _subscribers.Clear();
            }
        }

        private SubscriberCollection<T> GetOrCreateCollection<T>()
        {
            Type messageType = typeof(T);
            if (!_subscribers.TryGetValue(messageType, out ISubscriberCollection collection))
            {
                collection = new SubscriberCollection<T>();
                _subscribers[messageType] = collection;
            }

            return (SubscriberCollection<T>)collection;
        }

        private static void InvokeHandlerSafely<T>(Action<T> handler, T message)
        {
            try
            {
                handler(message);
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }

        private interface ISubscriberCollection
        {
        }

        private sealed class MessageSubscription : IMessageSubscription
        {
            private Action _disposeAction;

            public MessageSubscription(Action disposeAction)
            {
                _disposeAction = disposeAction;
            }

            public void Dispose()
            {
                _disposeAction?.Invoke();
                _disposeAction = null;
            }
        }

        private sealed class SubscriberCollection<T> : ISubscriberCollection
        {
            private readonly List<Subscriber<T>> _handlers = new();
            private readonly Queue<T> _pendingMessages = new();

            public bool HasSubscribers => _handlers.Count > 0;
            public bool IsEmpty => _handlers.Count == 0 && _pendingMessages.Count == 0;

            public void Add(Action<T> handler)
            {
                _handlers.Add(new Subscriber<T>(handler));
            }

            public void Remove(Action<T> handler)
            {
                _handlers.RemoveAll(item => item.Matches(handler));
            }

            public void Enqueue(T message)
            {
                _pendingMessages.Enqueue(message);
            }

            public void CleanupDeadSubscribers()
            {
                _handlers.RemoveAll(item => !item.IsAlive);
            }

            public Action<T>[] GetActiveHandlers()
            {
                CleanupDeadSubscribers();
                Action<T>[] results = new Action<T>[_handlers.Count];
                for (int i = 0; i < _handlers.Count; i++)
                {
                    results[i] = _handlers[i].Handler;
                }

                return results;
            }

            public List<T> DrainPendingMessages()
            {
                List<T> pending = null;
                while (_pendingMessages.Count > 0)
                {
                    pending ??= new List<T>();
                    pending.Add(_pendingMessages.Dequeue());
                }

                return pending;
            }
        }

        private sealed class Subscriber<T>
        {
            private readonly UnityEngine.Object _unityOwner;
            private readonly bool _hasUnityOwner;

            public Subscriber(Action<T> handler)
            {
                Handler = handler;
                if (handler.Target is UnityEngine.Object unityObject)
                {
                    _unityOwner = unityObject;
                    _hasUnityOwner = true;
                }
            }

            public Action<T> Handler { get; }

            public bool IsAlive => !_hasUnityOwner || _unityOwner != null;

            public bool Matches(Action<T> handler)
            {
                return Handler == handler;
            }
        }
    }
}
