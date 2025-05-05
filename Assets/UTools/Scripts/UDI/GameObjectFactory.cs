using UnityEngine;

namespace UTools
{
    public static class UGameObjectFactory
    {
        private static readonly UDIContainer Container = new UDIContainer();

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
            Container.InjectDependencies(instance);
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
            Container.InjectDependencies(instance);
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
                Container.InjectDependencies(gameObject);
            }
            return instance;
        }

 

        public static T InstantiateWithDependency<T>(T original, Transform parent) where T : Object
        {
            var instance = Object.Instantiate(original, parent);
            if (instance is GameObject gameObject)
            {
                Container.InjectDependencies(gameObject);
            }
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Transform parent, bool worldPositionStays) where T : Object
        {
            var instance = Object.Instantiate(original, parent, worldPositionStays);
            if (instance is GameObject gameObject)
            {
                Container.InjectDependencies(gameObject);
            }
            return instance;
        }

        public static T InstantiateWithDependency<T>(T original, Vector3 position, Quaternion rotation) where T : Object
        {
            var instance = Object.Instantiate(original, position, rotation);
            if (instance is GameObject gameObject)
            {
                Container.InjectDependencies(gameObject);
            }
            return instance;
        }

 

        public static T InstantiateWithDependency<T>(T original, Vector3 position, Quaternion rotation, Transform parent) where T : Object
        {
            var instance = Object.Instantiate(original, position, rotation, parent);
            if (instance is GameObject gameObject)
            {
                Container.InjectDependencies(gameObject);
            }
            return instance;
        }
    }
}