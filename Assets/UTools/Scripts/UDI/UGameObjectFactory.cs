using UnityEngine;

namespace UTools
{
    public static class UGameObjectFactory
    {
        private static UDIContainer _container;

        /// <summary>
        /// 设置全局容器引用（通常由 UDIInstallerBase 或 UDIContext 调用）
        /// </summary>
        public static void SetContainer(UDIContainer container)
        {
            _container = container;
        }

        /// <summary>
        /// 获取或创建默认容器
        /// </summary>
        private static UDIContainer GetContainer()
        {
            if (_container == null)
            {
                Debug.LogWarning("UGameObjectFactory: 没有设置容器，创建默认容器。建议通过 SetContainer 方法设置容器。");
                _container = new UDIContainer();
            }
            return _container;
        }

        /// <summary>
        /// Instantiates a GameObject and injects dependencies into its components.
        /// </summary>
        /// <param name="original">The original GameObject to instantiate.</param>
        /// <param name="position">The position for the new GameObject.</param>
        /// <param name="rotation">The rotation for the new GameObject.</param>
        /// <returns>The instantiated GameObject with dependencies injected.</returns>
        public static GameObject InstantiateWithDependency(GameObject original, Vector3 position, Quaternion rotation)
        {
            var instance = Object.Instantiate(original, position, rotation);
            GetContainer().InjectDependencies(instance);
            return instance;
        }

        /// <summary>
        /// Instantiates a GameObject and injects dependencies into its components.
        /// </summary>
        /// <param name="original">The original GameObject to instantiate.</param>
        /// <returns>The instantiated GameObject with dependencies injected.</returns>
        public static GameObject InstantiateWithDependency(GameObject original)
        {
            var instance = Object.Instantiate(original);
            GetContainer().InjectDependencies(instance);
            return instance;
        }

        /// <summary>
        /// Instantiates a GameObject and injects dependencies into its components.
        /// </summary>
        public static T InstantiateWithDependency<T>(T original) where T : Object
        {
            var instance = Object.Instantiate(original);
            if (instance is GameObject gameObject)
            {
                GetContainer().InjectDependencies(gameObject);
            }
            return instance;
        }



        public static T InstantiateWithDependency<T>(T original, Transform parent) where T : Object
        {
            var instance = Object.Instantiate(original, parent);
            if (instance is GameObject gameObject)
            {
                GetContainer().InjectDependencies(gameObject);
            }
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Transform parent, bool worldPositionStays) where T : Object
        {
            var instance = Object.Instantiate(original, parent, worldPositionStays);
            if (instance is GameObject gameObject)
            {
                GetContainer().InjectDependencies(gameObject);
            }
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            var instance = Object.Instantiate(original, position, rotation);
            if (instance is GameObject gameObject)
            {
                GetContainer().InjectDependencies(gameObject);
            }
            return instance;
        }



        public static T InstantiateWithDependency<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            var instance = Object.Instantiate(original, position, rotation, parent);
            if (instance is GameObject gameObject)
            {
                GetContainer().InjectDependencies(gameObject);
            }
            return instance;
        }
    }
}