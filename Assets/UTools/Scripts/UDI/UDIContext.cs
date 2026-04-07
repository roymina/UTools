using System.Collections.Generic;
using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UTools
{
    [DefaultExecutionOrder(-10000)]
    public class UDIContext : MonoBehaviour
    {
        [SerializeField]
        private List<MonoInstaller> _installers = new();
        [SerializeField]
        private List<ScriptableObjectInstaller> _scriptableObjectInstallers = new();
        [SerializeField]
        private GameObject _managedContentRoot;

        private UDIContainer _container;
        private LifecycleManager _lifecycleManager;
        private bool _hasInitialized;
        private bool _isInitializing;
        private CancellationTokenSource _initializationCancellation;
        private Task _readyTask;

        public UDIContainer Container => _container;
        public LifecycleManager LifecycleManager => _lifecycleManager;
        public bool IsReady { get; private set; }
        public Task ReadyTask => _readyTask ?? Task.CompletedTask;
        public Exception InitializationException { get; private set; }
        public GameObject ManagedContentRoot => _managedContentRoot;

        protected virtual void Awake()
        {
            Initialize();
        }

        protected virtual void OnDestroy()
        {
            _initializationCancellation?.Cancel();
            _initializationCancellation?.Dispose();
        }

        public void Initialize()
        {
            if (_readyTask == null)
            {
                _readyTask = InitializeAsyncInternal();
            }
        }

        public Task InitializeAsync()
        {
            Initialize();
            return ReadyTask;
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

        private async Task InitializeAsyncInternal()
        {
            if (_hasInitialized || _isInitializing)
            {
                return;
            }

            _hasInitialized = true;
            _isInitializing = true;
            IsReady = false;
            InitializationException = null;
            _initializationCancellation = new CancellationTokenSource();

            PrepareManagedContentRoot();

            try
            {
                _lifecycleManager = GetComponent<LifecycleManager>();
                if (_lifecycleManager == null)
                {
                    _lifecycleManager = gameObject.AddComponent<LifecycleManager>();
                }

                UDIContainer parentContainer = await ResolveParentContainerAsync();
                _container = new UDIContainer(parentContainer, _lifecycleManager);
                _container.Bind<LifecycleManager>()
                    .FromInstance(_lifecycleManager)
                    .AsSingle();

                InstallBindings();
                _container.FinalizeBindings();
                await _container.InitializeRequiredForContextStartAsync(_initializationCancellation.Token);

                InjectContextObjects(parentContainer == null && _managedContentRoot == null);
                _lifecycleManager.Initialize();
                ActivateManagedContentRoot();
                IsReady = true;

                if (parentContainer == null && !UDIGlobalRuntime.HasGlobalContainer)
                {
                    UGameObjectFactory.SetContainer(_container);
                }
            }
            catch (Exception exception)
            {
                InitializationException = exception;
                Debug.LogError($"UDIContext '{name}' failed to initialize: {exception}");
                throw;
            }
            finally
            {
                _isInitializing = false;
            }
        }

        private void InjectContextObjects(bool injectWholeScene)
        {
            if (_managedContentRoot != null)
            {
                _container.InjectGameObject(_managedContentRoot);
                return;
            }

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

        private async Task<UDIContainer> ResolveParentContainerAsync()
        {
            UDIContext parentContext = GetParentContext();
            if (parentContext != null)
            {
                await parentContext.InitializeAsync();
                return parentContext.Container;
            }

            if (UDIGlobalRuntime.HasGlobalContainer)
            {
                await UDIGlobalRuntime.EnsureInitializedAsync();
                return UDIGlobalRuntime.Container;
            }

            return null;
        }

        private void PrepareManagedContentRoot()
        {
            if (_managedContentRoot == null)
            {
                return;
            }

            if (_managedContentRoot.activeSelf)
            {
                Debug.LogWarning($"ManagedContentRoot '{_managedContentRoot.name}' should start inactive so UDIContext can guarantee async readiness.");
                _managedContentRoot.SetActive(false);
            }
        }

        private void ActivateManagedContentRoot()
        {
            if (_managedContentRoot != null)
            {
                _managedContentRoot.SetActive(true);
            }
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

    public abstract class GlobalInstaller : ScriptableObjectInstaller
    {
    }

    public interface IInstaller
    {
        void InstallBindings(UDIContainer container);
    }
}
