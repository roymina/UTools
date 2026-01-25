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
        private readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();
        private readonly Dictionary<Type, BindInfo> _bindings = new Dictionary<Type, BindInfo>();
        private readonly HashSet<Type> _singletonTypes = new HashSet<Type>();
        private readonly UDIContainer _parentContainer;

        public UDIContainer(UDIContainer parentContainer = null)
        {
            _parentContainer = parentContainer;
        }

        #region Fluent Binding API

        /// <summary>
        /// 开始一个新的绑定配置（Fluent API）
        /// </summary>
        public IBindingBuilder<T> Bind<T>()
        {
            var bindInfo = new BindInfo
            {
                ContractType = typeof(T),
                ConcreteType = typeof(T),
                Scope = BindingScope.Singleton
            };

            _bindings[typeof(T)] = bindInfo;
            return new BindingBuilder<T>(bindInfo, this);
        }

        /// <summary>
        /// 绑定接口到实现类型
        /// </summary>
        public IBindingBuilder<TContract> Bind<TContract, TImplementation>()
            where TImplementation : class, TContract
        {
            var bindInfo = new BindInfo
            {
                ContractType = typeof(TContract),
                ConcreteType = typeof(TImplementation),
                Scope = BindingScope.Singleton
            };

            _bindings[typeof(TContract)] = bindInfo;
            return new BindingBuilder<TContract>(bindInfo, this);
        }

        /// <summary>
        /// 完成绑定并创建实例（如果需要）
        /// </summary>
        internal void FinalizeBinding(BindInfo bindInfo)
        {
            if (bindInfo.Instance != null)
            {
                // 已有实例，直接注册
                _services[bindInfo.ContractType] = bindInfo.Instance;
                if (bindInfo.Scope == BindingScope.Singleton)
                {
                    _singletonTypes.Add(bindInfo.ContractType);
                }
                return;
            }

            if (bindInfo.NonLazy)
            {
                // NonLazy 模式，立即创建实例
                var instance = CreateInstance(bindInfo);
                _services[bindInfo.ContractType] = instance;
            }
        }

        private object CreateInstance(BindInfo bindInfo)
        {
            var type = bindInfo.ConcreteType ?? bindInfo.ContractType;

            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                return CreateMonoBehaviourInstance(type, bindInfo.GameObjectContext);
            }

            var instance = Activator.CreateInstance(type);
            InjectDependenciesIntoInstance(instance);
            ExecutePostConstructMethods(instance);
            return instance;
        }

        private object CreateMonoBehaviourInstance(Type type, GameObject contextObject = null)
        {
            // 首先尝试查找场景中已存在的实例
            var existingInstance = UnityEngine.Object.FindFirstObjectByType(type);
            if (existingInstance != null)
            {
                return existingInstance;
            }

            // 如果提供了上下文GameObject，在其上添加组件
            if (contextObject != null)
            {
                var component = contextObject.AddComponent(type);
                InjectDependenciesIntoInstance(component);
                ExecutePostConstructMethods(component);
                return component;
            }

            // 创建新的GameObject并添加组件
            Debug.Log($"自动创建类型{type.Name}的MonoBehaviour实例。");
            GameObject gameObject = new GameObject(type.Name);
            var newInstance = gameObject.AddComponent(type);
            InjectDependenciesIntoInstance(newInstance);
            ExecutePostConstructMethods(newInstance);
            return newInstance;
        }

        #endregion

        #region Legacy Registration API

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

        #endregion

        #region Resolution

        public object Resolve(Type type)
        {
            // 首先检查本容器的绑定
            if (_bindings.TryGetValue(type, out var bindInfo))
            {
                return ResolveFromBinding(bindInfo);
            }

            // 检查本容器的服务
            if (_services.TryGetValue(type, out var service))
            {
                return service;
            }

            // 尝试从父容器解析
            if (_parentContainer != null)
            {
                try
                {
                    return _parentContainer.Resolve(type);
                }
                catch
                {
                    // 父容器没有，继续在本容器创建
                }
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

        private object ResolveFromBinding(BindInfo bindInfo)
        {
            // 如果已有实例且是单例，返回该实例
            if (bindInfo.Instance != null && bindInfo.Scope == BindingScope.Singleton)
            {
                return bindInfo.Instance;
            }

            // 检查缓存的服务
            if (bindInfo.Scope == BindingScope.Singleton && _services.TryGetValue(bindInfo.ContractType, out var cached))
            {
                return cached;
            }

            // 创建新实例
            var instance = CreateInstance(bindInfo);

            // 如果是单例，缓存实例
            if (bindInfo.Scope == BindingScope.Singleton)
            {
                _services[bindInfo.ContractType] = instance;
                bindInfo.Instance = instance;
            }

            return instance;
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
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
        /// Injects dependencies into a GameObject and all its children recursively.
        /// </summary>
        /// <param name="gameObject">The GameObject whose components (and children's components) will be injected.</param>
        public void InjectGameObject(GameObject gameObject)
        {
            if (gameObject == null) return;

            // 注入当前 GameObject 的所有组件
            var components = gameObject.GetComponentsInChildren<MonoBehaviour>(true);
            foreach (var component in components)
            {
                if (component != null)
                {
                    InjectDependenciesIntoInstance(component);
                    ExecutePostConstructMethods(component);
                }
            }
        }

        #endregion

        #region Field/Property/Method Injection

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

        #endregion
    }
}
