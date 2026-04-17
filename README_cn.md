[English](README.md)

# UTools

UTools 是一个轻量级 Unity 工具集，主要覆盖依赖注入、对象查找、消息派发、指针事件和常用运行时辅助能力。

| 模块 | 作用 |
| --- | --- |
| `UDI` | 场景 / 全局依赖注入、生命周期回调、异步启动门闸 |
| `UComponent` | 指针进入 / 点击 / 拖拽监听，支持 Inspector 回调和全局消息发布 |
| `UFind` | 基于特性的组件、子节点、子节点列表和 `Resources` 自动绑定 |
| `UMessage` | 强类型发布 / 订阅消息，支持可释放订阅 |
| `UUtils` | 字符串、时间、文件、GameObject、UI、纹理、网格等辅助方法 |
| Editor 辅助 | Inspector 按钮、条件显示字段、组件自动增删等小工具 |

## 安装

### Git URL / UPM

打开 `Window > Package Manager`，选择 `Add package from git URL...`，填入：

```text
https://github.com/roymina/UTools.git?path=/Assets/UTools
```

这个仓库本身仍然是一个普通 Unity 工程，`Assets/UTools` 会作为 UPM 包根目录暴露出去。

如果示例 UI 缺少 TMP 资源，执行一次：

```text
Window > TextMeshPro > Import TMP Essential Resources
```

### `unitypackage`

如果你更偏好手动导入，可以使用 `Releases/` 目录中的打包产物。

## 快速开始

1. 安装包。
2. 在场景中创建一个专门的 `Bootstrap` 物体。
3. 在这个物体上添加唯一的 `UDIContext`。
4. 添加 `MonoInstaller`，或把 installer 资产拖进 `UDIContext` 的列表。
5. 在 `InstallBindings` 里注册服务。
6. 在场景脚本中使用 `[Inject]`。
7. 运行时实例化需要注入的 Prefab 时，使用 `UGameObjectFactory`。

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

`UDI` 是依赖注入模块。它负责：

- 准备服务对象
- 给场景对象和运行时对象做注入
- 统一管理初始化、Tick、销毁等生命周期
- 在需要时阻塞场景子树，等待异步服务准备完成

### 三种 Installer

- `MonoInstaller`：场景级 installer。适合绑定场景引用，例如 `Transform`、`Camera`、Prefab、已存在组件。
- `ScriptableObjectInstaller`：资产级 installer。适合多个场景共享同一套绑定或配置。
- `GlobalInstaller`：全局 installer。适合跨场景兜底服务，即使某些场景没有 `UDIContext` 也能使用。

### 哪些对象才算真正进入 UDI

关键不是“类有没有实现 `IInitializable` / `IAsyncInitializable`”，而是“对象有没有进入 `UDIContainer` 的管理流程”。

| 对象类型 | 需要挂在场景物体上吗？ | 如何进入 UDI | UDI 能保证什么 |
| --- | --- | --- | --- |
| 纯 C# 服务，例如 `InventoryService : IInitializable` | 不需要 | 通过容器 `Bind` / `Resolve` 创建或获取 | 注入、`[PostInjection]`、生命周期回调 |
| `MonoBehaviour` 消费者，例如 `HealthPanel : MonoBehaviour` | 需要 | 位于 `UDIContext` 注入到的场景树中，或通过 `UGameObjectFactory`、`.FromGameObject(...)`、容器创建进入 | 注入、`[PostInjection]`、生命周期回调 |
| `MonoInstaller` | 需要 | 挂在 bootstrap 物体上，或显式添加到 `UDIContext.Installers` | 启动时执行 `InstallBindings` |
| `ScriptableObjectInstaller` | 不需要，它是资产 | 放进 `UDIContext > Scriptable Object Installers` | 当前场景启动时执行 `InstallBindings` |
| `GlobalInstaller` | 不需要，它是资产 | 在 `Resources` 下放置唯一一个资产 | 全局容器会在场景加载前启动 |

注意：

- 生命周期接口本身不会让对象“自动被发现”。
- 只有对象被 `UDIContainer` 创建、解析或显式注入后，UDI 才能保证它的注入和生命周期。
- `MonoInstaller` 的职责是注册绑定，不是承载业务逻辑。

### `UDIContext`

`UDIContext` 是推荐的场景 DI 入口，建议这样使用：

1. 创建一个专门的 `Bootstrap` 物体。
2. 添加 `UDIContext`。
3. 添加 `MonoInstaller` 组件，或把它们拖到 `UDIContext > Installers`。
4. 把 `ScriptableObjectInstaller` 资产拖到 `UDIContext > Scriptable Object Installers`。
5. 如果有绑定使用 `.RequiredForContextStart()`，给 `UDIContext` 指定 `Async Wait Root`。

注意事项：

- 一个场景里应该只有一个 `UDIContext`。多个 `UDIContext` 会导致初始化失败。
- 最好把 `UDIContext` 和启动阶段用到的 installer 放在专门的 bootstrap 物体上。
- 普通场景消费者可以放在场景任意位置，只要它最终会被这个 context 注入到。
- `UDIContext.IsReady` 表示启动是否完成；手动等待时可 `await ReadyTask`。
- 如果初始化抛异常，`InitializationException` 会被记录，context 保持未就绪状态。

### `MonoInstaller`

当绑定依赖场景引用时，优先用 `MonoInstaller`。

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

建议：

- `.NonLazy()` 会立刻 finalize 当前绑定，建议始终放在链式调用末尾。
- 对于 Inspector 序列化拖拽进来的引用，优先使用 `.FromInstance(...)`。
- 对 `MonoBehaviour` 的特定宿主绑定，可使用 `.FromGameObject(host)`。

### `ScriptableObjectInstaller`

当同一套绑定希望被多个场景复用时，使用 `ScriptableObjectInstaller`。

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

- 从 Unity 的 `Create` 菜单创建 installer 资产。
- 把这个资产拖进 `UDIContext > Scriptable Object Installers`。
- 只有被拖进列表的资产才会参与当前场景绑定。
- 它是资源资产，不需要也不能挂到场景物体上。

### `GlobalInstaller`

当服务需要作为跨场景兜底绑定时，使用 `GlobalInstaller`。

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
- 把这个资产放在任意 `Resources` 目录下，例如 `Assets/Resources/GameGlobalInstaller.asset`。
- `.AsGlobal()` 只能在 `GlobalInstaller` 中使用。
- `GlobalInstaller` 是资源资产，不是场景组件。
- 如果 `Resources` 下有多个 `GlobalInstaller`，全局运行时会报错。

### Binding API

| API | 作用 |
| --- | --- |
| `Bind<T>()` | 把 `T` 同时作为契约类型和实现类型注册 |
| `Bind<TContract, TImplementation>()` | 将接口 / 基类注册到具体实现 |
| `.To<T>()` | 指定实现类型 |
| `.ToSelf()` | 使用契约类型本身作为实现 |
| `.AsSingle()` | 在当前容器中复用单例 |
| `.AsTransient()` | 每次 Resolve 都创建新实例 |
| `.InScope(BindingScope.Scoped)` | 在当前 context / container 中缓存一个实例 |
| `.FromInstance(instance)` | 使用已有实例 |
| `.FromGameObject(gameObject)` | 在指定物体上解析或补加 `MonoBehaviour` |
| `.NonLazy()` | 在 context 启动时立即创建 |
| `.RequiredForContextStart()` | 把该绑定标记为启动门闸的一部分 |
| `.AsGlobal()` | 标记为全局绑定，仅能在 `GlobalInstaller` 中使用 |

说明：

- 当前不支持构造函数注入。
- 对非抽象且有无参构造的纯 C# 类，UDI 可以在没有显式绑定时自动创建。
- 接口、抽象类和关键服务建议显式绑定。
- 循环依赖会直接抛错，不会做部分注入。

### 注入写法

`[Inject]` 可用于字段、可写属性、方法，私有成员也支持。

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

注入顺序：

1. 字段
2. 属性
3. 方法
4. `[PostInjection]`

注意：

- 生命周期接口不会替代注入；它只是让对象在进入 UDI 管理后，多出初始化 / Tick / 销毁等回调。
- 场景对象由 `UDIContext` 注入。
- 运行时 Prefab 建议通过 `UGameObjectFactory` 或 `PrefabFactory<T>` 创建。

### 生命周期接口

实现这些接口后，对象会被 `LifecycleManager` 统一调度：

| 接口 | 触发时机 |
| --- | --- |
| `IInitializable.Initialize()` | context 完成注入后执行 |
| `IAsyncInitializable.InitializeAsync(...)` | 只有绑定标记了 `.RequiredForContextStart()` 才会被等待 |
| `ITickable.Tick()` | 每帧 `Update` |
| `IFixedTickable.FixedTick()` | 每次 `FixedUpdate` |
| `ILateTickable.LateTick()` | 每帧 `LateUpdate` |
| `IUDisposable.Dispose()` | `LifecycleManager` 销毁时 |
| `IPausable.Pause()` / `Resume()` | 显式调用 `LifecycleManager.Pause()` / `Resume()` 时 |

注意：

- 纯 C# 服务实现这些接口时，不需要任何 `GameObject`。
- `MonoBehaviour` 只有在被场景 Context 注入，或被工厂 / 容器通过 UDI 创建后，才会参与这些生命周期。
- `IAsyncInitializable` 不会自动等待，除非它的绑定被标记为 `.RequiredForContextStart()`。

### 异步启动与 `Async Wait Root`

当场景中有一部分对象必须等待异步服务准备完毕后再激活时，使用 `.RequiredForContextStart()` 配合 `Async Wait Root`。

```csharp
using System.Threading;
using System.Threading.Tasks;
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

行为说明：

- `Async Wait Root` 会在启动时先被临时设为不激活。
- 所有标记为 `RequiredForContextStart` 的异步服务会按注册顺序依次初始化。
- 异步初始化完成后，UDI 才会对 `Async Wait Root` 子树做注入并恢复原激活状态。
- `Async Wait Root` 之外的对象会立即注入并继续运行。

注意：

- 把真正需要等待异步完成的 gameplay 节点放到 `Async Wait Root` 下面。
- 不要把 `UDIContext` 自己放进 `Async Wait Root`。
- 如果有 required async 绑定但没有设置 `Async Wait Root`，context 会初始化失败。

### 运行时 Prefab 注入

不要直接对包含 `[Inject]` 消费者的 Prefab 使用 `Object.Instantiate(...)`。请使用 `UGameObjectFactory`。

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

关键点：

- `UGameObjectFactory` 会先实例化，再对整个新对象子树执行 UDI 注入。
- 它会优先使用最近父级上的 `UDIContext`，其次回退到全局容器，再次才是默认容器。
- 如果直接用 `Object.Instantiate(...)`，UDI 不会自动补注入。

### 如何选择 Installer、服务和普通组件

可以按这个准则判断：

- `MonoInstaller`：做场景启动配置
- `ScriptableObjectInstaller`：做可复用资产配置
- `GlobalInstaller`：做跨场景兜底绑定
- 普通 `MonoBehaviour`：写实际的 UI / gameplay 行为
- 纯 C# 服务：写不依赖 Unity 消息函数的领域逻辑

常见误区：

- 不要把业务逻辑塞进 installer。
- 不要期待普通 `Instantiate` 自动完成注入。
- 不要在 `Resources` 里放多个 `GlobalInstaller`。

## UComponent

### `PointerEventListener`

`PointerEventListener` 把 Unity 的这些接口统一封装到了一个组件里：

- `IPointerClickHandler`
- `IPointerEnterHandler`
- `IPointerExitHandler`
- `IPointerDownHandler`
- `IPointerUpHandler`
- `IBeginDragHandler`
- `IDragHandler`
- `IEndDragHandler`

你可以把它理解为一个“统一的指针事件桥接器”，支持三种接法：

- 运行时代码回调
- Inspector 里的 UnityEvent
- 通过 `UMessageCenter` 发送全局消息

```csharp
using UnityEngine;
using UTools;

public sealed class PointerExample : MonoBehaviour
{
    [SerializeField] private PointerEventListener listener;

    private IMessageSubscription _subscription;

    private void Awake()
    {
        listener.onEnter += () => Debug.Log("Pointer entered");
        listener.onClickWithData += data => Debug.Log($"Clicked at {data.position}");
        listener.onDragWithData += (_, delta) => Debug.Log($"Drag delta: {delta}");
    }

    private void OnEnable()
    {
        _subscription = UMessageCenter.Instance.Subscribe<PointerEventMessage>(
            OnPointerMessage,
            replayPending: false);
    }

    private void OnDisable()
    {
        _subscription?.Dispose();
        _subscription = null;
    }

    private void OnPointerMessage(PointerEventMessage message)
    {
        if (message.Target != listener.gameObject)
        {
            return;
        }

        Debug.Log($"{message.EventType} from {message.Target.name}, 3D={message.Is3DObject}, delta={message.Delta}");
    }
}
```

使用说明：

- 支持点击、进入、离开、按下、抬起、开始拖拽、拖拽中、结束拖拽。
- `publishGlobally` 默认开启。只想本地响应时，可以关闭它。
- 每种事件都有三种出口：
  - 代码回调：`onClick`、`onClickWithData`、`onDragWithData`
  - Inspector 事件：`onClickEvent`、`onClickEventWithData`、`onDragEventWithData`
  - 全局消息：`PointerEventMessage`
- `PointerEventMessage` 包含：
  - `EventType`
  - `Target`
  - `EventData`
  - `Is3DObject`
  - `Delta`
- Unity 认为当前输入处于拖拽时，`OnPointerClick` 不会再补发 click。
- 如果目标不是 `RectTransform`，`Is3DObject` 会是 `true`。

场景要求：

- 场景里必须有 `EventSystem`。
- 对 UI 对象：
  - 监听器挂在 `Canvas` 下的 UI 对象上
  - `Canvas` 需要 `GraphicRaycaster`
  - 目标图形要允许 Raycast
- 对 3D 对象：
  - 目标物体需要 Collider
  - 相机需要 `PhysicsRaycaster`

## UFind

`UFind` 用来减少重复的 `GetComponent`、`transform.Find` 和 `Resources.Load` 代码。

### 基本使用

1. 脚本继承 `UBehaviour`。
2. 在字段上添加 `[Comp]`、`[Child]`、`[Children]` 或 `[Resource]`。
3. 如果重写 `Awake`，先调用 `base.Awake()`。

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
    [Resource("Icons/Inventory")] private Sprite _inventorySprite;

    protected override void Awake()
    {
        base.Awake();
        Title.text = "Inventory";
        _icon.sprite = _inventorySprite;
    }
}
```

简要说明：

- `[Comp]`：找同一物体上的组件
- `[Child]`：找子节点或子节点上的组件
- `[Children]`：收集某个子节点下的一组子对象 / 组件
- `[Resource]`：从 `Resources` 加载资源

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

注意：

- 用 `Publish<T>(message)` 发布消息。
- 用 `Subscribe<T>(handler)` 订阅消息，并保存返回的 `IMessageSubscription`。
- 在 `OnDisable` 或 `OnDestroy` 中释放订阅。
- 如果消息发布时还没有订阅者，默认会进入待处理队列，稍后回放给第一个订阅者。
- 不想接收旧消息时，使用 `replayPending: false`。

## UUtils

`UUtils` 提供一些常用运行时辅助方法。

### 字符串和时间

- `IsNullOrEmpty()`
- `CheckUserName()`
- `TrimLength(maxLength)`
- `ToTimeString()`
- `ToHhMmSsString()`

### 文件

```csharp
using UTools;

UFileUtilities.WriteToPersistentDataPath("{\"volume\":0.8}", "settings/user.json");
string json = UFileUtilities.ReadFromPersistentDataPath("settings/user.json");
```

### GameObject 和 UI

- `FindChild(...)`
- `GetAllDescendants()`
- `ToggleAllChildren(show)`
- `EnsureComponent<T>()`
- `ToggleAsCanvasGroup(...)`

### 纹理和网格

- `Texture2D.ToSprite()`
- `Sprite.ToTexture2D()`
- `CloneMesh(...)`
- `GenerateQuadMesh(...)`
- `GeneratePolygonMesh(...)`

## Editor 辅助

### `[Button]`

给无参方法生成 Inspector 按钮。

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

### `[ShowIf]`

根据另一个序列化字段控制当前字段是否显示。

```csharp
using UnityEngine;
using UTools;

public sealed class DamageConfig : MonoBehaviour
{
    [SerializeField] private bool useCritical;

    [SerializeField, ShowIf(nameof(useCritical))]
    private float criticalMultiplier = 2f;
}
```

### `[AutoComponent]`

通过一个布尔开关，在编辑器中自动补加或删除同物体上的组件。

```csharp
using UnityEngine;
using UTools;

public sealed class PhysicsToggle : MonoBehaviour
{
    [SerializeField, AutoComponent(typeof(Rigidbody), typeof(Collider))]
    private bool usePhysics;
}
```

## 测试

项目包含 Unity Test Framework 测试入口：

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`

## 仓库结构

- `Assets/UTools/Scripts`：运行时与编辑器源码
- `Assets/UTools/Example`：仓库内示例场景与示例脚本
- `Assets/UTools/Tests`：EditMode / PlayMode 测试
- `Assets/UTools/Documentation~`：包文档
- `Assets/UTools/Samples~`：包示例占位内容
