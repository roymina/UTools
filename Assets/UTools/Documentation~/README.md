# UTools

UTools is a lightweight Unity toolkit focused on four areas:

- `UDI`: field/property/method injection and runtime lifecycle hooks
- `UMessage`: typed publish/subscribe with pending replay support
- `UFind`: attribute-driven child/component/resource lookup
- `UUtils`: Unity-oriented helper extensions

## Recommended Usage

1. Add a `UDIContext` to a scene root.
2. Attach `MonoInstaller` or `ScriptableObjectInstaller` instances.
3. Register services through `container.Bind<T>()`.
4. Use `[Inject]`, `[Child]`, `[Comp]`, and `[Resource]` in runtime scripts.

## Global Injection

- Create a single `GlobalInstaller` asset inside a `Resources` folder.
- Register cross-scene fallback services with `.AsGlobal()`.
- Global services are available in scenes without a `UDIContext`.
- If a scene root has its own `UDIContext`, local bindings override global ones.

```csharp
using UTools;
using UnityEngine;

[CreateAssetMenu(menuName = "UTools/Global Installer")]
public sealed class GameGlobalInstaller : GlobalInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<IClock>()
            .To<SystemClock>()
            .AsSingle()
            .AsGlobal();
    }
}
```

## Async Readiness

- Services that must finish loading before a context starts can implement `IAsyncInitializable`.
- Mark those bindings with `.RequiredForContextStart()`.
- Assign `ManagedContentRoot` on `UDIContext` to delay activating scene content until required async services are ready.
- If async initialization fails, the context stays not-ready and keeps `ManagedContentRoot` inactive.

```csharp
using System.Threading;
using System.Threading.Tasks;
using UTools;

public sealed class ConfigService : IAsyncInitializable
{
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return LoadConfigAsync(cancellationToken);
    }

    private Task LoadConfigAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

## Compatibility Notes

- `UDIContext` is the only recommended DI entry point.
- `GlobalInstaller` assets must live under `Resources`.
- `ManagedContentRoot` should start inactive when a context depends on required async services.
- Constructor injection is not supported.
- Identifier-based resolution is not part of the recommended API surface.

## Testing

Use the Unity Test Runner for:

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`
