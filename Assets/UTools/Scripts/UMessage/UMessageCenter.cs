//-----------------------------------------------------------------------
// <copyright file="UMessageCenter.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
//     MessageCenter Class.
// </summary>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;


namespace UTools
{
    public class UMessageCenter
    {
        // Singleton instance
        private static UMessageCenter _instance;

        // Dictionary to store subscribers for each message type
        private Dictionary<Type, List<Delegate>> _subscribers = new Dictionary<Type, List<Delegate>>();

        // Ensure only one instance exists
        public static UMessageCenter Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UMessageCenter();
                }
                return _instance;
            }
        }

        private UMessageCenter()
        {
            // Private constructor to prevent instantiation
        }

        /// <summary>
        /// Subscribe to a message type.
        /// </summary>
        /// <typeparam name="T">The type of message to subscribe to.</typeparam>
        /// <param name="handler">The handler method to be called when the message is published.</param>
        public void Subscribe<T>(Action<T> handler)
        {
            Type messageType = typeof(T);
            if (!_subscribers.ContainsKey(messageType))
            {
                _subscribers[messageType] = new List<Delegate>();
            }

            // Add the handler to the list
            _subscribers[messageType].Add(handler);
        }

        /// <summary>
        /// Unsubscribe from a message type.
        /// </summary>
        /// <typeparam name="T">The type of message to unsubscribe from.</typeparam>
        /// <param name="handler">The handler method to be removed from the subscription.</param>
        public void Unsubscribe<T>(Action<T> handler)
        {
            Type messageType = typeof(T);
            if (_subscribers.ContainsKey(messageType))
            {
                // Remove the handler from the list
                _subscribers[messageType].Remove(handler);
            }
        }

        /// <summary>
        /// Publish a message to all subscribed handlers.
        /// </summary>
        /// <typeparam name="T">The type of message being published.</typeparam>
        /// <param name="message">The message to be published.</param>
        public void Publish<T>(T message)
        {
            Type messageType = typeof(T);
            if (_subscribers.ContainsKey(messageType))
            {
                foreach (var handler in _subscribers[messageType])
                {
                    // Invoke the handler
                    handler.DynamicInvoke(message);
                }
            }
        }
    }


}
