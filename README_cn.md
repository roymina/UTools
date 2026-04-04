[English](https://github.com/roymina/UTools)

# UTools

UTools 是一个轻量级 Unity 工具集，覆盖依赖注入、消息系统、对象查找和常用运行时辅助能力。它的目标是保持体积小、项目内可直接使用，同时方便整理成可复用插件。

## 模块说明

### UDI

- 通过 `[Inject]` 支持字段、属性、方法注入
- 通过 `UDIContext` 支持场景级和层级容器
- 支持运行时生命周期接口：`IInitializable`、`ITickable`、`ILateTickable`、`IFixedTickable`、`IUDisposable`、`IPausable`
- 支持 `NonLazy()`
- 支持 `TryResolve<T>()` / `TryResolve(Type, out object)`
- 支持场景对象和实例化预制体上的 `MonoBehaviour` 注入

说明：

- 不支持构造函数注入。
- `UDIInstallerBase` 仍保留用于兼容旧场景，但已经是旧入口。
- `WithId`、自定义 `Scoped` 语义、按标识符解析等高级能力不是当前推荐 API。

### UMessage

- 强类型发布 / 订阅
- 支持可释放的订阅句柄
- 支持“先发布、后订阅”的待处理消息回放
- 会自动清理已经销毁的 `UnityEngine.Object` 订阅目标

### UFind

- `[Child]` 子对象查找
- `[Comp]` 当前对象组件查找
- `[Resource]` 资源自动加载
- 反射元数据缓存，减少重复启动扫描
- 支持路径形式的子节点查找，例如 `[Child("Root/Panel/Button")]`

### UUtils

- 提供字符串、时间、文件、GameObject、网格、颜色、UI 等扩展方法
- 持久化目录读写现在会自动创建缺失目录

### Editor 辅助

- `ButtonAttribute`
- `ShowIfAttribute`
- `AutoComponentAttribute`

## 安装方式

### 方式 1：Git URL / UPM 安装

通过 Package Manager 添加：

`https://github.com/roymina/UTools.git?path=/Assets/UTools`

`TextMeshPro` 通过 Unity 官方包依赖 `com.unity.textmeshpro` 自动引入。
如果导入示例后出现 TMP 资源缺失，执行一次 `Window > TextMeshPro > Import TMP Essential Resources`。

### 方式 2：使用 `unitypackage`

如果你更偏好一次性导入，可以使用打包后的发布文件。

### 方式 3：本地嵌入式包工作流

仓库已经补齐了面向包化的基础元数据，位于 `Assets/UTools`：

- `package.json`
- `Documentation~/`
- `Samples~/`
- `CHANGELOG.md`
- 运行时、编辑器、示例、测试对应的 `asmdef`

## 推荐接入方式

推荐入口是 `UDIContext`。

1. 在场景根节点添加 `UDIContext`
2. 绑定一个或多个 `MonoInstaller` / `ScriptableObjectInstaller`
3. 在 `InstallBindings` 中注册服务
4. 在运行时脚本中通过 `[Inject]` 获取依赖

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

## 全局 Context 模式

如果你需要跨场景服务，可以让 Context 所在对象常驻：

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

子 Context 可以通过层级关系继承父容器服务。

## 旧方式

`UDIInstallerBase` 仍可用于老场景：

```csharp
using UTools;

public class LegacyInstaller : UDIInstallerBase
{
    protected override void RegisterServices()
    {
        Container.Register<LegacyService>();
    }
}
```

新代码建议统一迁移到 `UDIContext`。

## 消息系统示例

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

如果消息在订阅者出现前就已经发布，默认情况下第一个后续订阅者仍会收到待处理消息回放。

## UFind 示例

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

行为说明：

- `[Child]` 不传参数时，默认用字段名查找
- 如果同名子节点出现多个，UTools 会报错并要求改用路径查找

## 带注入的实例化

如果你需要在实例化预制体后完成注入，使用 `UGameObjectFactory`：

```csharp
var instance = UGameObjectFactory.InstantiateWithDependency(prefab, parentTransform);
```

这会对预制体根节点和其所有子节点上的 `MonoBehaviour` 一并执行注入。

## 测试

项目现在已经补齐 Unity Test Framework 测试入口：

- `Assets/UTools/Tests/EditMode`
- `Assets/UTools/Tests/PlayMode`

当前仓库中的自动化验证方式：

- `dotnet build utools.sln -nologo`
- Unity 编辑器内的 EditMode / PlayMode Test Runner

## 仓库结构

- `Assets/UTools/Scripts`：运行时与编辑器源码
- `Assets/UTools/Example`：示例场景和示例脚本
- `Assets/UTools/Tests`：EditMode 与 PlayMode 测试
- `Assets/UTools/Documentation~`：包内文档
- `Assets/UTools/Samples~`：包内示例占位目录

## 当前状态

UTools 已经朝“可复用插件结构”做过一轮重构，但仍保留了部分兼容旧场景的桥接能力。对于新的开发，推荐统一采用下面这套方式：

- `UDIContext`
- `MonoInstaller` / `ScriptableObjectInstaller`
- 使用可释放句柄的强类型 `UMessageCenter` 订阅
