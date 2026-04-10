# UTools

UTools is a lightweight Unity toolkit focused on four areas:

- `UDI`: field/property/method injection and runtime lifecycle hooks
- `UMessage`: typed publish/subscribe with pending replay support
- `UFind`: attribute-driven child/component/resource lookup
- `UUtils`: Unity-oriented helper extensions

## Recommended Usage

1. Add exactly one explicit `UDIContext` to the scene.
2. Attach `MonoInstaller` or `ScriptableObjectInstaller` instances.
3. Register services through `container.Bind<T>()`.
4. Use `[Inject]`, `[Child]`, `[Comp]`, and `[Resource]` in runtime scripts.

## Global Injection

- Create a single `GlobalInstaller` asset inside a `Resources` folder.
- Register cross-scene fallback services with `.AsGlobal()`.
- Global services are available in scenes without a `UDIContext`.
- If a scene has a single `UDIContext`, local bindings override global ones for the entire scene.
- If a scene contains multiple `UDIContext` components, initialization fails instead of partially injecting objects.

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
- Keep `UDIContext` and installers on a dedicated bootstrap object.
- When a scene uses a single `UDIContext`, other active scene roots are suspended until required async services are ready.
- Put camera, lighting, loading UI, and installers under the bootstrap root if they must remain active during async startup.
- Put gameplay/content in separate scene roots when they should stay suspended until startup completes.
- If async initialization fails, the context stays not-ready and suspended scene roots are not restored.

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
- Constructor injection is not supported.
- Identifier-based resolution is not part of the recommended API surface.

## Testing

Use the Unity Test Runner for:

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`
