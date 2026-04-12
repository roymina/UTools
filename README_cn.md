[English](README.md)

# UTools

UTools 是一个轻量级 Unity 工具集，用来处理依赖注入、UI / 对象查找、消息派发和常用运行时工具方法。

| 模块 | 用途 |
| --- | --- |
| `UDI` | 场景 / 全局依赖注入、生命周期回调、异步启动等待 |
| `UFind` | 基于特性的组件、子节点、子节点列表和 `Resources` 自动绑定 |
| `UMessage` | 强类型发布 / 订阅消息，支持可释放订阅 |
| `UUtils` | 字符串、时间、文件、GameObject、UI、纹理、网格等工具 |
| Editor 辅助 | Inspector 按钮、条件显示字段、组件自动增删等小工具 |

## 安装方式

### Git URL / UPM

打开 `Window > Package Manager`，选择 `Add package from git URL...`，填入：

```text
https://github.com/roymina/UTools.git?path=/Assets/UTools
```

这个仓库本身仍然是普通 Unity 工程，`Assets/UTools` 对外作为 UPM 包根目录。

`TextMeshPro` 会通过 `com.unity.textmeshpro` 引入。  
如果示例 UI 缺少 TMP 资源，执行一次：

```text
Window > TextMeshPro > Import TMP Essential Resources
```

### `unitypackage`

如果你更喜欢手动导入，可以使用 `Releases/` 目录中的发布文件。

## 快速开始

1. 安装包。
2. 在场景里创建一个 `Bootstrap` 物体，并添加唯一的 `UDIContext`。
3. 在同一个 `Bootstrap` 物体上添加 `MonoInstaller`，或把 installer 拖进 `UDIContext` 的列表。
4. 在 `InstallBindings` 中注册服务。
5. 在场景组件上使用 `[Inject]`，运行时 Prefab 使用 `UGameObjectFactory` 实例化。

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

`UDI` 是依赖注入模块。它负责准备服务、注入场景对象、注入运行时 Prefab，并把生命周期接口交给同一个 Context 管理。

### 三种 Installer

- `GlobalInstaller`：给**整个游戏**准备“全局兜底服务”。
- `MonoInstaller`：给**当前场景**准备“场景专属服务”。
- `ScriptableObjectInstaller`：给**当前场景**准备“可复用的资产型配置 / 服务”。

### `UDIContext`

`UDIContext` 是场景注入入口，推荐这样摆：

1. 创建一个专用 `Bootstrap` GameObject。
2. 添加 `UDIContext`。
3. 把 `MonoInstaller` 挂在同一个 GameObject 上，或拖到 `UDIContext` > `Installers`。
4. 把 `ScriptableObjectInstaller` 资产拖到 `UDIContext` > `Scriptable Object Installers`。
5. 如果有绑定使用 `.RequiredForContextStart()`，给 `UDIContext` 指定 `Async Wait Root`。

注意事项：

- 一个场景里保持唯一的 `UDIContext`，多个 Context 会直接初始化失败。
- `UDIContext` 和启动 installer 建议放在专用 `Bootstrap` 物体上，业务消费者可以放在场景任意位置。
- `MonoInstaller` 只有两种情况下会参与当前 Context：挂在 `UDIContext` 同一个物体上，或被显式拖进 `Installers` 列表。
- 单独放一个 `MonoInstaller` 且场景没有 Context 时，它可以自动补一个 Context，但正式场景建议显式配置。
- `UDIContext.IsReady` 表示启动是否完成；必须手动等待时可以 await `ReadyTask`。
- 初始化异常会记录到 `InitializationException`，Context 会保持未就绪。

### `MonoInstaller`

当绑定需要场景引用时使用 `MonoInstaller`，例如 Transform、Prefab、Camera、场景组件。

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

注意事项：

- `.NonLazy()` 会立刻创建 / finalize 当前绑定，建议永远放在链式调用最后。
- 序列化拖拽进来的场景引用优先用 `.FromInstance(...)`。
- `MonoBehaviour` 绑定需要指定物体时，用 `.FromGameObject(host)`。
- `FromGameObject` 会优先复用 Unity 已经找到的同类型实例，找不到时才会在目标物体上获取或添加组件。

### `ScriptableObjectInstaller`

当一组绑定需要被多个场景复用时，使用 `ScriptableObjectInstaller`。

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

使用方式：

- 通过 Unity 的 `Create` 菜单创建 installer 资产。
- 把资产拖到 `UDIContext` > `Scriptable Object Installers`。
- 只有拖进这个列表的 installer 资产才会参与当前场景的绑定。
- 场景对象引用放在 `MonoInstaller`，可复用配置和数据放在 `ScriptableObjectInstaller`。

### `GlobalInstaller`

当服务需要跨场景兜底时，使用 `GlobalInstaller`。

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

使用方式：

- 创建且只创建一个 `GlobalInstaller` 资产。
- 把资产放在 `Resources` 目录下，例如 `Assets/Resources/GameGlobalInstaller.asset`。
- `.AsGlobal()` 只能在 `GlobalInstaller` 里使用。
- 没有 `UDIContext` 的场景也能注入全局服务。
- 有唯一 `UDIContext` 的场景会优先使用本地绑定，找不到时再回退到全局绑定。
- 不要在 `Resources` 下放多个 `GlobalInstaller` 资产，当前全局运行时只支持一个。

### 绑定 API

| API | 用途 |
| --- | --- |
| `Bind<T>()` | 把 `T` 同时作为契约类型和实现类型注册 |
| `Bind<TContract, TImplementation>()` | 把接口 / 基类注册到具体实现 |
| `.To<T>()` | 指定具体实现类型 |
| `.ToSelf()` | 使用契约类型本身作为实现类型 |
| `.AsSingle()` | 当前容器内复用同一个实例 |
| `.AsTransient()` | 每次 Resolve 都创建新实例 |
| `.InScope(BindingScope.Scoped)` | 在当前 Context / 容器内缓存一个实例 |
| `.FromInstance(instance)` | 使用已有实例，常用于序列化引用 |
| `.FromGameObject(gameObject)` | 在指定 GameObject 上解析或添加 `MonoBehaviour` |
| `.NonLazy()` | Context 启动时立即创建实例 |
| `.RequiredForContextStart()` | 让该绑定在异步启动门槛完成前必须就绪 |
| `.AsGlobal()` | 标记全局绑定，只能用于 `GlobalInstaller` |

注意事项：

- 当前不支持构造函数注入。
- 具体非抽象类如果没有显式绑定，UDI 可以用无参构造自动创建。
- 接口、抽象类和关键服务建议显式绑定。
- 循环依赖会直接抛错，不会做部分注入。

### 注入写法

`[Inject]` 可以标在字段、可写属性和方法上，私有成员也支持。

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

注意事项：

- 注入顺序是字段、属性、方法、`[PostInjection]`。
- `[Inject]` 方法可以带参数，每个参数都会从容器 Resolve。
- `[PostInjection]` 在依赖赋值完成后执行，也可以带可解析参数。
- 场景对象由 `UDIContext` 注入；运行时 Prefab 用 `UGameObjectFactory` 或 `PrefabFactory<T>` 创建。
- `UDIContext` 默认执行顺序很早；不要把消费者脚本设置到比 `UDIContext` 更早执行。

### 生命周期接口

服务或被注入对象实现这些接口后，会被 `LifecycleManager` 管理。

| 接口 | 触发时机 |
| --- | --- |
| `IInitializable.Initialize()` | Context 注入完成后 |
| `IAsyncInitializable.InitializeAsync(...)` | 只有绑定标记 `.RequiredForContextStart()` 时才会等待 |
| `ITickable.Tick()` | 未暂停时每帧 `Update` |
| `IFixedTickable.FixedTick()` | 未暂停时每次 `FixedUpdate` |
| `ILateTickable.LateTick()` | 未暂停时每帧 `LateUpdate` |
| `IUDisposable.Dispose()` | `LifecycleManager` 销毁时 |
| `IPausable.Pause()` / `Resume()` | 手动调用 `LifecycleManager.Pause()` / `Resume()` 时 |

注意事项：

- 如果 `UDIContext` 物体上没有 `LifecycleManager`，会自动添加。
- 非 Lazy 或已 Resolve 的实例只会被跟踪一次。
- `IAsyncInitializable` 不会自动等待，必须把绑定标记为 `.RequiredForContextStart()`。

### 异步注入和等待

当某个场景子树必须等远程配置、存档、资源清单等异步服务完成后再激活时，使用这个流程。

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

这样注册服务后：

- 在 `UDIContext` 上指定 `Async Wait Root`。
- 场景开始时，如果 `Async Wait Root` 原本是激活状态，它会先被设为失活。
- 所有 Required 异步服务会按绑定注册顺序依次初始化。
- 异步服务全部完成后，UDI 会注入 `Async Wait Root` 子树，然后恢复它原来的激活状态。
- `Async Wait Root` 外面的对象会立即注入并继续运行。

注意事项：

- 需要等待异步服务的 gameplay 节点都放到 `Async Wait Root` 下。
- 不要把 `UDIContext` 所在物体放进 `Async Wait Root`。
- `Async Wait Root` 必须属于同一个场景。
- 存在 Required 异步绑定但没有指定 `Async Wait Root` 时，Context 会初始化失败。
- 如果 `Async Wait Root` 启动前就是失活状态，异步完成后仍保持失活，但依然会完成注入。
- 如果异步初始化失败或被取消，Context 会保持未就绪，等待根节点不会被恢复。

### 运行时 Prefab 注入

包含 `[Inject]` 消费者的 Prefab，不要直接用 `Object.Instantiate`，改用 `UGameObjectFactory`。

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

注意事项：

- 如果实例需要使用最近的父级 Context，优先使用带 `Transform parent` 的重载。
- 工厂会注入新实例 GameObject 子树上的所有 `MonoBehaviour`。
- 如果找不到 Context，工厂会退回全局 / 默认容器并输出警告。

## UFind

`UFind` 用来减少重复的 `GetComponent`、`transform.Find` 和 `Resources.Load` 代码。

### 基本使用

1. 脚本继承 `UBehaviour`。
2. 在字段上添加 `[Comp]`、`[Child]`、`[Children]` 或 `[Resource]`。
3. 如果重写 `Awake`，先调用 `base.Awake()`，再使用绑定字段。

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

- 查找当前 GameObject 上的组件。
- 只支持字段。
- 字段已有值时不会覆盖。
- 找不到组件时输出 warning。

### `[Child]`

- 不传参数时，使用字段名作为子节点名。
- 传字符串时，使用指定名称或路径。
- 字段类型是 `GameObject` 时，赋值子节点对象本身。
- 字段类型是组件时，先找到子节点，再从该子节点上取组件。
- 简单名称会在所有后代中忽略大小写查找。
- 路径查找使用相对当前 transform 的 `Transform.Find`。
- 如果多个后代重名，使用 `Root/Panel/Button` 这类路径写法。

### `[Children]`

- 先找到一个父级子节点，再把它的 children 收集成列表。
- 支持 `List<GameObject>` 和 `List<TComponent>`。
- 不指定 `parentName` 时，使用字段名作为父级子节点名。
- 默认 `includeDescendants = false`，只收集第一层子节点。
- 默认 `includeInactive = true`，会包含失活子节点。
- 组件列表只保留带有目标组件的子节点。
- 错误按字段输出；一个字段绑定失败不会影响其它字段继续绑定。

### `[Resource]`

- 使用 `Resources.Load(path, fieldType)` 加载资源。
- 不传路径时，使用字段名作为资源路径。
- 路径不要写文件扩展名。
- 资源必须放在 Unity 的任意 `Resources` 目录下。

## UMessage

`UMessage` 是强类型消息中心。

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

使用注意：

- 用 `Publish<T>(message)` 发布消息。
- 用 `Subscribe<T>(handler)` 订阅消息，并保存返回的 `IMessageSubscription`。
- 在 `OnDisable` 或 `OnDestroy` 中释放订阅。
- 如果消息发布时还没有订阅者，它会进入待处理队列，并回放给第一个后续订阅者。
- 不想接收旧消息时使用 `Subscribe<T>(handler, replayPending: false)`。
- `UMessageCenter.Instance.Clear()` 会清空所有订阅和待处理消息，主要用于测试或完整重置。
- handler 抛异常会被记录，不会阻断其它 handler。
- 已销毁的 `UnityEngine.Object` 订阅目标会自动清理。

```csharp
IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<ScoreChangedMessage>(
    message => Debug.Log(message.Value),
    replayPending: false);
```

## UUtils

`UUtils` 是一组小型运行时工具。引入 `UTools` 命名空间后，按扩展方法或静态方法使用。

### 字符串与时间

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

常用方法：

- `IsNullOrEmpty()`、`IsNotNullOrEmpty()`
- `CheckUserName()`、`CheckStrChinese()`、`IsIPAddress()`
- `TrimLength(maxLength)`、`ToBase64String()`
- `ToTimeString()`、`ToHhMmSsString()`、`TryCalculateTimeSpan(...)`

### 文件读写

```csharp
using UTools;

UFileUtilities.WriteToPersistentDataPath("{\"volume\":0.8}", "settings/user.json");
string json = UFileUtilities.ReadFromPersistentDataPath("settings/user.json");
```

注意事项：

- 文件会读写到 `Application.persistentDataPath`。
- 父目录会自动创建。
- `ReadFromPersistentDataPath` 默认会在文件不存在时创建空文件。

### GameObject 与 UI

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

常用方法：

- `FindChild(...)`、`GetAllDescendants()`、`GetDirectChildren()`
- `ShowOnlyDescendantNamed(...)`、`HideDescendantNamed(...)`
- `ToggleAllChildren(show)` 会切换所有后代，不会切换根物体自身
- `EnsureComponent<T>()`、`HasComponent<T>()`、`SetLayerRecursively(...)`
- `ToggleAsCanvasGroup(...)`、`ToggleAsCanvasGroupAuto(...)`、`TweenColor(...)`、`MoveOutOfScreen(...)`

注意事项：

- 大多数工具方法是 null-safe 的，输入无效时返回 `null`、空集合或 `false`。
- UI tween 没传 runner 时，会创建一个隐藏的常驻协程 runner。
- 如果希望协程生命周期跟随某个对象，请传入自己的 `MonoBehaviour` runner。

### 纹理与网格

常用方法：

- `Texture2D.ToSprite()`
- `Sprite.ToTexture2D()`
- `Texture.ToTexture2D()`
- `Texture2D.ToBase64()`
- `DecodeBase64Image(...)`
- `CloneMesh(...)`、`CombineMesh(...)`、`GenerateQuadMesh(...)`、`GeneratePolygonMesh(...)`、`GeneratePlane(...)`

注意事项：

- 纹理转换会创建运行时对象，不再需要时要自行销毁。
- 网格工具会创建运行时 GameObject / Mesh，请按普通生成物生命周期管理。

## Editor 辅助

### `[Button]`

给无参方法生成一个 Inspector 按钮。

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

注意事项：

- 方法必须无参数。
- 支持 public / private 实例方法。
- 多选对象时，按钮会对每个选中的目标执行一次。

### `[ShowIf]`

根据另一个序列化字段决定当前字段是否显示。

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

注意事项：

- 默认在条件字段为真时显示。
- `inverse: false` 表示在条件字段为假时显示。
- 支持的条件字段类型：`bool`、`int`、`float`、`string`、对象引用。
- 条件字段缺失或类型不支持时，会显示该字段并输出错误。

### `[AutoComponent]`

通过一个 bool 开关，在 Inspector 中给同一个 GameObject 添加或删除指定组件。

```csharp
using UnityEngine;
using UTools;

public sealed class PhysicsToggle : MonoBehaviour
{
    [SerializeField, AutoComponent(typeof(Rigidbody), typeof(Collider))]
    private bool usePhysics;
}
```

注意事项：

- 这个特性主要用于 `bool` 字段。
- 编辑器中勾选时，会补齐缺失组件。
- 编辑器中取消勾选时，会删除这些组件。
- 运行时不会添加或删除组件。

## 测试

项目包含 Unity Test Framework 测试入口：

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`

## 仓库结构

- `Assets/UTools/Scripts`：运行时与编辑器源码
- `Assets/UTools/Example`：仓库内使用的示例场景与示例脚本
- `Assets/UTools/Tests`：EditMode 与 PlayMode 测试
- `Assets/UTools/Documentation~`：包文档
- `Assets/UTools/Samples~`：包示例占位内容
