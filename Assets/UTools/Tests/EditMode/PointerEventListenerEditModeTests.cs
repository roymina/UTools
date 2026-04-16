using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;

namespace UTools.Tests
{
    public class PointerEventListenerEditModeTests
    {
        private EventSystem _eventSystem;
        private GameObject _eventSystemObject;

        [SetUp]
        public void SetUp()
        {
            UMessageCenter.Instance.Clear();
            _eventSystemObject = new GameObject("EventSystem");
            _eventSystem = _eventSystemObject.AddComponent<EventSystem>();
        }

        [TearDown]
        public void TearDown()
        {
            UMessageCenter.Instance.Clear();
            if (_eventSystemObject != null)
            {
                Object.DestroyImmediate(_eventSystemObject);
            }
        }

        [Test]
        public void PointerClick_WhenNotDragging_InvokesCallbacksAndPublishesMessage()
        {
            int clickCount = 0;
            PointerEventData callbackData = null;
            PointerEventMessage receivedMessage = default;
            int receivedMessages = 0;

            IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<PointerEventMessage>(message =>
            {
                receivedMessages++;
                receivedMessage = message;
            }, replayPending: false);

            GameObject target = new("Target");
            PointerEventListener listener = target.AddComponent<PointerEventListener>();
            listener.publishGlobally = true;
            listener.onClick += () => clickCount++;
            listener.onClickWithData += data => callbackData = data;

            PointerEventData eventData = CreateEventData();
            listener.OnPointerClick(eventData);

            Assert.That(clickCount, Is.EqualTo(1));
            Assert.That(callbackData, Is.SameAs(eventData));
            Assert.That(receivedMessages, Is.EqualTo(1));
            Assert.That(receivedMessage.EventType, Is.EqualTo(PointerEventType.Click));
            Assert.That(receivedMessage.Target, Is.EqualTo(target));
            Assert.That(receivedMessage.EventData, Is.SameAs(eventData));
            Assert.That(receivedMessage.Is3DObject, Is.True);

            subscription.Dispose();
            Object.DestroyImmediate(target);
        }

        [Test]
        public void PointerClick_WhenDragging_DoesNotInvokeHandlersOrPublish()
        {
            int clickCount = 0;
            int receivedMessages = 0;

            IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<PointerEventMessage>(_ => receivedMessages++, replayPending: false);

            GameObject target = new("Target");
            PointerEventListener listener = target.AddComponent<PointerEventListener>();
            listener.publishGlobally = true;
            listener.onClick += () => clickCount++;

            PointerEventData eventData = CreateEventData();
            eventData.dragging = true;

            listener.OnPointerClick(eventData);

            Assert.That(clickCount, Is.EqualTo(0));
            Assert.That(receivedMessages, Is.EqualTo(0));

            subscription.Dispose();
            Object.DestroyImmediate(target);
        }

        [Test]
        public void Drag_UsesPointerDeltaForCallbacksAndMessages()
        {
            Vector2 callbackDelta = Vector2.zero;
            PointerEventMessage receivedMessage = default;

            IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<PointerEventMessage>(message => receivedMessage = message, replayPending: false);

            GameObject target = new("UI Target", typeof(RectTransform));
            PointerEventListener listener = target.AddComponent<PointerEventListener>();
            listener.publishGlobally = true;
            listener.onDragWithData += (_, delta) => callbackDelta = delta;

            PointerEventData eventData = CreateEventData();
            eventData.delta = new Vector2(12f, -4f);

            listener.OnDrag(eventData);

            Assert.That(callbackDelta, Is.EqualTo(eventData.delta));
            Assert.That(receivedMessage.EventType, Is.EqualTo(PointerEventType.Drag));
            Assert.That(receivedMessage.Delta, Is.EqualTo(eventData.delta));
            Assert.That(receivedMessage.Is3DObject, Is.False);

            subscription.Dispose();
            Object.DestroyImmediate(target);
        }

        private PointerEventData CreateEventData()
        {
            return new PointerEventData(_eventSystem)
            {
                position = new Vector2(100f, 50f),
                button = PointerEventData.InputButton.Left
            };
        }
    }
}
