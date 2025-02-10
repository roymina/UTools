# UTools

UTools 是一个轻量级的 Unity 插件，为游戏开发提供基本的工具和模式。它包含依赖注入、消息中心、GameObject/Component 查找器以及各种实用功能，以简化您的 Unity 开发工作流程。

## 功能特性

### 1. 依赖注入 (UDI)

- 轻量级依赖注入系统
- 支持方法和字段注入
- 支持 MonoBehaviour 类注入
- 支持项目范围的依赖注入
- 自动依赖解析
- 单例管理

### 2. 消息中心 (UMessage)

- 事件驱动的通信系统
- 类型安全的消息传递
- 订阅/取消订阅机制
- 全局和作用域事件处理

### 3. GameObject/Component/Resource 查找器 (UFind)

- 便捷的子 GameObject 查找
- 基于属性的对象查找
- 组件缓存以提升性能
- 基于层级的对象查找
- 类型安全的组件访问

### 4. 实用功能 (UUtilis)

- 常用 Unity 操作

## 安装

1. 从 Release 页面下载 UTools 包
2. 将 UTools.unitypackage 导入到您的 Unity 项目中
3. 即可开始使用！

## 快速入门

### 依赖注入示例

1. 编写一个继承自 `UDIInstallerBase` 的类，并实现 `RegisterGlobalServices()` 和 `RegisterSceneServices()` 方法，分别对应当前场景注入和全局注入

   ```c#
   using UTools;

   public class _TestInstaller : UDIInstallerBase
   {
       //注册为全局服务
       protected override void RegisterGlobalServices()
       {
           Container.Register<NormalClass>();
           Container.Register<MonoBehaviourClass>();
       }
       //注册为当前场景服务
       protected override void RegisterSceneServices()
       {
           Container.Register<NormalClass>();
           Container.Register<MonoBehaviourClass>();
       }
   }
   ```

   > 对于全局注册的服务，加载新场景后，注入的类仍然存在，不会被销毁。
   >
   > 如果场景中已经存在 MonoBehaviour 类，将自动找到并注册该类。否则，将创建一个同名的新对象并挂载该类。

2. 使用 [Inject] 特性在任何类中注入服务。

   ```c#
   using UTools;
   public class TestInjection : MonoBehaviour
   {
       [Inject] NormalClass normalClass;
       [Inject] MonoBehaviourClass monoBehaviourClass;
       void Start()
       {
         normalClass.DoSomthing();
         monoBehaviourClass.DoSomthing();
       }
   }
   ```

3. 你也可以在任何方法上使用 `[Inject]` 特性，依赖项将作为参数注入到该类中

   ```c#
   public class TestServiceA
   {
       [Inject]
       public void InjectServiceB(TestServiceB testServiceB)
       {
           testServiceB.SayHello();
       }
   }
   ```

   > 注意，在上面的示例中，`TestServiceB` 将被自动注册，无需使用 `Container.Register()` 方法进行注册。

### 消息中心示例

```c#
//订阅消息
UMessageCenter.Instance.Subscribe<MyCustomMessage>(msg =>
{
    txtSubscriber1.text = txtSubscriber2.text = txtSubscriber3.text = msg.ToString();
});

//发布消息
btnPublisher.onClick.AddListener(() =>
{
    UMessageCenter.Instance.Publish(new MyCustomMessage { Name = "MyCustomMessage" });
});
```

### GameObject/Component/Resource 查找器示例

编写一个继承自 `UBehaviour` 的类，使用`[Child]`标签指定子对象，在游戏启动时脚本会自动查找并缓存该对象。子对象可以是 GameObject 或者 Component。

如果 Child 标签未传入任何参数，则会查找和变量名相同名称的子对象。

```c#
using UTools;
public class TestBehaviour : UBehaviour
{
    [Child]
    public GameObject childObject;

    [Child]
    public TextMeshProUGUI text;
}

```

如果传入参数，则忽略变量名，直接查找指定名称的子对象。

```c#
[Child("someObject")]
public GameObject childObject;
```

使用[Resource]标签指定资源路径，在游戏启动时脚本会自动查找并缓存该资源。

```c#
//查找Resources文件夹下的testPrefab资源并作为GameObject载入
[Resource]
public GameObject testPrefab;

//查找Resources/Prefabs/TestPrefab资源并作为GameObject载入
[Resource("Prefabs/TestPrefab")]
public GameObject testPrefab;
```
