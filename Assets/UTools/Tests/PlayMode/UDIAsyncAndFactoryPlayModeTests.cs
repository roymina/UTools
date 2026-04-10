using System.Collections;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace UTools.Tests
{
    public class UDIAsyncAndFactoryPlayModeTests
    {
        [UnitySetUp]
        public IEnumerator SetUp()
        {
            Scene scene = SceneManager.CreateScene($"InitTestScene{System.Guid.NewGuid()}");
            SceneManager.SetActiveScene(scene);
            yield return null;
        }

        [TearDown]
        public void TearDown()
        {
            ResetGlobalRuntime();
            ResetFactoryContainer();
        }

        [UnityTest]
        public IEnumerator SceneContext_WithGlobalContainer_InjectsSiblingRootUsingLocalBindings()
        {
            GameObject globalHost = CreateGlobalContainerHost(new MarkerService("global"));

            GameObject bootstrap = new("Bootstrap");
            bootstrap.SetActive(false);

            UDIContext context = bootstrap.AddComponent<UDIContext>();
            MarkerInstaller installer = bootstrap.AddComponent<MarkerInstaller>();
            installer.Service = new MarkerService("local");

            GameObject sibling = new("Sibling");
            sibling.SetActive(false);
            MarkerConsumer consumer = sibling.AddComponent<MarkerConsumer>();

            bootstrap.SetActive(true);
            sibling.SetActive(true);

            yield return new WaitUntil(() => context.IsReady);
            yield return null;

            Assert.That(consumer.Service, Is.Not.Null);
            Assert.That(consumer.Service.Source, Is.EqualTo("local"));
            Assert.That(consumer.AwakenedWithService, Is.True);

            Object.Destroy(sibling);
            Object.Destroy(bootstrap);
            Object.Destroy(globalHost);
        }

        [UnityTest]
        public IEnumerator SceneContext_WithRequiredAsyncServices_WaitsOnlyAsyncWaitRootSubtree()
        {
            GameObject bootstrap = new("Bootstrap");
            bootstrap.SetActive(false);

            UDIContext context = bootstrap.AddComponent<UDIContext>();
            AsyncInstaller installer = bootstrap.AddComponent<AsyncInstaller>();
            AsyncGateService gateService = new();
            installer.Service = gateService;

            GameObject bootstrapChild = new("BootstrapChild");
            bootstrapChild.transform.SetParent(bootstrap.transform);
            ActivationProbe bootstrapProbe = bootstrapChild.AddComponent<ActivationProbe>();

            GameObject asyncWaitRoot = new("AsyncWaitRoot");
            asyncWaitRoot.transform.SetParent(bootstrap.transform);
            AsyncConsumer waitingConsumer = asyncWaitRoot.AddComponent<AsyncConsumer>();

            SetAsyncWaitRoot(context, asyncWaitRoot);

            GameObject sibling = new("Sibling");
            sibling.SetActive(false);
            AsyncConsumer siblingConsumer = sibling.AddComponent<AsyncConsumer>();

            bootstrap.SetActive(true);
            sibling.SetActive(true);
            yield return null;

            Assert.That(context.IsReady, Is.False);
            Assert.That(bootstrap.activeSelf, Is.True);
            Assert.That(bootstrapChild.activeInHierarchy, Is.True);
            Assert.That(bootstrapProbe.AwakeCount, Is.EqualTo(1));
            Assert.That(asyncWaitRoot.activeSelf, Is.False);
            Assert.That(waitingConsumer.AwakeCount, Is.EqualTo(0));
            Assert.That(sibling.activeSelf, Is.True);
            Assert.That(siblingConsumer.Service, Is.Not.Null);
            Assert.That(siblingConsumer.AwakenedWithService, Is.True);

            gateService.Complete();
            yield return new WaitUntil(() => context.IsReady);
            yield return null;

            Assert.That(asyncWaitRoot.activeSelf, Is.True);
            Assert.That(waitingConsumer.Service, Is.Not.Null);
            Assert.That(waitingConsumer.AwakenedWithService, Is.True);
            Assert.That(sibling.activeSelf, Is.True);
            Assert.That(siblingConsumer.Service, Is.Not.Null);

            Object.Destroy(sibling);
            Object.Destroy(bootstrap);
        }

        [UnityTest]
        public IEnumerator SceneContext_WithRequiredAsyncServices_AndInactiveAsyncWaitRoot_KeepsItInactiveAfterReady()
        {
            GameObject bootstrap = new("Bootstrap");
            bootstrap.SetActive(false);

            UDIContext context = bootstrap.AddComponent<UDIContext>();
            AsyncInstaller installer = bootstrap.AddComponent<AsyncInstaller>();
            AsyncGateService gateService = new();
            installer.Service = gateService;

            GameObject asyncWaitRoot = new("AsyncWaitRoot");
            asyncWaitRoot.transform.SetParent(bootstrap.transform);
            asyncWaitRoot.SetActive(false);
            AsyncConsumer waitingConsumer = asyncWaitRoot.AddComponent<AsyncConsumer>();

            SetAsyncWaitRoot(context, asyncWaitRoot);

            bootstrap.SetActive(true);
            yield return null;

            Assert.That(context.IsReady, Is.False);
            Assert.That(asyncWaitRoot.activeSelf, Is.False);
            Assert.That(waitingConsumer.AwakeCount, Is.EqualTo(0));

            gateService.Complete();
            yield return new WaitUntil(() => context.IsReady);
            yield return null;

            Assert.That(asyncWaitRoot.activeSelf, Is.False);
            Assert.That(waitingConsumer.Service, Is.Not.Null);
            Assert.That(waitingConsumer.AwakeCount, Is.EqualTo(0));

            Object.Destroy(bootstrap);
        }

        [UnityTest]
        public IEnumerator SceneContext_WithoutRequiredAsyncServices_DoesNotDelayAsyncWaitRoot()
        {
            GameObject bootstrap = new("Bootstrap");
            bootstrap.SetActive(false);

            UDIContext context = bootstrap.AddComponent<UDIContext>();
            MarkerInstaller installer = bootstrap.AddComponent<MarkerInstaller>();
            installer.Service = new MarkerService("local");

            GameObject asyncWaitRoot = new("AsyncWaitRoot");
            asyncWaitRoot.transform.SetParent(bootstrap.transform);
            MarkerConsumer consumer = asyncWaitRoot.AddComponent<MarkerConsumer>();

            SetAsyncWaitRoot(context, asyncWaitRoot);

            bootstrap.SetActive(true);
            yield return null;
            yield return new WaitUntil(() => context.IsReady);
            yield return null;

            Assert.That(bootstrap.activeSelf, Is.True);
            Assert.That(asyncWaitRoot.activeSelf, Is.True);
            Assert.That(consumer.Service, Is.Not.Null);
            Assert.That(consumer.AwakenedWithService, Is.True);

            Object.Destroy(bootstrap);
        }

        [UnityTest]
        public IEnumerator SceneContext_WithRequiredAsyncServices_AndMissingAsyncWaitRoot_LogsErrorAndDoesNotInitialize()
        {
            GameObject bootstrap = new("Bootstrap");
            bootstrap.SetActive(false);

            UDIContext context = bootstrap.AddComponent<UDIContext>();
            AsyncInstaller installer = bootstrap.AddComponent<AsyncInstaller>();
            installer.Service = new AsyncGateService();

            LogAssert.Expect(LogType.Error, new Regex("AsyncWaitRoot", RegexOptions.IgnoreCase));

            bootstrap.SetActive(true);
            yield return null;

            Assert.That(context.IsReady, Is.False);
            Assert.That(context.InitializationException, Is.Not.Null);

            Object.Destroy(bootstrap);
        }

        [UnityTest]
        public IEnumerator SceneContext_WithExternalInstaller_LogsWarningAndContinues()
        {
            GameObject bootstrap = new("Bootstrap");
            bootstrap.SetActive(false);

            UDIContext context = bootstrap.AddComponent<UDIContext>();
            AsyncInstaller installer = bootstrap.AddComponent<AsyncInstaller>();
            AsyncGateService gateService = new();
            installer.Service = gateService;

            GameObject asyncWaitRoot = new("AsyncWaitRoot");
            asyncWaitRoot.transform.SetParent(bootstrap.transform);
            AsyncConsumer waitingConsumer = asyncWaitRoot.AddComponent<AsyncConsumer>();
            SetAsyncWaitRoot(context, asyncWaitRoot);

            GameObject externalInstallerRoot = new("ExternalInstallerRoot");
            externalInstallerRoot.SetActive(false);
            WarningInstaller externalInstaller = externalInstallerRoot.AddComponent<WarningInstaller>();
            externalInstaller.Service = new MarkerService("warn");

            LogAssert.Expect(LogType.Warning, new Regex("AsyncWaitRoot", RegexOptions.IgnoreCase));

            bootstrap.SetActive(true);
            externalInstallerRoot.SetActive(true);
            yield return null;

            Assert.That(context.IsReady, Is.False);
            Assert.That(externalInstaller.GetComponent<UDIContext>(), Is.Null);
            Assert.That(waitingConsumer.AwakeCount, Is.EqualTo(0));

            gateService.Complete();
            yield return new WaitUntil(() => context.IsReady);
            yield return null;

            Assert.That(waitingConsumer.Service, Is.Not.Null);

            Object.Destroy(externalInstallerRoot);
            Object.Destroy(bootstrap);
        }

        [UnityTest]
        public IEnumerator SceneContext_WithMultipleContexts_LogsErrorAndDoesNotInitialize()
        {
            GameObject firstRoot = new("FirstContextRoot");
            firstRoot.SetActive(false);
            UDIContext firstContext = firstRoot.AddComponent<UDIContext>();
            MarkerInstaller firstInstaller = firstRoot.AddComponent<MarkerInstaller>();
            firstInstaller.Service = new MarkerService("first");

            GameObject secondRoot = new("SecondContextRoot");
            secondRoot.SetActive(false);
            UDIContext secondContext = secondRoot.AddComponent<UDIContext>();
            MarkerInstaller secondInstaller = secondRoot.AddComponent<MarkerInstaller>();
            secondInstaller.Service = new MarkerService("second");

            LogAssert.Expect(LogType.Error, new Regex("UDIContext.*multiple.*scene|scene.*multiple.*UDIContext", RegexOptions.IgnoreCase));
            LogAssert.Expect(LogType.Error, new Regex("UDIContext.*multiple.*scene|scene.*multiple.*UDIContext", RegexOptions.IgnoreCase));

            firstRoot.SetActive(true);
            secondRoot.SetActive(true);
            yield return null;

            Assert.That(firstContext.IsReady, Is.False);
            Assert.That(secondContext.IsReady, Is.False);
            Assert.That(firstContext.InitializationException, Is.Not.Null);
            Assert.That(secondContext.InitializationException, Is.Not.Null);

            Object.Destroy(firstRoot);
            Object.Destroy(secondRoot);
        }

        [UnityTest]
        public IEnumerator GlobalRuntime_WithoutSceneContext_InjectsRootObjects()
        {
            GameObject globalHost = CreateGlobalContainerHost(new MarkerService("global"));

            GameObject root = new("GlobalConsumerRoot");
            root.SetActive(false);
            MarkerConsumer consumer = root.AddComponent<MarkerConsumer>();

            root.SetActive(true);
            InvokeGlobalSceneInjection(root.scene);
            yield return null;

            Assert.That(consumer.Service, Is.Not.Null);
            Assert.That(consumer.Service.Source, Is.EqualTo("global"));

            Object.Destroy(root);
            Object.Destroy(globalHost);
        }

        [UnityTest]
        public IEnumerator Factory_UsesNearestLocalContextForInstantiation()
        {
            GameObject root = new("LocalContextRoot");
            root.SetActive(false);

            UDIContext context = root.AddComponent<UDIContext>();
            LocalInstaller installer = root.AddComponent<LocalInstaller>();
            installer.Service = new LocalMarkerService();

            root.SetActive(true);
            yield return new WaitUntil(() => context.IsReady);

            GameObject parent = new("Parent");
            parent.transform.SetParent(root.transform);

            GameObject prefab = new("InjectedPrefab");
            prefab.AddComponent<LocalConsumer>();

            GameObject instance = UGameObjectFactory.InstantiateWithDependency(prefab, parent.transform);
            LocalConsumer consumer = instance.GetComponent<LocalConsumer>();

            Assert.That(consumer.Service, Is.Not.Null);
            Assert.That(consumer.AwakenedWithService, Is.True);

            Object.Destroy(prefab);
            Object.Destroy(root);
        }

        private static GameObject CreateGlobalContainerHost(MarkerService service)
        {
            GameObject host = new("GlobalHost");
            LifecycleManager lifecycleManager = host.AddComponent<LifecycleManager>();
            UDIContainer container = new(null, lifecycleManager, allowGlobalBindings: true);
            container.Bind<MarkerService>()
                .FromInstance(service)
                .AsSingle()
                .AsGlobal();
            container.FinalizeBindings();

            SetGlobalRuntime(container);
            return host;
        }

        private static void SetGlobalRuntime(UDIContainer container)
        {
            System.Type runtimeType = typeof(UDIContext).Assembly.GetType("UTools.UDIGlobalRuntime");
            runtimeType?.GetField("_bootstrapAttempted", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, true);
            runtimeType?.GetField("_container", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, container);
            runtimeType?.GetField("_readyTask", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, Task.CompletedTask);
            runtimeType?.GetField("_initializationException", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
            runtimeType?.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
        }

        private static void ResetGlobalRuntime()
        {
            System.Type runtimeType = typeof(UDIContext).Assembly.GetType("UTools.UDIGlobalRuntime");
            runtimeType?.GetField("_bootstrapAttempted", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, false);
            runtimeType?.GetField("_container", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
            runtimeType?.GetField("_readyTask", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
            runtimeType?.GetField("_initializationException", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
            runtimeType?.GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)?.SetValue(null, null);
        }

        private static void ResetFactoryContainer()
        {
            typeof(UGameObjectFactory)
                .GetField("_container", BindingFlags.Static | BindingFlags.NonPublic)
                ?.SetValue(null, null);
        }

        private static void SetAsyncWaitRoot(UDIContext context, GameObject asyncWaitRoot)
        {
            FieldInfo field = typeof(UDIContext).GetField("_asyncWaitRoot", BindingFlags.Instance | BindingFlags.NonPublic);
            Assert.That(field, Is.Not.Null, "UDIContext is expected to expose a serialized _asyncWaitRoot field.");
            field.SetValue(context, asyncWaitRoot);
        }

        private static void InvokeGlobalSceneInjection(UnityEngine.SceneManagement.Scene scene)
        {
            System.Type runtimeType = typeof(UDIContext).Assembly.GetType("UTools.UDIGlobalRuntime");
            runtimeType?.GetMethod("InjectScene", BindingFlags.Static | BindingFlags.NonPublic)
                ?.Invoke(null, new object[] { scene });
        }

        private sealed class AsyncInstaller : MonoInstaller
        {
            public AsyncGateService Service { get; set; }

            public override void InstallBindings(UDIContainer container)
            {
                container.Bind<AsyncGateService>()
                    .FromInstance(Service)
                    .AsSingle()
                    .RequiredForContextStart();
            }
        }

        private sealed class LocalInstaller : MonoInstaller
        {
            public LocalMarkerService Service { get; set; }

            public override void InstallBindings(UDIContainer container)
            {
                container.Bind<LocalMarkerService>()
                    .FromInstance(Service)
                    .AsSingle();
            }
        }

        private sealed class AsyncGateService : IAsyncInitializable
        {
            private readonly TaskCompletionSource<bool> _ready = new();

            public Task InitializeAsync(CancellationToken cancellationToken)
            {
                cancellationToken.Register(() => _ready.TrySetCanceled(cancellationToken));
                return _ready.Task;
            }

            public void Complete()
            {
                _ready.TrySetResult(true);
            }
        }

        private sealed class LocalMarkerService
        {
        }

        private sealed class MarkerService
        {
            public MarkerService(string source)
            {
                Source = source;
            }

            public string Source { get; }
        }

        private sealed class AsyncConsumer : MonoBehaviour
        {
            [Inject] public AsyncGateService Service;

            public int AwakeCount { get; private set; }
            public bool AwakenedWithService { get; private set; }

            private void Awake()
            {
                AwakeCount++;
                AwakenedWithService = Service != null;
            }
        }

        private sealed class ActivationProbe : MonoBehaviour
        {
            public int AwakeCount { get; private set; }

            private void Awake()
            {
                AwakeCount++;
            }
        }

        private sealed class LocalConsumer : MonoBehaviour
        {
            [Inject] public LocalMarkerService Service;

            public bool AwakenedWithService { get; private set; }

            private void Awake()
            {
                AwakenedWithService = Service != null;
            }
        }

        private sealed class MarkerInstaller : MonoInstaller
        {
            public MarkerService Service { get; set; }

            public override void InstallBindings(UDIContainer container)
            {
                container.Bind<MarkerService>()
                    .FromInstance(Service)
                    .AsSingle();
            }
        }

        private sealed class WarningInstaller : MonoInstaller
        {
            public MarkerService Service { get; set; }

            public override void InstallBindings(UDIContainer container)
            {
                container.Bind<MarkerService>()
                    .FromInstance(Service)
                    .AsSingle();
            }
        }

        private sealed class MarkerConsumer : MonoBehaviour
        {
            [Inject] public MarkerService Service;

            public bool AwakenedWithService { get; private set; }

            private void Awake()
            {
                AwakenedWithService = Service != null;
            }
        }
    }
}
