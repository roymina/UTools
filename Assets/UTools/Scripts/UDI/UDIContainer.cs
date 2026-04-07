using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace UTools
{
    public class UDIContainer
    {
        private readonly Dictionary<Type, object> _services = new();
        private readonly Dictionary<Type, BindInfo> _bindings = new();
        private readonly List<BindInfo> _bindingOrder = new();
        private readonly HashSet<object> _initializedInstances = new();
        private readonly HashSet<object> _asyncInitializedInstances = new();
        private readonly UDIContainer _parentContainer;
        private readonly bool _allowGlobalBindings;

        private LifecycleManager _lifecycleManager;

        public UDIContainer(UDIContainer parentContainer = null, LifecycleManager lifecycleManager = null, bool allowGlobalBindings = false)
        {
            _parentContainer = parentContainer;
            _lifecycleManager = lifecycleManager;
            _allowGlobalBindings = allowGlobalBindings;
        }

        public void SetLifecycleManager(LifecycleManager lifecycleManager)
        {
            _lifecycleManager = lifecycleManager;
        }

        public IBindingBuilder<T> Bind<T>()
        {
            var bindInfo = new BindInfo
            {
                ContractType = typeof(T),
                ConcreteType = typeof(T),
                Scope = BindingScope.Singleton
            };

            _bindings[typeof(T)] = bindInfo;
            ReplaceBindingOrder(bindInfo);
            return new BindingBuilder<T>(bindInfo, this);
        }

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
            ReplaceBindingOrder(bindInfo);
            return new BindingBuilder<TContract>(bindInfo, this);
        }

        public async Task InitializeRequiredForContextStartAsync(CancellationToken cancellationToken)
        {
            foreach (BindInfo bindInfo in _bindingOrder.Where(item => item.RequiredForContextStart).ToArray())
            {
                cancellationToken.ThrowIfCancellationRequested();
                object instance = Resolve(bindInfo.ContractType);
                if (instance is IAsyncInitializable asyncInitializable && _asyncInitializedInstances.Add(instance))
                {
                    await asyncInitializable.InitializeAsync(cancellationToken);
                }
            }
        }

        internal void MarkBindingGlobal(BindInfo bindInfo)
        {
            if (!_allowGlobalBindings)
            {
                throw new InvalidOperationException("AsGlobal() can only be used from the dedicated GlobalInstaller.");
            }

            bindInfo.IsGlobal = true;
            bindInfo.Scope = BindingScope.Singleton;
        }

        internal void MarkBindingRequiredForContextStart(BindInfo bindInfo)
        {
            bindInfo.RequiredForContextStart = true;
            bindInfo.NonLazy = true;
        }

        internal void FinalizeBinding(BindInfo bindInfo)
        {
            if (bindInfo == null)
            {
                return;
            }

            if (bindInfo.Instance != null)
            {
                RegisterBoundInstance(bindInfo, bindInfo.Instance);
                return;
            }

            if (bindInfo.NonLazy)
            {
                object instance = CreateInstance(bindInfo);
                if (ShouldCache(bindInfo.Scope))
                {
                    _services[bindInfo.ContractType] = instance;
                    bindInfo.Instance = instance;
                }
            }
        }

        public void FinalizeBindings()
        {
            foreach (BindInfo bindInfo in _bindings.Values.ToArray())
            {
                FinalizeBinding(bindInfo);
            }
        }

        public void Register<T>() where T : class, new()
        {
            Type type = typeof(T);
            if (_services.ContainsKey(type))
            {
                return;
            }

            T instance = Activator.CreateInstance<T>();
            _services[type] = instance;
            InitializeInstance(instance);
        }

        public object Resolve(Type type)
        {
            return Resolve(type, new Stack<Type>());
        }

        public T Resolve<T>()
        {
            return (T)Resolve(typeof(T));
        }

        public bool TryResolve(Type type, out object instance)
        {
            try
            {
                instance = Resolve(type);
                return true;
            }
            catch
            {
                instance = null;
                return false;
            }
        }

        public bool TryResolve<T>(out T instance)
        {
            if (TryResolve(typeof(T), out object resolved) && resolved is T typed)
            {
                instance = typed;
                return true;
            }

            instance = default;
            return false;
        }

        public void InjectDependencies()
        {
            foreach (object service in _services.Values.ToArray())
            {
                InitializeInstance(service);
            }
        }

        public void InjectDependencies(MonoBehaviour[] objects)
        {
            foreach (MonoBehaviour obj in objects)
            {
                InitializeInstance(obj);
            }
        }

        public void InjectDependencies(GameObject gameObject)
        {
            foreach (MonoBehaviour component in gameObject.GetComponents<MonoBehaviour>())
            {
                InitializeInstance(component);
            }
        }

        public void InjectGameObject(GameObject gameObject)
        {
            if (gameObject == null)
            {
                return;
            }

            foreach (MonoBehaviour component in gameObject.GetComponentsInChildren<MonoBehaviour>(true))
            {
                if (component != null)
                {
                    InitializeInstance(component);
                }
            }
        }

        private object Resolve(Type type, Stack<Type> resolutionPath)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            if (resolutionPath.Contains(type))
            {
                string chain = string.Join(" -> ", resolutionPath.Reverse().Select(item => item.Name).Concat(new[] { type.Name }));
                throw new InvalidOperationException($"检测到循环依赖: {chain}");
            }

            resolutionPath.Push(type);
            try
            {
                if (TryResolveFromCurrentContainer(type, resolutionPath, out object resolved))
                {
                    return resolved;
                }

                if (_parentContainer != null && _parentContainer.TryResolveFromCurrentContainer(type, resolutionPath, out resolved))
                {
                    return resolved;
                }

                if (typeof(MonoBehaviour).IsAssignableFrom(type))
                {
                    UnityEngine.Object existingInstance = UnityEngine.Object.FindFirstObjectByType(type);
                    if (existingInstance != null)
                    {
                        _services[type] = existingInstance;
                        InitializeInstance(existingInstance);
                        return existingInstance;
                    }

                    Debug.Log($"自动创建类型 {type.Name} 的 MonoBehaviour 实例。");
                    GameObject gameObject = new(type.Name);
                    Component newInstance = gameObject.AddComponent(type);
                    _services[type] = newInstance;
                    InitializeInstance(newInstance);
                    UnityEngine.Object.DontDestroyOnLoad(gameObject);
                    return newInstance;
                }

                if (!CanAutoCreate(type))
                {
                    throw new InvalidOperationException($"类型 {type.FullName} 未注册，且无法自动实例化。");
                }

                return AutoRegisterAndResolve(type);
            }
            finally
            {
                resolutionPath.Pop();
            }
        }

        private bool TryResolveFromCurrentContainer(Type type, Stack<Type> resolutionPath, out object instance)
        {
            if (_bindings.TryGetValue(type, out BindInfo bindInfo))
            {
                instance = ResolveFromBinding(bindInfo, resolutionPath);
                return true;
            }

            if (_services.TryGetValue(type, out object service))
            {
                instance = service;
                return true;
            }

            instance = null;
            return false;
        }

        private object ResolveFromBinding(BindInfo bindInfo, Stack<Type> resolutionPath)
        {
            if (bindInfo.Instance != null && ShouldCache(bindInfo.Scope))
            {
                RegisterBoundInstance(bindInfo, bindInfo.Instance);
                return bindInfo.Instance;
            }

            if (ShouldCache(bindInfo.Scope) && _services.TryGetValue(bindInfo.ContractType, out object cached))
            {
                return cached;
            }

            object instance = CreateInstance(bindInfo);
            if (ShouldCache(bindInfo.Scope))
            {
                _services[bindInfo.ContractType] = instance;
                bindInfo.Instance = instance;
            }

            return instance;
        }

        private object AutoRegisterAndResolve(Type type)
        {
            object instance = Activator.CreateInstance(type);
            _services[type] = instance;
            InitializeInstance(instance);
            return instance;
        }

        private object CreateInstance(BindInfo bindInfo)
        {
            Type type = bindInfo.ConcreteType ?? bindInfo.ContractType;
            object instance;

            if (typeof(MonoBehaviour).IsAssignableFrom(type))
            {
                instance = CreateMonoBehaviourInstance(type, bindInfo.GameObjectContext);
            }
            else
            {
                if (!CanAutoCreate(type))
                {
                    throw new InvalidOperationException($"类型 {type.FullName} 无法自动创建，请先在容器中注册。");
                }

                instance = Activator.CreateInstance(type);
                InitializeInstance(instance);
            }

            return instance;
        }

        private object CreateMonoBehaviourInstance(Type type, GameObject contextObject = null)
        {
            UnityEngine.Object existingInstance = UnityEngine.Object.FindFirstObjectByType(type);
            if (existingInstance != null)
            {
                InitializeInstance(existingInstance);
                return existingInstance;
            }

            if (contextObject != null)
            {
                Component component = contextObject.GetComponent(type) ?? contextObject.AddComponent(type);
                InitializeInstance(component);
                return component;
            }

            Debug.Log($"自动创建类型 {type.Name} 的 MonoBehaviour 实例。");
            GameObject gameObject = new(type.Name);
            Component newInstance = gameObject.AddComponent(type);
            InitializeInstance(newInstance);
            return newInstance;
        }

        private void InitializeInstance(object instance)
        {
            if (instance == null || !_initializedInstances.Add(instance))
            {
                return;
            }

            InjectFields(instance);
            InjectProperties(instance);
            InjectMethods(instance);
            ExecutePostConstructMethods(instance);
            _lifecycleManager?.Add(instance);
        }

        private void RegisterBoundInstance(BindInfo bindInfo, object instance)
        {
            _services[bindInfo.ContractType] = instance;
            bindInfo.Instance = instance;
            InitializeInstance(instance);
        }

        private void InjectFields(object obj)
        {
            if (obj == null)
            {
                return;
            }

            IEnumerable<System.Reflection.FieldInfo> fields = ReflectionCache.GetInstanceFields(obj.GetType())
                .Where(field => Attribute.IsDefined(field, typeof(InjectAttribute)));

            foreach (var field in fields)
            {
                object service = Resolve(field.FieldType);
                field.SetValue(obj, service);
            }
        }

        private void InjectProperties(object obj)
        {
            if (obj == null)
            {
                return;
            }

            IEnumerable<System.Reflection.PropertyInfo> properties = ReflectionCache.GetInstanceProperties(obj.GetType())
                .Where(property => Attribute.IsDefined(property, typeof(InjectAttribute)) && property.CanWrite);

            foreach (var property in properties)
            {
                object service = Resolve(property.PropertyType);
                property.SetValue(obj, service);
            }
        }

        private void InjectMethods(object obj)
        {
            if (obj == null)
            {
                return;
            }

            IEnumerable<System.Reflection.MethodInfo> methods = ReflectionCache.GetInstanceMethods(obj.GetType())
                .Where(method => Attribute.IsDefined(method, typeof(InjectAttribute)));

            foreach (var method in methods)
            {
                object[] parameters = method.GetParameters()
                    .Select(parameter => Resolve(parameter.ParameterType))
                    .ToArray();
                method.Invoke(obj, parameters);
            }
        }

        private void ExecutePostConstructMethods(object obj)
        {
            if (obj == null)
            {
                return;
            }

            IEnumerable<System.Reflection.MethodInfo> methods = ReflectionCache.GetInstanceMethods(obj.GetType())
                .Where(method => Attribute.IsDefined(method, typeof(PostInjectionAttribute)));

            foreach (var method in methods)
            {
                object[] parameters = method.GetParameters()
                    .Select(parameter => Resolve(parameter.ParameterType))
                    .ToArray();
                method.Invoke(obj, parameters);
            }
        }

        private static bool ShouldCache(BindingScope scope)
        {
            return scope == BindingScope.Singleton || scope == BindingScope.Scoped;
        }

        private static bool CanAutoCreate(Type type)
        {
            return !type.IsAbstract && !type.IsInterface && !type.ContainsGenericParameters;
        }

        private void ReplaceBindingOrder(BindInfo bindInfo)
        {
            _bindingOrder.RemoveAll(item => item.ContractType == bindInfo.ContractType);
            _bindingOrder.Add(bindInfo);
        }
    }

    internal static class ReflectionCache
    {
        private static readonly Dictionary<Type, System.Reflection.FieldInfo[]> InstanceFields = new();
        private static readonly Dictionary<Type, System.Reflection.PropertyInfo[]> InstanceProperties = new();
        private static readonly Dictionary<Type, System.Reflection.MethodInfo[]> InstanceMethods = new();

        public static System.Reflection.FieldInfo[] GetInstanceFields(Type type)
        {
            if (!InstanceFields.TryGetValue(type, out var fields))
            {
                List<System.Reflection.FieldInfo> results = new();
                Type currentType = type;
                while (currentType != null && currentType != typeof(object))
                {
                    results.AddRange(currentType.GetFields(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.DeclaredOnly));
                    currentType = currentType.BaseType;
                }

                fields = results.ToArray();
                InstanceFields[type] = fields;
            }

            return fields;
        }

        public static System.Reflection.PropertyInfo[] GetInstanceProperties(Type type)
        {
            if (!InstanceProperties.TryGetValue(type, out var properties))
            {
                List<System.Reflection.PropertyInfo> results = new();
                Type currentType = type;
                while (currentType != null && currentType != typeof(object))
                {
                    results.AddRange(currentType.GetProperties(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.DeclaredOnly));
                    currentType = currentType.BaseType;
                }

                properties = results.ToArray();
                InstanceProperties[type] = properties;
            }

            return properties;
        }

        public static System.Reflection.MethodInfo[] GetInstanceMethods(Type type)
        {
            if (!InstanceMethods.TryGetValue(type, out var methods))
            {
                List<System.Reflection.MethodInfo> results = new();
                Type currentType = type;
                while (currentType != null && currentType != typeof(object))
                {
                    results.AddRange(currentType.GetMethods(
                        System.Reflection.BindingFlags.Instance |
                        System.Reflection.BindingFlags.Public |
                        System.Reflection.BindingFlags.NonPublic |
                        System.Reflection.BindingFlags.DeclaredOnly));
                    currentType = currentType.BaseType;
                }

                methods = results.ToArray();
                InstanceMethods[type] = methods;
            }

            return methods;
        }
    }
}
