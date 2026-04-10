using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace UTools
{
    [DefaultExecutionOrder(-10000)]
    internal sealed class UDIGlobalRuntime : MonoBehaviour
    {
        private const string RuntimeObjectName = "__UTools.GlobalRuntime";

        private static UDIGlobalRuntime _instance;
        private static UDIContainer _container;
        private static Task _readyTask;
        private static Exception _initializationException;
        private static bool _bootstrapAttempted;

        private CancellationTokenSource _initializationCancellation;
        private LifecycleManager _lifecycleManager;

        internal static bool HasGlobalContainer
        {
            get
            {
                EnsureExists();
                return _container != null;
            }
        }

        internal static UDIContainer Container
        {
            get
            {
                EnsureExists();
                return _container;
            }
        }

        internal static Task ReadyTask
        {
            get
            {
                EnsureExists();
                return _readyTask ?? Task.CompletedTask;
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            EnsureExists();
        }

        internal static Task EnsureInitializedAsync()
        {
            EnsureExists();
            return ReadyTask;
        }

        private static void EnsureExists()
        {
            if (_bootstrapAttempted)
            {
                return;
            }

            _bootstrapAttempted = true;
            GlobalInstaller[] installers = Resources.LoadAll<GlobalInstaller>(string.Empty);
            if (installers.Length == 0)
            {
                return;
            }

            if (installers.Length > 1)
            {
                _initializationException = new InvalidOperationException("Only one GlobalInstaller asset is supported. Please keep a single GlobalInstaller in Resources.");
                Debug.LogError(_initializationException.Message);
                return;
            }

            GameObject runtimeObject = new(RuntimeObjectName);
            DontDestroyOnLoad(runtimeObject);
            _instance = runtimeObject.AddComponent<UDIGlobalRuntime>();
            _instance.InitializeRuntime(installers[0]);
        }

        private void InitializeRuntime(GlobalInstaller installer)
        {
            _initializationCancellation = new CancellationTokenSource();
            _lifecycleManager = GetComponent<LifecycleManager>();
            if (_lifecycleManager == null)
            {
                _lifecycleManager = gameObject.AddComponent<LifecycleManager>();
            }

            SceneManager.sceneLoaded += OnSceneLoaded;
            _readyTask = InitializeRuntimeAsync(installer, _initializationCancellation.Token);
        }

        private async Task InitializeRuntimeAsync(GlobalInstaller installer, CancellationToken cancellationToken)
        {
            try
            {
                _container = new UDIContainer(null, _lifecycleManager, allowGlobalBindings: true);
                _container.Bind<LifecycleManager>()
                    .FromInstance(_lifecycleManager)
                    .AsSingle();

                installer.InstallBindings(_container);
                _container.FinalizeBindings();
                await _container.InitializeRequiredForContextStartAsync(cancellationToken);
                _lifecycleManager.Initialize();
            }
            catch (Exception exception)
            {
                _initializationException = exception;
                Debug.LogError($"Global UDI runtime failed to initialize: {exception}");
                throw;
            }
        }

        private async void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (_container == null)
            {
                return;
            }

            try
            {
                await ReadyTask;
                InjectScene(scene);
            }
            catch (Exception exception)
            {
                Debug.LogError($"Global UDI runtime failed to inject scene '{scene.name}': {exception}");
            }
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            _initializationCancellation?.Cancel();
            _initializationCancellation?.Dispose();
        }

        private static void InjectScene(Scene scene)
        {
            if (_container == null)
            {
                return;
            }

            UDIContext sceneContext = UDIContext.FindUniqueSceneContext(scene, out string error);
            if (error != null)
            {
                throw new InvalidOperationException(error);
            }

            if (sceneContext != null)
            {
                sceneContext.Initialize();
                return;
            }

            foreach (GameObject rootGameObject in scene.GetRootGameObjects())
            {
                if (_instance != null && rootGameObject == _instance.gameObject)
                {
                    continue;
                }

                _container.InjectGameObject(rootGameObject);
            }
        }
    }
}
