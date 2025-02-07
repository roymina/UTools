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

namespace UTools
{
    public class UDIContainer
    {
        private static UDIContainer _instance;
        private Dictionary<Type, object> _singletons = new Dictionary<Type, object>();
        private Dictionary<Type, object> _persistentSingletons = new Dictionary<Type, object>();
        private Dictionary<Type, Func<object>> _factories = new Dictionary<Type, Func<object>>();

        public static UDIContainer Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new UDIContainer();
                }
                return _instance;
            }
        }

        private UDIContainer()
        {
        }

        public void RegisterSingleton<T>(T instance)
        {
            _singletons[typeof(T)] = instance;
        }

        public void RegisterPersistentSingleton<T>(T instance)
        {
            _persistentSingletons[typeof(T)] = instance;
        }

        public void RegisterFactory<T>(Func<T> factory)
        {
            _factories[typeof(T)] = () => factory();
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public object Resolve(Type type)
        {
            if (_persistentSingletons.ContainsKey(type))
            {
                return _persistentSingletons[type];
            }

            if (_singletons.ContainsKey(type))
            {
                return _singletons[type];
            }

            if (_factories.ContainsKey(type))
            {
                var instance = _factories[type].Invoke();
                return instance;
            }

            throw new Exception($"No registration for type {type.Name}");
        }
    }
}
