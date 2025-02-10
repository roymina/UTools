[English](https://github.com/roymina/UTools)

# UTools

UTools 是一个轻量级的 Unity 插件，为游戏开发提供基本的工具和模式。它包含依赖注入、消息中心、GameObject/Component 查找器以及各种实用功能，以简化您的 Unity 开发工作流程。UTools 没有任何第三方依赖。

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

- 基于层级的对象查找
- 基于属性的对象查找
- Resource 资源自动载入
- 组件缓存以提升性能
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

### 实用功能示例

工具类被封装在一个纯静态类中，所有的方法均为扩展方法，在相应类型的实例后面加上`.`即可调出。

#### [通用]

1. **Map**：将一个值从一个范围映射到另一个范围，确保该值在原始范围内。

#### [字符串]

1. **IsNullOrEmpty**：检查字符串是否为 null 或为空。

2. **IsNotNullOrEmpty**：检查字符串是否不为 null 且不为空。
3. **CheckUserName**：检查输入字符串是否仅由字母、数字和下划线组成。
4. **TrimLength**：将字符串修剪为指定长度，如果被截断则附加省略号。
5. **CheckStrChinese**：检查输入字符串是否仅由中文字符组成。
6. **ToBase64String**：将字符串转换为 Base64 字符串。

#### [日期和时间]

1. **ToTimeString**：将整数（秒）转换为时间字符串，支持可选的中文格式和自定义时间格式。
2. **Tohhmmss**：将 `TimeSpan` 转换为格式化字符串，支持可选的中文格式。
3. **CalculateTimeSpan**：计算两个时间字符串之间的 `TimeSpan`。

#### [GameObject 和组件]

1. **FindChild**：通过名称查找子 `GameObject`，支持搜索所有后代和模糊搜索。
2. **GetAllChildren**：检索 `GameObject` 的所有子物体和后代，包括未激活的。
3. **ExistsChild**：检查是否存在具有指定名称的子 `GameObject`。
4. **ShowOneChild**：仅显示一个指定的子 `GameObject`，通过名称或索引。
5. **HideOneChild**：隐藏一个指定的子 `GameObject`，通过名称或索引。
6. **ToggleAllChildren**：切换所有子 `GameObject` 的可见性。
7. **ToggleMesh**：切换 `MeshRenderers` 和 `SkinnedMeshRenderers` 的可见性。
8. **CloneMesh**：克隆 `GameObject` 的网格，支持可选的新材质和名称。
9. **CombineMesh**：将所有子 `GameObject` 的网格合并为一个网格。
10. **GetMeshFilterBounds**：计算附加到 `Transform` 的 `MeshFilter` 的边界，支持可选的所有子 `Transform`。
11. **FindNearestObject**：查找具有类型 `T` 组件的最近 `GameObject`。
12. **IsActiveAndMeshEnabled**：检查 `GameObject` 是否处于活动状态且其 `MeshRenderer` 是否启用。
13. **HasComponent**：检查 `GameObject` 是否具有特定组件。
14. **EnsureComponent**：确保 `GameObject` 具有特定组件，必要时添加。
15. **GetComponentInSelfThenParent**：从 `GameObject` 或其父级获取组件。
16. **FindChildByName**：通过名称查找子 `Transform`。

#### [颜色]

1. **ConvertColorToColor32**：将 `Color` 转换为 `Color32`。
2. **ConvertColor32ToColor**：将 `Color32` 转换为 `Color`。

#### [UI]

1. **ToggleAsCanvasGroup**：切换 `RectTransform` 的可见性作为 `CanvasGroup`，支持补间和交互性。
2. **ToggleAsCanvasGroupAuto**：自动切换 `RectTransform` 的可见性作为 `CanvasGroup`，支持补间。
3. **Show (Transform)**：通过设置本地缩放显示或隐藏 `Transform`。
4. **Show (RectTransform)**：通过设置本地缩放显示或隐藏 `RectTransform`。
5. **ToSprite**：将 `Texture2D` 转换为 `Sprite`。
6. **ToTexture2D (Sprite)**：将 `Sprite` 转换为 `Texture2D`。
7. **ToTexture2D (string)**：将 Base64 字符串转换为 `Texture2D`。
8. **ToBase64**：将 `Texture2D` 转换为 Base64 字符串。
9. **ToTexture2D (Texture)**：将 `Texture` 转换为 `Texture2D`。
10. **DecodeBase64Image**：将 Base64 字符串解码为 `Texture2D`。
11. **TweenColor**：补间 UI `Image` 的颜色。
12. **MoveOutOfScreen**：将 UI 元素移出屏幕到指定方向，支持补间。
