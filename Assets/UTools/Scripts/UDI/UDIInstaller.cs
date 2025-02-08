using UnityEngine;
using UnityEngine.SceneManagement;

namespace UTools
{
    public abstract class UDIInstaller : MonoBehaviour
    {
        [SerializeField] private UDIInstaller sceneInstaller;
        [SerializeField] private UDIInstaller globalInstaller;

        public static UDIContainer Container { get; private set; }

        protected virtual void Awake()
        {
            if (Container == null)
            {
                Container = new UDIContainer();
                RegisterServices();
                if (globalInstaller != null)
                {
                    globalInstaller.RegisterServices();
                    DontDestroyOnLoad(globalInstaller.gameObject);
                }
                SceneManager.sceneLoaded += OnSceneLoaded;
            }
            if (sceneInstaller != null)
            {
                sceneInstaller.RegisterServices();
            }
            InjectAllComponents();
        }

        protected abstract void RegisterServices();

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            InjectAllComponents();
        }

        protected virtual void InjectAllComponents()
        {
            var allComponents = FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);
            Container.InjectDependencies(allComponents);
        }
    }
}

