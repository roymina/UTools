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
using System.Reflection;
using UnityEngine;

namespace UTools
{
    public class UDIContainer
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public void Register<T>() where T : class, new()
        {
            var instance = new T();
            _services[typeof(T)] = instance;
            InjectMethods(instance);
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
        public void InjectDependencies()
        {
            foreach (var service in _services.Values)
            {
                InjectMethods(service);
            }
        }
        public void InjectDependencies(MonoBehaviour[] objects)
        {
            foreach (var obj in objects)
            {
                InjectFields(obj);
                InjectMethods(obj);
            }
        }
        private void InjectFields(MonoBehaviour obj)
        {
            var fields = obj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => Attribute.IsDefined(f, typeof(InjectAttribute)));

            foreach (var field in fields)
            {
                var service = Resolve(field.FieldType);
                field.SetValue(obj, service);
            }
        }

        private void InjectMethods(object obj)
        {
            var methods = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => Attribute.IsDefined(m, typeof(InjectAttribute)));

            foreach (var method in methods)
            {
                var parameters = method.GetParameters()
                    .Select(p => Resolve(p.ParameterType))
                    .ToArray();
                method.Invoke(obj, parameters);
            }
        }
    }
}
