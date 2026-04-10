using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using UnityEngine;

namespace UTools.Tests
{
    public class UDIGlobalAndAsyncEditModeTests
    {
        [Test]
        public void UDIContext_DoesNotExposeManagedContentRootApi()
        {
            Assert.That(typeof(UDIContext).GetField("_managedContentRoot", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic), Is.Null);
            Assert.That(typeof(UDIContext).GetProperty("ManagedContentRoot"), Is.Null);
        }

        [Test]
        public void AsGlobal_ThrowsWhenBindingIsNotFromGlobalContainer()
        {
            GameObject host = new("LocalHost");
            LifecycleManager lifecycleManager = host.AddComponent<LifecycleManager>();
            UDIContainer container = new(null, lifecycleManager);

            InvalidOperationException exception = Assert.Throws<InvalidOperationException>(() =>
                container.Bind<ITestService>()
                    .To<GlobalTestService>()
                    .AsSingle()
                    .AsGlobal());

            Assert.That(exception.Message, Does.Contain("AsGlobal"));

            UnityEngine.Object.DestroyImmediate(host);
        }

        [Test]
        public void Resolve_PrefersLocalBindingAndFallsBackToGlobal()
        {
            GameObject host = new("GlobalHost");
            LifecycleManager lifecycleManager = host.AddComponent<LifecycleManager>();
            UDIContainer globalContainer = new(null, lifecycleManager, allowGlobalBindings: true);
            globalContainer.Bind<ITestService>()
                .To<GlobalTestService>()
                .AsSingle()
                .AsGlobal();

            UDIContainer localContainer = new(globalContainer);
            localContainer.Bind<ITestService>()
                .To<LocalTestService>()
                .AsSingle();

            ITestService localResolved = localContainer.Resolve<ITestService>();
            ITestService globalResolved = new UDIContainer(globalContainer).Resolve<ITestService>();

            Assert.That(localResolved, Is.TypeOf<LocalTestService>());
            Assert.That(globalResolved, Is.TypeOf<GlobalTestService>());

            UnityEngine.Object.DestroyImmediate(host);
        }

        [Test]
        public async Task InitializeRequiredForContextStartAsync_RunsRequiredServicesInRegistrationOrder()
        {
            List<string> calls = new();
            GameObject host = new("AsyncHost");
            LifecycleManager lifecycleManager = host.AddComponent<LifecycleManager>();
            UDIContainer container = new(null, lifecycleManager);

            container.Bind<FirstAsyncService>()
                .FromInstance(new FirstAsyncService(calls))
                .AsSingle()
                .RequiredForContextStart();

            container.Bind<SecondAsyncService>()
                .FromInstance(new SecondAsyncService(calls))
                .AsSingle()
                .RequiredForContextStart();

            await container.InitializeRequiredForContextStartAsync(CancellationToken.None);

            CollectionAssert.AreEqual(new[] { "first", "second" }, calls);

            UnityEngine.Object.DestroyImmediate(host);
        }

        private interface ITestService
        {
        }

        private sealed class GlobalTestService : ITestService
        {
        }

        private sealed class LocalTestService : ITestService
        {
        }

        private sealed class FirstAsyncService : IAsyncInitializable
        {
            private readonly List<string> _calls;

            public FirstAsyncService(List<string> calls)
            {
                _calls = calls;
            }

            public Task InitializeAsync(CancellationToken cancellationToken)
            {
                _calls.Add("first");
                return Task.CompletedTask;
            }
        }

        private sealed class SecondAsyncService : IAsyncInitializable
        {
            private readonly List<string> _calls;

            public SecondAsyncService(List<string> calls)
            {
                _calls = calls;
            }

            public Task InitializeAsync(CancellationToken cancellationToken)
            {
                _calls.Add("second");
                return Task.CompletedTask;
            }
        }
    }
}
