using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UTools
{
    public class UDIContext : MonoBehaviour
    {
        [SerializeField]
        private List<MonoInstaller> _installers = new();
        [SerializeField]
        private List<ScriptableObjectInstaller> _scriptableObjectInstallers = new();

        private UDIContainer _container;
        private LifecycleManager _lifecycleManager;
        private bool _hasInitialized;

        public UDIContainer Container => _container;
        public LifecycleManager LifecycleManager => _lifecycleManager;

        protected virtual void Awake()
        {
            Initialize();
        }

        public void Initialize()
        {
            if (_hasInitialized)
            {
                return;
            }

            _hasInitialized = true;
            _lifecycleManager = GetComponent<LifecycleManager>();
            if (_lifecycleManager == null)
            {
                _lifecycleManager = gameObject.AddComponent<LifecycleManager>();
            }

            UDIContext parentContext = GetParentContext();
            UDIContainer parentContainer = parentContext?.Container;

            _container = new UDIContainer(parentContainer, _lifecycleManager);
            _container.Bind<LifecycleManager>()
                .FromInstance(_lifecycleManager)
                .AsSingle();

            InstallBindings();
            _container.FinalizeBindings();
            InjectContextObjects(parentContainer == null);

            if (parentContainer == null)
            {
                UGameObjectFactory.SetContainer(_container);
            }

            _lifecycleManager.Initialize();
        }

        private void InstallBindings()
        {
            HashSet<MonoInstaller> installedMonoInstallers = new();

            foreach (var installer in _scriptableObjectInstallers)
            {
                if (installer != null)
                {
                    installer.InstallBindings(_container);
                }
            }

            foreach (var installer in _installers)
            {
                if (installer != null)
                {
                    installer.InstallBindings(_container);
                    installedMonoInstallers.Add(installer);
                }
            }

            foreach (MonoInstaller installer in GetComponents<MonoInstaller>())
            {
                if (installer != null && installedMonoInstallers.Add(installer))
                {
                    installer.InstallBindings(_container);
                }
            }
        }

        private void InjectContextObjects(bool injectWholeScene)
        {
            if (injectWholeScene)
            {
                Scene scene = gameObject.scene;
                foreach (GameObject rootGameObject in scene.GetRootGameObjects())
                {
                    _container.InjectGameObject(rootGameObject);
                }
                return;
            }

            _container.InjectGameObject(gameObject);
        }

        private UDIContext GetParentContext()
        {
            Transform parent = transform.parent;
            while (parent != null)
            {
                UDIContext context = parent.GetComponent<UDIContext>();
                if (context != null)
                {
                    return context;
                }

                parent = parent.parent;
            }

            return null;
        }
    }

    public abstract class MonoInstaller : MonoBehaviour, IInstaller
    {
        protected virtual void Awake()
        {
            UDIContext localContext = GetComponent<UDIContext>();
            if (localContext != null)
            {
                localContext.Initialize();
                return;
            }

            if (GetComponentInParent<UDIContext>() != null)
            {
                return;
            }

            gameObject.AddComponent<UDIContext>().Initialize();
        }

        public abstract void InstallBindings(UDIContainer container);
    }

    public abstract class ScriptableObjectInstaller : ScriptableObject, IInstaller
    {
        public abstract void InstallBindings(UDIContainer container);
    }

    public interface IInstaller
    {
        void InstallBindings(UDIContainer container);
    }
}
