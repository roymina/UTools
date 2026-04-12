[中文](README_cn.md)

# UTools

![Unity](https://img.shields.io/badge/Unity-2023.2%2B-000000?logo=unity&logoColor=white)
![UPM](https://img.shields.io/badge/UPM-Git%20URL-blue)
![Version](https://img.shields.io/badge/version-1.0.0-blue)
![C#](https://img.shields.io/badge/C%23-Unity-239120?logo=csharp&logoColor=white)
![Tests](https://img.shields.io/badge/tests-EditMode%20%7C%20PlayMode-brightgreen)

UTools is a lightweight Unity toolkit for dependency injection, UI/object lookup, message dispatching, and common runtime helpers.

| Module | What it is for |
| --- | --- |
| `UDI` | Scene/global dependency injection, lifecycle callbacks, async startup gates |
| `UFind` | Attribute-based component, child, children-list, and `Resources` binding |
| `UMessage` | Typed publish/subscribe messages with disposable subscriptions |
| `UUtils` | String, time, file, GameObject, UI, texture, and mesh helpers |
| Editor helpers | Small Inspector attributes such as buttons, conditional fields, and component toggles |

## Installation

### Git URL / UPM

Open `Window > Package Manager`, choose `Add package from git URL...`, then use:

```text
https://github.com/roymina/UTools.git?path=/Assets/UTools
```

This repository is still a normal Unity project. `Assets/UTools` is the package root exposed to UPM.

`TextMeshPro` is declared through `com.unity.textmeshpro`. If sample UI assets miss TMP resources, run:

```text
Window > TextMeshPro > Import TMP Essential Resources
```

### `unitypackage`

If you prefer manual import, use the packaged release artifact in `Releases/`.

## Quick Start

1. Install the package.
2. Add exactly one `UDIContext` to a scene bootstrap GameObject.
3. Add a `MonoInstaller` to the same bootstrap object, or assign installers into the `UDIContext` lists.
4. Register services in `InstallBindings`.
5. Use `[Inject]` on scene components and use `UGameObjectFactory` for injected runtime prefabs.

```csharp
using UnityEngine;
using UTools;

public interface IGameClock
{
    float Time { get; }
}

public sealed class UnityGameClock : IGameClock
{
    public float Time => UnityEngine.Time.time;
}

public sealed class GameInstaller : MonoInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<IGameClock>()
            .To<UnityGameClock>()
            .AsSingle();
    }
}

public sealed class ClockLabel : MonoBehaviour
{
    [Inject] private IGameClock _clock;

    private void Start()
    {
        Debug.Log($"Injected time: {_clock.Time}");
    }
}
```

## UDI

`UDI` is the dependency injection module. Use it to prepare services, inject scene objects, inject runtime prefabs, and run lifecycle interfaces from a single context.

### Three Installer Types

- `GlobalInstaller`: prepares **global fallback services for the whole game**.
- `MonoInstaller`: prepares **scene-specific services for the current scene**.
- `ScriptableObjectInstaller`: prepares **reusable asset-based config/services for the current scene**.

### `UDIContext`

Use `UDIContext` as the scene DI entry point.

1. Create a dedicated `Bootstrap` GameObject.
2. Add `UDIContext`.
3. Add `MonoInstaller` components to the same GameObject, or assign them to `UDIContext` > `Installers`.
4. Assign `ScriptableObjectInstaller` assets to `UDIContext` > `Scriptable Object Installers`.
5. If any binding uses `.RequiredForContextStart()`, assign `Async Wait Root`.

Notes:

- Keep exactly one `UDIContext` in a scene. Multiple contexts cause initialization to fail.
- Keep `UDIContext` and startup installers on a dedicated bootstrap object. Gameplay consumers can live anywhere in the scene.
- A `MonoInstaller` is executed only when it is on the same GameObject as `UDIContext` or explicitly assigned into the `Installers` list.
- A standalone `MonoInstaller` can auto-create a context when none exists, but explicit setup is safer and clearer.
- `UDIContext.IsReady` tells whether startup completed; `ReadyTask` can be awaited by code that must wait manually.
- If initialization throws, `InitializationException` is set and the context stays not ready.

### `MonoInstaller`

Use `MonoInstaller` when bindings need scene references such as transforms, prefabs, cameras, or serialized components.

```csharp
using UnityEngine;
using UTools;

public sealed class GameplayInstaller : MonoInstaller
{
    [SerializeField] private Transform spawnRoot;
    [SerializeField] private EnemyView enemyPrefab;

    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<Transform>()
            .FromInstance(spawnRoot)
            .AsSingle();

        container.Bind<EnemyService>()
            .ToSelf()
            .AsSingle()
            .NonLazy();

        container.Bind<IFactory<EnemyView>>()
            .FromInstance(new PrefabFactory<EnemyView>(enemyPrefab, container, spawnRoot))
            .AsSingle();
    }
}
```

Notes:

- Put `.NonLazy()` at the end of the binding chain because it creates/finalizes the binding immediately.
- Prefer `.FromInstance(...)` for serialized scene references.
- Use `.FromGameObject(host)` for `MonoBehaviour` bindings when the component should be found or added on a specific object.
- `FromGameObject` still reuses an existing instance of that `MonoBehaviour` type if Unity finds one first.

### `ScriptableObjectInstaller`

Use `ScriptableObjectInstaller` when the same binding set should be reused by multiple scenes.

```csharp
using UnityEngine;
using UTools;

[CreateAssetMenu(menuName = "Game/Installers/Audio Installer")]
public sealed class AudioInstaller : ScriptableObjectInstaller
{
    [SerializeField] private AudioSettings settings;

    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<AudioSettings>()
            .FromInstance(settings)
            .AsSingle();

        container.Bind<AudioService>()
            .ToSelf()
            .AsSingle();
    }
}
```

Usage:

- Create the asset from Unity's `Create` menu.
- Assign it to `UDIContext` > `Scriptable Object Installers`.
- Only assets assigned in that list are executed for the current scene.
- Keep per-scene object references in `MonoInstaller`; keep reusable config/data in `ScriptableObjectInstaller`.

### `GlobalInstaller`

Use `GlobalInstaller` for fallback services shared by scenes.

```csharp
using UnityEngine;
using UTools;

[CreateAssetMenu(menuName = "UTools/Global Installer")]
public sealed class GameGlobalInstaller : GlobalInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<IClock>()
            .To<UnityClock>()
            .AsSingle()
            .AsGlobal();
    }
}
```

Usage:

- Create exactly one `GlobalInstaller` asset.
- Put the asset under a `Resources` folder, for example `Assets/Resources/GameGlobalInstaller.asset`.
- Use `.AsGlobal()` only inside a `GlobalInstaller`.
- Scenes without `UDIContext` can still receive global services.
- Scenes with one `UDIContext` use local bindings first, then fall back to global bindings.
- Do not create multiple `GlobalInstaller` assets under `Resources`; the global runtime supports only one.

### Binding API

| API | Use it for |
| --- | --- |
| `Bind<T>()` | Register `T` as both contract and concrete type |
| `Bind<TContract, TImplementation>()` | Register an interface/base type to an implementation |
| `.To<T>()` | Set the concrete implementation |
| `.ToSelf()` | Use the contract type itself as the concrete type |
| `.AsSingle()` | Reuse one instance in the current container |
| `.AsTransient()` | Create a new instance for each resolve |
| `.InScope(BindingScope.Scoped)` | Cache one instance in the current context/container |
| `.FromInstance(instance)` | Use an existing instance, usually a serialized reference |
| `.FromGameObject(gameObject)` | Resolve or add a `MonoBehaviour` on a target GameObject |
| `.NonLazy()` | Create the instance during context startup |
| `.RequiredForContextStart()` | Wait for this binding before the async startup gate opens |
| `.AsGlobal()` | Mark a global binding; only valid in `GlobalInstaller` |

Notes:

- Constructor injection is not supported.
- If a concrete non-abstract class is resolved without an explicit binding, UDI can auto-create it with a parameterless constructor.
- Prefer explicit bindings for services you rely on, especially interfaces and abstract types.
- Circular dependencies throw an error instead of resolving partially.

### Injection

Use `[Inject]` on fields, writable properties, or methods. Private members are supported.

```csharp
using UnityEngine;
using UTools;

public sealed class PlayerPresenter : MonoBehaviour
{
    [Inject] private IGameClock _clock;

    [Inject]
    public Transform SpawnRoot { get; private set; }

    private Camera _camera;

    [Inject]
    private void Construct(Camera camera)
    {
        _camera = camera;
    }

    [PostInjection]
    private void AfterInject()
    {
        Debug.Log($"Clock={_clock.Time}, Root={SpawnRoot.name}, Camera={_camera.name}");
    }
}
```

Notes:

- Injection order is fields, properties, methods, then `[PostInjection]`.
- `[Inject]` methods can have parameters; each parameter is resolved from the container.
- `[PostInjection]` runs after dependencies are assigned and can also receive resolved parameters.
- Scene objects are injected by `UDIContext`; runtime prefabs should be created with `UGameObjectFactory` or `PrefabFactory<T>`.
- UDI runs very early by default. Avoid setting consumer scripts to execute earlier than `UDIContext`.

### Lifecycle Interfaces

Implement lifecycle interfaces on services or injected objects when the context should manage them.

| Interface | When it runs |
| --- | --- |
| `IInitializable.Initialize()` | After context injection is complete |
| `IAsyncInitializable.InitializeAsync(...)` | Only awaited when the binding is marked `.RequiredForContextStart()` |
| `ITickable.Tick()` | Every `Update` while not paused |
| `IFixedTickable.FixedTick()` | Every `FixedUpdate` while not paused |
| `ILateTickable.LateTick()` | Every `LateUpdate` while not paused |
| `IUDisposable.Dispose()` | When the `LifecycleManager` is destroyed |
| `IPausable.Pause()` / `Resume()` | When `LifecycleManager.Pause()` / `Resume()` is called |

Notes:

- `LifecycleManager` is added automatically to the `UDIContext` object if missing.
- Non-lazy and resolved instances are tracked once.
- `IAsyncInitializable` is not automatically awaited unless its binding is required for context start.

### Async Injection and Waiting

Use this when a scene subtree must not wake until required async services are ready.

```csharp
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UTools;

public sealed class RemoteConfigService : IAsyncInitializable
{
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await Task.Delay(3000, cancellationToken);
    }
}

public sealed class GameInstaller : MonoInstaller
{
    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<RemoteConfigService>()
            .ToSelf()
            .AsSingle()
            .RequiredForContextStart();
    }
}
```

After this binding is registered:

- Assign `Async Wait Root` on `UDIContext`.
- On scene start, `Async Wait Root` is disabled if it was active.
- Required async services run in binding registration order.
- After they finish, UDI injects the `Async Wait Root` subtree, then restores its previous active state.
- Objects outside `Async Wait Root` are injected immediately and keep running.

Notes:

- Put gameplay nodes that must wait under `Async Wait Root`.
- Do not put the `UDIContext` object inside `Async Wait Root`.
- `Async Wait Root` must belong to the same scene as the context.
- If a required async binding exists but `Async Wait Root` is not assigned, initialization fails.
- If `Async Wait Root` was inactive before startup, it stays inactive after async initialization, but it is still injected.
- If async initialization fails or is cancelled, the context stays not ready and the wait root is not restored.

### Runtime Prefab Injection

Use `UGameObjectFactory` instead of `Object.Instantiate` for prefabs that contain `[Inject]` consumers.

```csharp
using UnityEngine;
using UTools;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnRoot;

    public GameObject Spawn(Vector3 position)
    {
        return UGameObjectFactory.InstantiateWithDependency(
            enemyPrefab,
            position,
            Quaternion.identity,
            spawnRoot);
    }
}
```

Notes:

- Prefer overloads that pass a `Transform parent` when the instance should use the nearest parent context.
- The factory injects all `MonoBehaviour` components in the created GameObject subtree.
- If no context exists, the factory falls back to a global/default container and logs a warning.

## UFind

`UFind` reduces repetitive `GetComponent`, `transform.Find`, and `Resources.Load` code.

### Basic Setup

1. Inherit from `UBehaviour`.
2. Add `[Comp]`, `[Child]`, `[Children]`, or `[Resource]` to fields.
3. If you override `Awake`, call `base.Awake()` before using bound fields.

```csharp
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UTools;

public sealed class InventoryPanel : UBehaviour
{
    [Comp] private Canvas _canvas;
    [Comp] private Button _closeButton;

    [Child] private TextMeshProUGUI Title;
    [Child("Content/Buttons/ConfirmButton")] private Button _confirmButton;
    [Child("Content/Icon")] private Image _icon;

    [Children("Content/Buttons")] private List<Button> _buttons;
    [Children(parentName = "Content/Buttons", includeDescendants = true, includeInactive = false)]
    private List<GameObject> _activeButtonNodes;

    [Resource("Icons/Inventory")] private Sprite _inventorySprite;

    protected override void Awake()
    {
        base.Awake();
        Title.text = "Inventory";
        _icon.sprite = _inventorySprite;
    }
}
```

### `[Comp]`

- Finds a component on the same GameObject.
- Works on fields only.
- Does not overwrite a field that already has a value.
- Logs a warning if the component is missing.

### `[Child]`

- With no argument, uses the field name as the child name.
- With a string argument, uses that name or path.
- If the field type is `GameObject`, assigns the child object.
- If the field type is a component, finds the child first, then gets that component from the child.
- Simple names search descendants case-insensitively.
- Path lookup uses `Transform.Find` relative to the current transform.
- If multiple descendants share a simple name, use a path such as `Root/Panel/Button`.

### `[Children]`

- Finds a parent child object, then collects its children into a list.
- Supports `List<GameObject>` and `List<TComponent>`.
- With no `parentName`, uses the field name as the parent name.
- `includeDescendants = false` by default, so only direct children are collected.
- `includeInactive = true` by default.
- Component lists keep only child objects that contain the requested component.
- Errors are reported per field; one failed binding does not stop other fields.

### `[Resource]`

- Loads an asset with `Resources.Load(path, fieldType)`.
- With no path, uses the field name as the resource path.
- Do not include the file extension in the path.
- Keep resources under any Unity `Resources` folder.

## UMessage

`UMessage` is a typed message center.

```csharp
using UnityEngine;
using UTools;

public sealed class ScoreChangedMessage
{
    public int Value;
}

public sealed class ScorePublisher : MonoBehaviour
{
    public void ReportScore(int score)
    {
        UMessageCenter.Instance.Publish(new ScoreChangedMessage { Value = score });
    }
}

public sealed class ScoreListener : MonoBehaviour
{
    private IMessageSubscription _subscription;

    private void OnEnable()
    {
        _subscription = UMessageCenter.Instance.Subscribe<ScoreChangedMessage>(OnScoreChanged);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    private void OnScoreChanged(ScoreChangedMessage message)
    {
        Debug.Log($"Score updated: {message.Value}");
    }
}
```

Usage notes:

- Use `Publish<T>(message)` to send messages.
- Use `Subscribe<T>(handler)` to listen and keep the returned `IMessageSubscription`.
- Dispose subscriptions in `OnDisable` or `OnDestroy`.
- If a message is published before any subscriber exists, it is queued and replayed to the first later subscriber.
- Use `Subscribe<T>(handler, replayPending: false)` when you do not want old messages.
- `UMessageCenter.Instance.Clear()` removes all subscribers and pending messages; it is mainly useful for tests or full resets.
- Exceptions thrown by handlers are logged and do not stop other handlers.
- Destroyed `UnityEngine.Object` subscriber targets are cleaned up automatically.

```csharp
IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<ScoreChangedMessage>(
    message => Debug.Log(message.Value),
    replayPending: false);
```

## UUtils

`UUtils` is a collection of small runtime helpers. Import `UTools` and call them as extension/static methods.

### Strings and Time

```csharp
using System;
using UnityEngine;
using UTools;

public sealed class UtilityExample : MonoBehaviour
{
    private void Start()
    {
        bool validName = "player_one".CheckUserName();
        string shortName = "VeryLongDisplayName".TrimLength(10);
        string timer = 95.ToTimeString();
        string chineseTimer = TimeSpan.FromSeconds(3661).ToHhMmSsString(useChinese: true);

        Debug.Log($"{validName}, {shortName}, {timer}, {chineseTimer}");
    }
}
```

Common methods:

- `IsNullOrEmpty()`, `IsNotNullOrEmpty()`
- `CheckUserName()`, `CheckStrChinese()`, `IsIPAddress()`
- `TrimLength(maxLength)`, `ToBase64String()`
- `ToTimeString()`, `ToHhMmSsString()`, `TryCalculateTimeSpan(...)`

### Files

```csharp
using UTools;

UFileUtilities.WriteToPersistentDataPath("{\"volume\":0.8}", "settings/user.json");
string json = UFileUtilities.ReadFromPersistentDataPath("settings/user.json");
```

Notes:

- Files are read from and written to `Application.persistentDataPath`.
- Parent folders are created automatically.
- `ReadFromPersistentDataPath` creates an empty file by default when it does not exist.

### GameObject and UI

```csharp
using UnityEngine;
using UTools;

public sealed class UiHelperExample : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private RectTransform popup;

    public void ShowInventory()
    {
        GameObject closeButton = panelRoot.FindChild("CloseButton");
        panelRoot.ToggleAllChildren(true);
        popup.ToggleAsCanvasGroup(true, useTween: false);

        closeButton?.ToggleBlink(true);
    }
}
```

Common methods:

- `FindChild(...)`, `GetAllDescendants()`, `GetDirectChildren()`
- `ShowOnlyDescendantNamed(...)`, `HideDescendantNamed(...)`
- `ToggleAllChildren(show)` toggles descendants, not the root itself
- `EnsureComponent<T>()`, `HasComponent<T>()`, `SetLayerRecursively(...)`
- `ToggleAsCanvasGroup(...)`, `ToggleAsCanvasGroupAuto(...)`, `TweenColor(...)`, `MoveOutOfScreen(...)`

Notes:

- Most helpers are null-safe and return `null`, empty collections, or `false` when input is invalid.
- UI tween helpers use a hidden persistent coroutine runner if no runner is provided.
- Pass your own `MonoBehaviour` runner when you want coroutine lifetime to follow a specific object.

### Textures and Meshes

Common methods:

- `Texture2D.ToSprite()`
- `Sprite.ToTexture2D()`
- `Texture.ToTexture2D()`
- `Texture2D.ToBase64()`
- `DecodeBase64Image(...)`
- `CloneMesh(...)`, `CombineMesh(...)`, `GenerateQuadMesh(...)`, `GeneratePolygonMesh(...)`, `GeneratePlane(...)`

Notes:

- Texture conversion creates runtime objects; destroy them when they are no longer needed.
- Mesh helpers create runtime GameObjects/meshes; manage their lifetime like other generated Unity objects.

## Editor Helpers

### `[Button]`

Creates an Inspector button for a parameterless method.

```csharp
using UnityEngine;
using UTools;

public sealed class SpawnDebugTool : MonoBehaviour
{
    [Button("Spawn Test Enemy")]
    private void SpawnTestEnemy()
    {
        Debug.Log("Spawned");
    }
}
```

Notes:

- Methods must have no parameters.
- Public and private instance methods are supported.
- With multi-object selection, the button invokes the method on every selected target.

### `[ShowIf]`

Shows a serialized field based on another serialized field.

```csharp
using UnityEngine;
using UTools;

public sealed class DamageConfig : MonoBehaviour
{
    [SerializeField] private bool useCritical;

    [SerializeField, ShowIf(nameof(useCritical))]
    private float criticalMultiplier = 2f;

    [SerializeField, ShowIf(nameof(useCritical), inverse: false)]
    private float normalMultiplier = 1f;
}
```

Notes:

- By default, the field is shown when the condition is truthy.
- `inverse: false` shows the field when the condition is falsey.
- Supported condition types: `bool`, `int`, `float`, `string`, and object reference.
- Missing or unsupported condition fields are shown and log an error.

### `[AutoComponent]`

Adds or removes required components from the same GameObject through a boolean Inspector toggle.

```csharp
using UnityEngine;
using UTools;

public sealed class PhysicsToggle : MonoBehaviour
{
    [SerializeField, AutoComponent(typeof(Rigidbody), typeof(Collider))]
    private bool usePhysics;
}
```

Notes:

- The attribute is intended for `bool` fields.
- Toggling on adds missing components in the editor.
- Toggling off removes those components in the editor.
- It does not add or remove components while the game is playing.

## Tests

Unity Test Framework entry points are included:

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`

## Repository Structure

- `Assets/UTools/Scripts`: runtime and editor source
- `Assets/UTools/Example`: example scenes and scripts used inside this repo
- `Assets/UTools/Tests`: EditMode and PlayMode tests
- `Assets/UTools/Documentation~`: package documentation
- `Assets/UTools/Samples~`: package sample placeholder content
