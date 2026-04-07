using UnityEngine;

namespace UTools
{
    public static class UGameObjectFactory
    {
        private static UDIContainer _container;

        public static void SetContainer(UDIContainer container)
        {
            _container = container;
        }

        private static UDIContainer GetContainer()
        {
            if (_container == null)
            {
                Debug.LogWarning("UGameObjectFactory: 未设置容器，自动创建默认容器。建议通过 UDIContext 设置。");
                _container = new UDIContainer();
            }

            return _container;
        }

        public static GameObject InstantiateWithDependency(GameObject original, Vector3 position, Quaternion rotation)
        {
            GameObject instance = Object.Instantiate(original, position, rotation);
            ResolveContainer(null).InjectGameObject(instance);
            return instance;
        }

        public static GameObject InstantiateWithDependency(GameObject original)
        {
            GameObject instance = Object.Instantiate(original);
            ResolveContainer(null).InjectGameObject(instance);
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original) where T : Object
        {
            T instance = Object.Instantiate(original);
            InjectObject(instance);
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Transform parent) where T : Object
        {
            T instance = Object.Instantiate(original, parent);
            InjectObject(instance, parent);
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Transform parent, bool worldPositionStays) where T : Object
        {
            T instance = Object.Instantiate(original, parent, worldPositionStays);
            InjectObject(instance, parent);
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            T instance = Object.Instantiate(original, position, rotation);
            InjectObject(instance);
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            T instance = Object.Instantiate(original, position, rotation, parent);
            InjectObject(instance, parent);
            return instance;
        }

        private static void InjectObject(Object instance, Transform parent = null)
        {
            if (instance is GameObject gameObject)
            {
                ResolveContainer(parent ?? gameObject.transform.parent).InjectGameObject(gameObject);
            }
            else if (instance is Component component)
            {
                ResolveContainer(parent ?? component.transform.parent).InjectGameObject(component.gameObject);
            }
        }

        private static UDIContainer ResolveContainer(Transform parent)
        {
            UDIContext context = FindNearestContext(parent);
            if (context != null && context.Container != null)
            {
                return context.Container;
            }

            if (UDIGlobalRuntime.HasGlobalContainer)
            {
                return UDIGlobalRuntime.Container;
            }

            return GetContainer();
        }

        private static UDIContext FindNearestContext(Transform current)
        {
            while (current != null)
            {
                UDIContext context = current.GetComponent<UDIContext>();
                if (context != null)
                {
                    return context;
                }

                current = current.parent;
            }

            return null;
        }
    }
}
