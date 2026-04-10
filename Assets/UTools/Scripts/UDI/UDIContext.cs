using System.Collections.Generic;
using System;
using System.Linq;
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

        private UDIContainer _container;
        private LifecycleManager _lifecycleManager;
        private bool _hasInitialized;
        private bool _isInitializing;
        private CancellationTokenSource _initializationCancellation;
        private Task _readyTask;
        private bool _injectWholeScene;
        private GameObject _bootstrapRoot;
        private readonly List<GameObject> _suspendedSceneRoots = new();

        public UDIContainer Container => _container;
        public LifecycleManager LifecycleManager => _lifecycleManager;
        public bool IsReady { get; private set; }
        public Task ReadyTask => _readyTask ?? Task.CompletedTask;
        public Exception InitializationException { get; private set; }

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

            try
            {
                ConfigureSceneOwnership();

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
                SuspendSceneRootsUntilReadyIfNeeded();
                await _container.InitializeRequiredForContextStartAsync(_initializationCancellation.Token);

                InjectContextObjects();
                _lifecycleManager.Initialize();
                RestoreSuspendedSceneRoots();
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

        internal static UDIContext FindUniqueSceneContext(Scene scene, out string error)
        {
            error = null;
            List<UDIContext> contexts = new();

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                contexts.AddRange(rootGameObject.GetComponentsInChildren<UDIContext>(true));
            }

            contexts = contexts
                .Where(context => context != null)
                .Distinct()
                .ToList();

            if (contexts.Count == 0)
            {
                return null;
            }

            if (contexts.Count > 1)
            {
                error = $"Scene '{scene.name}' contains multiple UDIContext components. Scene-level injection supports exactly one UDIContext per scene.";
                return null;
            }

            return contexts[0];
        }

        private void InjectContextObjects()
        {
            if (_injectWholeScene)
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

        private void ConfigureSceneOwnership()
        {
            UDIContext sceneContext = FindUniqueSceneContext(gameObject.scene, out string error);
            if (error != null)
            {
                throw new InvalidOperationException(error);
            }

            if (sceneContext == null)
            {
                throw new InvalidOperationException($"Scene '{gameObject.scene.name}' does not contain a UDIContext.");
            }

            if (sceneContext != this)
            {
                throw new InvalidOperationException($"UDIContext '{name}' is not the active scene context for scene '{gameObject.scene.name}'.");
            }

            _injectWholeScene = true;
            _bootstrapRoot = transform.root != null ? transform.root.gameObject : gameObject;
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

        private void SuspendSceneRootsUntilReadyIfNeeded()
        {
            if (!_injectWholeScene || !_container.HasRequiredForContextStartBindings)
            {
                return;
            }

            foreach (GameObject rootGameObject in gameObject.scene.GetRootGameObjects())
            {
                if (rootGameObject == null || rootGameObject == _bootstrapRoot || !rootGameObject.activeSelf)
                {
                    continue;
                }

                _suspendedSceneRoots.Add(rootGameObject);
                rootGameObject.SetActive(false);
            }
        }

        private void RestoreSuspendedSceneRoots()
        {
            foreach (GameObject rootGameObject in _suspendedSceneRoots)
            {
                if (rootGameObject != null)
                {
                    rootGameObject.SetActive(true);
                }
            }

            _suspendedSceneRoots.Clear();
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
