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
            GetContainer().InjectGameObject(instance);
            return instance;
        }

        public static GameObject InstantiateWithDependency(GameObject original)
        {
            GameObject instance = Object.Instantiate(original);
            GetContainer().InjectGameObject(instance);
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
            InjectObject(instance);
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Transform parent, bool worldPositionStays) where T : Object
        {
            T instance = Object.Instantiate(original, parent, worldPositionStays);
            InjectObject(instance);
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
            InjectObject(instance);
            return instance;
        }

        private static void InjectObject(Object instance)
        {
            if (instance is GameObject gameObject)
            {
                GetContainer().InjectGameObject(gameObject);
            }
            else if (instance is Component component)
            {
                GetContainer().InjectGameObject(component.gameObject);
            }
        }
    }
}
