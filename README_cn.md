[English](https://github.com/roymina/UTools)

# UTools

UTools 是一个轻量级 Unity 工具集，主要包含四个实用模块：

- `UDI`：依赖注入与运行时生命周期管理
- `UFind`：基于特性的组件、子节点与资源自动绑定
- `UMessage`：强类型发布 / 订阅消息系统
- `UUtils`：字符串、文件、UI、网格、纹理、GameObject 等常用工具

## 安装方式

### Git URL / UPM

在 `Package Manager` 中选择 `Add package from git URL...`，填入：

`https://github.com/roymina/UTools.git?path=/Assets/UTools`

仓库本身仍然保持为普通 Unity 工程，而 `Assets/UTools` 对外作为可安装的包根目录。

`TextMeshPro` 会通过 Unity 官方包依赖 `com.unity.textmeshpro` 自动引入。  
如果导入示例后出现 TMP 资源缺失，请执行一次：

`Window > TextMeshPro > Import TMP Essential Resources`

### `unitypackage`

如果你更偏好手动导入，也可以直接使用 `Releases/` 目录中的发布文件。

## 模块说明

### UDI

- 使用 `UDIContext` 作为依赖注入入口
- 通过 `[Inject]` 支持字段、属性、方法注入
- 通过 `[PostInjection]` 支持注入完成后的回调
- 支持 `IInitializable`、`IAsyncInitializable`、`ITickable`、`ILateTickable`、`IFixedTickable`、`IUDisposable`、`IPausable`
- 支持 `AsSingle()`、`AsTransient()`、`InScope(...)`、`FromInstance(...)`、`FromGameObject(...)`、`AsGlobal()`、`RequiredForContextStart()`、`NonLazy()`
- 支持通过 `UGameObjectFactory` 对运行时实例化对象进行注入

说明：

- 当前不支持构造函数注入
- 当前没有 `UDIInstallerBase`
- 推荐使用 `MonoInstaller` 与 `ScriptableObjectInstaller`
- `GlobalInstaller` 资源需放在 `Resources` 目录下，才能启用跨场景全局注入
- 场景级本地注入要求场景中显式且唯一地放置一个 `UDIContext`
- 推荐把 `UDIContext` 与 `MonoInstaller` 放在单独的 Bootstrap 物体上，业务消费者可位于场景任意层级
- 如果场景里出现多个 `UDIContext`，初始化会直接报错，而不是做部分注入
- 当上下文依赖耗时初始化服务时，推荐使用 `AsyncWaitRoot` 明确指定需要等待的子树；其余场景对象会立即注入并继续运行

#### UDI 示例：注册服务并启动场景 Context

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

#### UDI 示例：字段、属性、方法与注入后回调

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

#### UDI 示例：实例化 Prefab 并自动注入

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
#### UDI 全局注入使用方式

- 在 `Resources` 目录下创建一个 `GlobalInstaller` 资源
- 在 `GlobalInstaller` 中注册少量跨场景常驻服务，并使用 `.AsGlobal()`
- 没有 `UDIContext` 的场景也可以直接注入这些全局服务
- 如果某个场景根对象上挂了普通 `UDIContext`，则该本地上下文中的绑定优先于全局绑定
- 
#### UDI 示例：注册跨场景全局服务

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

#### UDI 异步初始化使用方式

- 对于必须先加载完成的数据 / 服务，实现 `IAsyncInitializable`
- 对应绑定增加 `.RequiredForContextStart()`
- 在 `UDIContext` 上指定 `AsyncWaitRoot`
- `AsyncWaitRoot` 会在 Required 异步服务初始化成功后才被注入；如果原本是激活状态，初始化后会恢复激活
- 当场景使用唯一 `UDIContext` 时，其它已激活的场景根对象也会在初始化完成前保持暂停
- `AsyncWaitRoot` 之外的对象会立即注入并继续运行，但不应假设 Required 异步服务已完成初始化
- 如果场景存在 Required 异步服务但没有配置 `AsyncWaitRoot`，Context 会直接报错并中止初始化
- 如果初始化失败，Context 会保持未就绪状态，`AsyncWaitRoot` 也不会被激活


#### UDI 示例：等待异步服务就绪后再启动场景内容

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

### UFind

- `[Comp]` 绑定当前 GameObject 上的组件
- `[Child]` 绑定子节点或子节点上的组件
- `[Child("Root/Panel/Button")]` 支持路径查找
- `[Resource]` 自动从 `Resources` 加载资源
- `UBehaviour` 会缓存反射信息，减少重复扫描

#### UFind 示例：自动绑定本地组件、子节点和资源

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

#### UFind 行为说明

- `[Child]` 不传参数时，默认使用字段名查找
- 如果字段类型是 `GameObject`，会直接赋值子节点对象本身
- 如果字段类型是组件，会先找到子节点，再从该节点上获取组件
- 如果有多个同名子节点，建议使用路径写法

### UMessage

- 使用 `UMessageCenter.Instance`
- 支持强类型 `Subscribe<T>()`、`Publish<T>()`、`Unsubscribe<T>()`
- `Subscribe` 返回 `IMessageSubscription`，便于在 `OnDisable` 中释放
- 默认支持“先发布、后订阅”的待处理消息回放
- 会自动清理已经销毁的 `UnityEngine.Object` 订阅目标

#### UMessage 示例：订阅、接收回放并释放订阅

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

#### UMessage 示例：关闭待处理消息回放

```csharp
using UTools;

IMessageSubscription subscription = UMessageCenter.Instance.Subscribe<ScoreChangedMessage>(
    message => UnityEngine.Debug.Log(message.Value),
    replayPending: false);
```

### UUtils

`UUtils` 包含大量扩展与工具方法，常见类别包括：

- 字符串辅助：`IsNullOrEmpty()`、`CheckUserName()`、`TrimLength()`
- 时间辅助：`ToTimeString()`、`Tohhmmss()`
- 持久化读写：`ReadFromPersistDataPath()`、`WriteToPersistDataPath()`
- GameObject 操作：`FindChild()`、`GetAllDecendents()`、`ToggleAllChildren()`
- UI 与资源辅助：`ToggleAsCanvasGroup()`、`ToSprite()`、`ToTexture2D()`、`TweenColor()`
- 网格辅助：`CloneMesh()`、`CombineMesh()`、`GenerateQuadMesh()`

#### UUtils 示例：字符串、时间和持久化读写

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

#### UUtils 示例：GameObject 与 UI 辅助

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

### Editor 辅助

- `ButtonAttribute`
- `ShowIfAttribute`
- `AutoComponentAttribute`

## 测试

项目已经包含 Unity Test Framework 测试入口：

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`

## 仓库结构

- `Assets/UTools/Scripts`：运行时与编辑器源码
- `Assets/UTools/Example`：仓库内使用的示例场景与示例脚本
- `Assets/UTools/Tests`：EditMode 与 PlayMode 测试
- `Assets/UTools/Documentation~`：包文档
- `Assets/UTools/Samples~`：包示例占位内容
