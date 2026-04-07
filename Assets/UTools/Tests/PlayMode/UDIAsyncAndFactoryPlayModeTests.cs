using System.Collections;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace UTools.Tests
{
    public class UDIAsyncAndFactoryPlayModeTests
    {
        [UnityTest]
        public IEnumerator RequiredAsyncServices_ActivateManagedContentRootAfterInitialization()
        {
            GameObject root = new("ContextRoot");
            root.SetActive(false);

            GameObject contentRoot = new("ContentRoot");
            contentRoot.transform.SetParent(root.transform);
            contentRoot.SetActive(false);

            AsyncConsumer consumer = contentRoot.AddComponent<AsyncConsumer>();
            UDIContext context = root.AddComponent<UDIContext>();
            SetManagedContentRoot(context, contentRoot);

            AsyncGateService gateService = new();
            AsyncInstaller installer = root.AddComponent<AsyncInstaller>();
            installer.Service = gateService;

            root.SetActive(true);
            yield return null;

            Assert.That(context.IsReady, Is.False);
            Assert.That(contentRoot.activeSelf, Is.False);
            Assert.That(consumer.AwakeCount, Is.EqualTo(0));

            gateService.Complete();
            yield return new WaitUntil(() => context.IsReady);
            yield return null;

            Assert.That(contentRoot.activeSelf, Is.True);
            Assert.That(consumer.Service, Is.Not.Null);
            Assert.That(consumer.AwakenedWithService, Is.True);

            Object.Destroy(root);
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

        private static void SetManagedContentRoot(UDIContext context, GameObject managedContentRoot)
        {
            typeof(UDIContext)
                .GetField("_managedContentRoot", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.SetValue(context, managedContentRoot);
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

        private sealed class LocalConsumer : MonoBehaviour
        {
            [Inject] public LocalMarkerService Service;

            public bool AwakenedWithService { get; private set; }

            private void Awake()
            {
                AwakenedWithService = Service != null;
            }
        }
    }
}
