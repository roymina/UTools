[中文](https://github.com/roymina/UTools/blob/main/README_cn.md)

# UTools

UTools is a lightweight Unity toolkit that groups four practical modules:

- `UDI`: dependency injection and runtime lifecycle management
- `UFind`: attribute-based component, child, and resource binding
- `UMessage`: typed publish / subscribe messaging
- `UUtils`: common runtime helpers for strings, files, UI, meshes, textures, and GameObjects

## Installation

### Git URL / UPM

In `Package Manager`, choose `Add package from git URL...` and use:

`https://github.com/roymina/UTools.git?path=/Assets/UTools`

This repository remains a normal Unity project, while `Assets/UTools` is exposed as the package root.

`TextMeshPro` is pulled in through the official Unity package dependency `com.unity.textmeshpro`.
If imported samples show missing TMP assets, run `Window > TextMeshPro > Import TMP Essential Resources` once.

### `unitypackage`

Use the packaged release artifact in `Releases/` if you prefer manual import.

## Module Overview

### UDI

- Uses `UDIContext` as the DI entry point
- Supports field, property, and method injection via `[Inject]`
- Supports post-injection callbacks via `[PostInjection]`
- Supports `IInitializable`, `IAsyncInitializable`, `ITickable`, `ILateTickable`, `IFixedTickable`, `IUDisposable`, `IPausable`
- Supports `AsSingle()`, `AsTransient()`, `InScope(...)`, `FromInstance(...)`, `FromGameObject(...)`, `AsGlobal()`, `RequiredForContextStart()`, `NonLazy()`
- Supports scene injection and injected prefab instantiation through `UGameObjectFactory`

Notes:

- Constructor injection is not supported.
- There is no `UDIInstallerBase` in the current codebase.
- `MonoInstaller` and `ScriptableObjectInstaller` are the supported installer types.
- `GlobalInstaller` assets must be placed under `Resources` to enable hidden cross-scene global injection.
- A scene-level local container requires exactly one explicit `UDIContext` in the scene.
- Keep `UDIContext` and `MonoInstaller` on a dedicated bootstrap object; consumers can live anywhere else in the scene.
- If a scene contains multiple `UDIContext` components, initialization fails with an error instead of partially injecting objects.
- `ManagedContentRoot` still works, and required async services now also suspend other scene roots until injection is ready.

#### UDI Example: register services and start a scene context

```csharp
using UnityEngine;
using UTools;

public interface ILogger
{
    void Log(string message);
}

public sealed class UnityLogger : ILogger
{
    public void Log(string message)
    {
        Debug.Log(message);
    }
}

public sealed class GameInstaller : MonoInstaller
{
    [SerializeField] private Transform spawnRoot;

    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<ILogger>()
            .To<UnityLogger>()
            .AsSingle();

        container.Bind<Transform>()
            .FromInstance(spawnRoot)
            .AsSingle();

        container.Bind<GameManager>()
            .ToSelf()
            .AsSingle()
            .NonLazy();
    }
}
```

#### UDI Example: field, property, method, and post-injection

```csharp
using UnityEngine;
using UTools;

public sealed class GameManager : IInitializable, ITickable
{
    [Inject] private ILogger _logger;

    [Inject]
    public Transform SpawnRoot { get; private set; }

    private Camera _mainCamera;

    [Inject]
    private void Construct(Camera mainCamera)
    {
        _mainCamera = mainCamera;
    }

    public void Initialize()
    {
        _logger.Log("GameManager initialized");
    }

    [PostInjection]
    private void AfterInject()
    {
        _logger.Log($"Spawn root: {SpawnRoot.name}, camera: {_mainCamera.name}");
    }

    public void Tick()
    {
    }
}
```

#### UDI Example: instantiate a prefab and inject all child behaviours

```csharp
using UnityEngine;
using UTools;

public sealed class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private Transform spawnPoint;

    public void Spawn()
    {
        UGameObjectFactory.InstantiateWithDependency(
            enemyPrefab,
            spawnPoint.position,
            spawnPoint.rotation);
    }
}
```

#### UDI Example: register a cross-scene global fallback service

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

#### UDI Example: block a context until async services are ready

```csharp
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UTools;

public sealed class AsyncConfigInstaller : MonoInstaller
{
    [SerializeField] private ConfigService configService;

    public override void InstallBindings(UDIContainer container)
    {
        container.Bind<ConfigService>()
            .FromInstance(configService)
            .AsSingle()
            .RequiredForContextStart();
    }
}

public sealed class ConfigService : IAsyncInitializable
{
    public Task InitializeAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
```

- With a single scene `UDIContext`, required async services suspend other active scene roots until initialization finishes.
- Keep gameplay consumers off the bootstrap object so they do not wake before the scene context restores the scene.

### UFind

- `[Comp]` binds a component on the same GameObject
- `[Child]` binds a child GameObject or a component on a child
- `[Child("Root/Panel/Button")]` supports path lookup
- `[Resource]` loads from `Resources`
- `UBehaviour` caches reflection metadata to avoid repeated scans

#### UFind Example: auto-bind local components, children, and resources

```csharp
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UTools;

public sealed class InventoryPanel : UBehaviour
{
    [Comp] public Canvas Canvas;
    [Comp] public Button CloseButton;

    [Child] public TextMeshProUGUI Title;
    [Child("Content/Buttons/ConfirmButton")] public Button ConfirmButton;
    [Child("Content/Icon")] public Image Icon;

    [Resource("Icons/Inventory")] public Sprite InventorySprite;

    protected override void Awake()
    {
        base.Awake();
        Title.text = "Inventory";
        Icon.sprite = InventorySprite;
    }
}
```

#### UFind Behavior Notes

- If `[Child]` has no argument, the field name is used.
- If the field type is `GameObject`, the child object itself is assigned.
- If the field type is a component, UTools finds the child first, then gets the component from that child.
- If multiple children share the same name, use a path instead of a simple name.

### UMessage

- Uses `UMessageCenter.Instance`
- Supports typed `Subscribe<T>()`, `Publish<T>()`, and `Unsubscribe<T>()`
- Returns `IMessageSubscription` so you can dispose subscriptions cleanly
- Replays pending messages to the first later subscriber by default
- Cleans up dead `UnityEngine.Object` subscriber targets automatically

#### UMessage Example: subscribe, replay pending messages, and dispose

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

#### UMessage Example: subscribe without replay

```csharp
using UTools;

IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<ScoreChangedMessage>(
    message => UnityEngine.Debug.Log(message.Value),
    replayPending: false);
```

### UUtils

`UUtils` is a large extension/helper collection. Common groups include:

- string helpers such as `IsNullOrEmpty()`, `CheckUserName()`, `TrimLength()`
- time helpers such as `ToTimeString()` and `Tohhmmss()`
- persistent data helpers such as `ReadFromPersistDataPath()` and `WriteToPersistDataPath()`
- GameObject helpers such as `FindChild()`, `GetAllDecendents()`, `ToggleAllChildren()`
- UI and asset helpers such as `ToggleAsCanvasGroup()`, `ToSprite()`, `ToTexture2D()`, `TweenColor()`
- mesh helpers such as `CloneMesh()`, `CombineMesh()`, `GenerateQuadMesh()`

#### UUtils Example: strings, time, and persistent data

```csharp
using UnityEngine;
using UTools;

public sealed class UtilityExample : MonoBehaviour
{
    private void Start()
    {
        string name = "player_one";
        bool isValid = name.CheckUserName();
        string shortName = "VeryLongDisplayName".TrimLength(10);
        string timer = 95.ToTimeString();

        UUtils.WriteToPersistDataPath("{\"volume\":0.8}", "user_settings.json");
        string json = UUtils.ReadFromPersistDataPath("user_settings.json");

        Debug.Log($"valid={isValid}, short={shortName}, timer={timer}, json={json}");
    }
}
```

#### UUtils Example: GameObject and UI helpers

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

        if (closeButton != null)
        {
            Debug.Log($"Found child: {closeButton.name}");
        }
    }
}
```

### Editor Helpers

- `ButtonAttribute`
- `ShowIfAttribute`
- `AutoComponentAttribute`

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
