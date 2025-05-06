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

            // Always use default constructor - no constructor injection
            var instance = Activator.CreateInstance<T>();
            _services[typeof(T)] = instance;
            InjectDependenciesIntoInstance(instance);
            ExecutePostConstructMethods(instance);
        }

        public object Resolve(Type type)
        {
            if (_services.TryGetValue(type, out var service))
            {
                return service;
            }

            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                // 首先尝试查找场景中已存在的实例
                var existingInstance = UnityEngine.Object.FindFirstObjectByType(type);
                if (existingInstance != null)
                {
                    // 缓存MonoBehaviour实例以便将来使用
                    _services[type] = existingInstance;
                    return existingInstance;
                }

                // 如果找不到实例，自动创建一个GameObject并挂载组件
                Debug.Log($"自动创建类型{type.Name}的MonoBehaviour实例。");
                GameObject gameObject = new GameObject(type.Name);
                var newInstance = gameObject.AddComponent(type);
                // 缓存新创建的MonoBehaviour实例
                _services[type] = newInstance;

                // 为新创建的实例注入依赖
                InjectDependenciesIntoInstance(newInstance);
                ExecutePostConstructMethods(newInstance);

                UnityEngine.Object.DontDestroyOnLoad(gameObject); // 保持单例模式，防止场景切换时销毁
                return newInstance;
            }

            return RegisterAndResolve(type);
        }

        private object RegisterAndResolve(Type type)
        {
            // Use default constructor - no constructor injection
            var instance = Activator.CreateInstance(type);
            _services[type] = instance;
            InjectDependenciesIntoInstance(instance);
            ExecutePostConstructMethods(instance);
            return instance;
        }

        /// <summary>
        /// Injects dependencies into all registered services.
        /// </summary> 
        public void InjectDependencies()
        {
            foreach (var service in _services.Values)
            {
                InjectDependenciesIntoInstance(service);
                ExecutePostConstructMethods(service);
            }
        }

        /// <summary>
        /// Injects dependencies into a single instance.
        /// </summary>
        /// <param name="instance">The instance to inject dependencies into.</param>
        private void InjectDependenciesIntoInstance(object instance)
        {
            InjectFields(instance);
            InjectProperties(instance);
            InjectMethods(instance);
        }

        /// <summary>
        /// Injects dependencies into all registered MonoBehaviour classes.
        /// </summary>
        /// <param name="objects"></param>
        public void InjectDependencies(MonoBehaviour[] objects)
        {
            foreach (var obj in objects)
            {
                InjectDependenciesIntoInstance(obj);
                ExecutePostConstructMethods(obj);
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
                InjectDependenciesIntoInstance(component);
                ExecutePostConstructMethods(component);
            }
        }

        /// <summary>
        /// Injects dependencies into fields marked with [Inject] attribute.
        /// </summary>
        /// <param name="obj">The object whose fields will be injected.</param>
        private void InjectFields(object obj)
        {
            if (obj == null) return;

            var fields = obj.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(f => Attribute.IsDefined(f, typeof(InjectAttribute)));

            foreach (var field in fields)
            {
                var service = Resolve(field.FieldType);
                field.SetValue(obj, service);
            }
        }

        /// <summary>
        /// Injects dependencies into properties marked with [Inject] attribute.
        /// </summary>
        /// <param name="obj">The object whose properties will be injected.</param>
        private void InjectProperties(object obj)
        {
            if (obj == null) return;

            var properties = obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(p => Attribute.IsDefined(p, typeof(InjectAttribute)) && p.CanWrite);

            foreach (var property in properties)
            {
                var service = Resolve(property.PropertyType);
                property.SetValue(obj, service);
            }
        }

        /// <summary>
        /// Injects dependencies using methods marked with [Inject] attribute.
        /// </summary>
        /// <param name="obj">The object whose methods will be used for injection.</param>
        private void InjectMethods(object obj)
        {
            if (obj == null) return;

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

        /// <summary>
        /// Executes methods annotated with the PostConstruct attribute after dependency injection is complete.
        /// </summary>
        /// <param name="obj">The object whose PostConstruct methods will be executed.</param>
        private void ExecutePostConstructMethods(object obj)
        {
            if (obj == null) return;

            var methods = obj.GetType().GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(m => Attribute.IsDefined(m, typeof(PostInjectionAttribute)));

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
