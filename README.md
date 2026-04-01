[中文](https://github.com/roymina/UTools/blob/main/README_cn.md)

# UTools

UTools is a lightweight Unity toolkit for dependency injection, messaging, object lookup, and everyday runtime helpers. It is designed to stay small, usable in game projects, and easy to extract into a reusable package.

## Modules

### UDI

- Field, property, and method injection via `[Inject]`
- Scene-level and hierarchical containers via `UDIContext`
- Runtime lifecycle hooks: `IInitializable`, `ITickable`, `ILateTickable`, `IFixedTickable`, `IUDisposable`, `IPausable`
- `NonLazy()` support
- `TryResolve<T>()` / `TryResolve(Type, out object)` support
- `MonoBehaviour` resolution and injection for scene objects and instantiated prefabs

Notes:

- Constructor injection is not supported.
- `UDIContext` is the supported DI entry point.
- Identifier-based resolution is not part of the current API surface.

### UMessage

- Typed publish/subscribe API
- Disposable subscription handle support
- Pending-message replay for late subscribers
- Automatic cleanup for destroyed `UnityEngine.Object` subscriber targets

### UFind

- `[Child]` child-object lookup
- `[Comp]` component lookup on self
- `[Resource]` `Resources.Load` binding
- Reflection metadata caching to reduce repeated startup scanning
- Path-based child lookup support such as `[Child("Root/Panel/Button")]`

### UUtils

- String, time, file, GameObject, mesh, color, and UI utility extensions
- Persistent-data helpers now create missing directories automatically

### Editor Helpers

- `ButtonAttribute`
- `ShowIfAttribute`
- `AutoComponentAttribute`

## Installation

### Option 1: Unity package / source drop-in

Import the project content under `Assets/UTools` into your Unity project.

### Option 2: `unitypackage`

Use the packaged release artifact if you want a simpler import flow.

### Option 3: Local package workflow

This repository now includes package-oriented metadata under `Assets/UTools`, including:

- `package.json`
- `Documentation~/`
- `Samples~/`
- `CHANGELOG.md`
- `asmdef` files for runtime, editor, examples, and tests

## Recommended Setup

The recommended entry point is `UDIContext`.

1. Add a `UDIContext` to a scene root object.
2. Add one or more `MonoInstaller` or `ScriptableObjectInstaller` instances.
3. Register services in `InstallBindings`.
4. Use `[Inject]` in runtime classes that should receive services.

```csharp
using UTools;
using UnityEngine;

public interface ILogger
{
    void Log(string message);
}

public class UnityLogger : ILogger
{
    public void Log(string message) => Debug.Log(message);
}

public class GameInstaller : MonoInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<ILogger>()
            .To<UnityLogger>()
            .AsSingle();

        container.Bind<GameManager>()
            .ToSelf()
            .AsSingle()
            .NonLazy();
    }
}
```

```csharp
using UTools;

public class GameManager : IInitializable, ITickable
{
    [Inject] private ILogger _logger;

    public void Initialize()
    {
        _logger.Log("GameManager initialized");
    }

    public void Tick()
    {
    }
}
```

## Global Context Pattern

If you want cross-scene services, keep the context object alive:

```csharp
using UnityEngine;
using UTools;

public class ProjectContext : UDIContext
{
    protected override void Awake()
    {
        if (FindObjectsByType<ProjectContext>(FindObjectsSortMode.None).Length > 1)
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
        base.Awake();
    }
}
```

Child contexts can inherit from parent containers through the scene hierarchy.

## Installer Example

Use a `MonoInstaller` under a `UDIContext`:

```csharp
using UTools;

public class ExampleInstaller : MonoInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<LegacyService>()
            .AsSingle();
    }
}
```

## Messaging Example

```csharp
using UTools;

public class ExampleUsage
{
    private IMessageSubscription _subscription;

    public void Enable()
    {
        _subscription = UMessageCenter.Instance.Subscribe<MyMessage>(OnMessage);
    }

    public void Disable()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    public void Publish()
    {
        UMessageCenter.Instance.Publish(new MyMessage { Text = "Hello" });
    }

    private void OnMessage(MyMessage message)
    {
        UnityEngine.Debug.Log(message.Text);
    }
}

public class MyMessage
{
    public string Text;
}
```

If a message is published before there are subscribers, the first later subscriber can still replay the pending message by default.

## UFind Example

```csharp
using TMPro;
using UnityEngine;
using UTools;

public class ExampleView : UBehaviour
{
    [Comp] public Canvas Canvas;
    [Child] public TextMeshProUGUI Title;
    [Child("Root/Buttons/Confirm")] public GameObject ConfirmButton;
    [Resource("Icons/Logo")] public Sprite Logo;
}
```

Behavior notes:

- If `[Child]` has no argument, the field name is used.
- If duplicate child names exist, UTools logs an error and asks for a path-based lookup.

## Instantiation With Injection

Use `UGameObjectFactory` if you need injection on instantiated prefabs:

```csharp
var instance = UGameObjectFactory.InstantiateWithDependency(prefab, parentTransform);
```

This injects the prefab root and all child `MonoBehaviour` components.

## Testing

The project now includes Unity Test Framework entry points:

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`

Current automated validation in this repository:

- `dotnet build utools.sln -nologo`
- Unity EditMode / PlayMode tests through the Unity Test Runner

## Repository Structure

- `Assets/UTools/Scripts`: runtime and editor source
- `Assets/UTools/Example`: sample scenes and scripts
- `Assets/UTools/Tests`: EditMode and PlayMode tests
- `Assets/UTools/Documentation~`: package documentation
- `Assets/UTools/Samples~`: package sample placeholders

## Status

UTools has been refactored toward a reusable package structure, but there are still compatibility shims for older scenes and examples. The recommended path for new development is:

- `UDIContext`
- `MonoInstaller` / `ScriptableObjectInstaller`
- typed `UMessageCenter` subscriptions with disposable handles
