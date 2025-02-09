using UnityEngine;
using UnityEngine.SceneManagement;

namespace UTools
{
    public abstract class UDIInstallerBase : MonoBehaviour
    {
        protected static readonly UDIContainer Container = new UDIContainer();

        protected abstract void RegisterSceneServices();
        protected abstract void RegisterGlobalServices();

        private void Awake()
        {
            RegisterSceneServices();
            InjectAllComponents();
            SceneManager.sceneLoaded += (scene, mode) => InjectAllComponents();
        }

        public static void InitializeGlobalInstaller(UDIInstallerBase globalInstaller)
        {
            globalInstaller.RegisterGlobalServices();
            DontDestroyOnLoad(globalInstaller.gameObject);
        }

        private void InjectAllComponents()
        {
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Container.InjectDependencies(allComponents);
            Container.InjectDependencies();
        }
    }
}

