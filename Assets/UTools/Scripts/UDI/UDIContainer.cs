//-----------------------------------------------------------------------
// <copyright file="UDependencyContainer.cs" company="DxTech Co. Ltd.">
//     Copyright (c) DxTech Co. Ltd. All rights reserved.
// </copyright>
// <author>Roy</author>
// <date>2025-02-07</date>
// <summary>
// DependencyContainer
// </summary>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UTools
{
    public class UDIContainer
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>(T instance) where T : class
        {
            _services[typeof(T)] = instance;
        }

        public void Register<T>() where T : class, new()
        {
            _services[typeof(T)] = new T();
        }

        public T Resolve<T>() where T : class
        {
            return _services.TryGetValue(typeof(T), out var service) ? service as T : throw new Exception($"Service {typeof(T)} not registered");
        }


        public object Resolve(Type type)
        {
            if (_services.TryGetValue(type, out var service))
            {
                return service;
            }

            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                var existingInstance = UnityEngine.Object.FindFirstObjectByType(type);
                if (existingInstance != null)
                {
                    return existingInstance;
                }

                var newGameObject = new GameObject(type.Name);
                var newInstance = newGameObject.AddComponent(type);
                return newInstance;
            }

            throw new Exception($"Service {type} not registered");
        }

        public void InjectDependencies(MonoBehaviour[] objects)
        {
            foreach (var obj in objects)
            {
                var fields = obj.GetType().GetFields(System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
                    .Where(f => Attribute.IsDefined(f, typeof(InjectAttribute)));

                foreach (var field in fields)
                {
                    var service = Resolve(field.FieldType);
                    field.SetValue(obj, service);
                }
            }
        }
    }
}
