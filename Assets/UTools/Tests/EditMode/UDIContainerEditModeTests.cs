using NUnit.Framework;
using UnityEngine;

namespace UTools.Tests
{
    public class UDIContainerEditModeTests
    {
        [Test]
        public void ResolveAfterLifecycleInitialize_InitializesServiceExactlyOnce()
        {
            GameObject host = new("LifecycleHost");
            LifecycleManager lifecycleManager = host.AddComponent<LifecycleManager>();
            UDIContainer container = new(null, lifecycleManager);

            lifecycleManager.Initialize();
            TestInitializable service = container.Resolve<TestInitializable>();

            Assert.That(service.InitializeCount, Is.EqualTo(1));

            Object.DestroyImmediate(host);
        }

        [Test]
        public void NonLazyBinding_CreatesAndCachesSingleton()
        {
            GameObject host = new("LifecycleHost");
            LifecycleManager lifecycleManager = host.AddComponent<LifecycleManager>();
            UDIContainer container = new(null, lifecycleManager);

            container.Bind<TestInitializable>()
                .AsSingle()
                .NonLazy();

            TestInitializable first = container.Resolve<TestInitializable>();
            TestInitializable second = container.Resolve<TestInitializable>();

            Assert.That(first, Is.SameAs(second));

            Object.DestroyImmediate(host);
        }

        [Test]
        public void TryResolveForUnboundInterface_ReturnsFalse()
        {
            UDIContainer container = new();

            bool resolved = container.TryResolve(typeof(ITestContract), out object instance);

            Assert.That(resolved, Is.False);
            Assert.That(instance, Is.Null);
        }

        private interface ITestContract
        {
        }

        private sealed class TestInitializable : IInitializable
        {
            public int InitializeCount { get; private set; }

            public void Initialize()
            {
                InitializeCount++;
            }
        }
    }
}
