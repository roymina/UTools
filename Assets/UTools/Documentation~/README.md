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
5. Use `[Children]` when you need a `List<T>` from a parent object's child hierarchy.

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
- Assign `AsyncWaitRoot` when required async services should delay only one subtree.
- `AsyncWaitRoot` and its descendants are injected only after required async services finish initializing.
- Objects outside `AsyncWaitRoot` are injected immediately and keep running; they must not assume async-ready services are fully initialized.
- If required async bindings exist and `AsyncWaitRoot` is not assigned, the context fails initialization with an error.
- If async initialization fails, the context stays not-ready and `AsyncWaitRoot` is not reactivated.

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

## UFind

- `[Comp]` binds a component on the same GameObject
- `[Child]` binds a single child GameObject or a component on a child
- `[Children]` binds a `List<GameObject>` or `List<TComponent>` from a parent object's children
- `[Resource]` loads from `Resources`

```csharp
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UTools;

public sealed class LobbyPanel : UBehaviour
{
    [Children] public List<GameObject> SlotGroup;
    [Children("SlotGroup")] public List<Button> SlotButtons;
    [Children(parentName = "SlotGroup", includeDecendents = true, includeInactive = false)]
    public List<GameObject> ActiveSlotNodes;
}
```

- If `[Children]` has no `parentName`, the field name is used to find the parent object.
- Example: `[Children] public List<GameObject> SlotGroup;` looks for a parent object named `SlotGroup`, then returns its children.
- Example: `[Children("SlotGrid")] public List<Button> SlotButtons;` looks for the parent object `SlotGrid`, then collects `Button` components from its children.
- `[Children]` defaults to first-level children only (`includeDecendents = false`).
- `[Children]` includes inactive children by default (`includeInactive = true`).
- Component lists only keep children that actually contain the requested component.
- Errors are emitted per field, so one missing parent lookup does not stop another `[Children]` field from binding successfully.

## Testing

Use the Unity Test Runner for:

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`
