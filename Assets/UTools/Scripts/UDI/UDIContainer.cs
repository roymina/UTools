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
        private static readonly HashSet<Type> _singletonTypes = new HashSet<Type>();

        public void Register<T>() where T : class, new()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                return; // Skip registration if already registered as a singleton
            }

            var instance = new T();
            _services[typeof(T)] = instance;
            InjectMethods(instance);
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

            return RegisterAndResolve(type);
        }


        private object RegisterAndResolve(Type type)
        {
            var instance = Activator.CreateInstance(type);
            _services[type] = instance;
            InjectMethods(instance);
            return instance;
        }
        /// <summary>
        /// Injects dependencies into all registered services.
        /// </summary> 
        public void InjectDependencies()
        {
            foreach (var service in _services.Values)
            {
                InjectMethods(service);
            }
        }
        /// <summary>
        /// Injects dependencies into all registered MonoBehaviour classes.
        /// </summary>
        /// <param name="objects"></param>
        public void InjectDependencies(MonoBehaviour[] objects)
        {
            foreach (var obj in objects)
            {
                InjectFields(obj);
                InjectMethods(obj);
            }
        }

        /// <summary>
        /// Injects dependencies into all MonoBehaviour components of a GameObject.
        /// </summary>
        /// <param name="gameObject">The GameObject whose components will be injected.</param>
        public void InjectDependencies(GameObject gameObject)
        {
            var components = gameObject.GetComponents<MonoBehaviour>();
            foreach (var component in components)
            {
                InjectFields(component);
                InjectMethods(component);
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
