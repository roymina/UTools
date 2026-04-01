# UTools 稳定化与可复用化改进计划

## Summary
- 目标是保留现有 `UTools` 命名空间和大部分使用方式，优先修复行为不成立的能力，再把仓库整理成适合长期复用的插件结构。
- 第一轮不引入构造函数注入，也不做大规模 API 重命名；重点把 `DI`、消息系统、查找工具、编辑器扩展和测试基线补齐。

## Key Changes
- `DI` 核心重构为“绑定完成 -> 解析 -> 生命周期注册”三段式。
  - 让 `Bind(...).AsSingle()/AsTransient()/FromInstance()/FromGameObject()/NonLazy()` 在安装阶段真正完成注册。
  - 在实例创建和 `FromInstance` 注册后统一接入 `LifecycleManager.Add`，确保 `IInitializable/ITickable/...` 生效且只注册一次。
  - 保留 `UDIInstallerBase`，但标记为过时入口；内部改为桥接到受控容器流程，不再做全场景扫描注入。
  - 新增 `TryResolve<T>(out T)` / `TryResolve(Type, out object)`，未解析成功时给出明确错误；父容器解析不再吞掉非“未注册”异常。
  - `WithId`、`InScope(Scoped)`、`OnInstantiated` 本轮不实现完整语义，统一标记为过时并从 README 移除，避免继续承诺未落地能力。

- 消息系统改为类型安全、可解绑、语义明确的实现。
  - 保留 `Subscribe/Unsubscribe/Publish` 现有入口。
  - 新增返回订阅句柄的 `Subscribe<T>(Action<T> handler, bool replayPending = true)` 重载，旧接口内部委托到新实现。
  - 去掉 `DynamicInvoke`，改为每种消息类型维护强类型处理器列表。
  - 维持当前“无人订阅时缓存、首次后续订阅可回放”的兼容语义，但把它明确成可选的 `replayPending` 行为并补文档、补测试。
  - 对失效的 `UnityEngine.Object` 订阅者做清理，避免长期单例持有悬空委托。

- `UFind`、`UUtils`、`Highlighter` 做功能修补和结构拆分。
  - 修复 `ExistsChild` 参数错误、持久化目录未创建、重复注入导致 `PostInjection` 多次执行等确定性 bug。
  - `UBehaviour` 增加按类型缓存反射元数据，避免每次 `Awake` 全量反射扫描；子节点查找支持“名称重复时报错/按路径查找”的明确策略。
  - 把超大的 `UUtils` 按域拆成 `StringExtensions`、`TimeExtensions`、`GameObjectExtensions`、`UIExtensions`、`FileUtils` 等；保留 `UUtils` 作为兼容转发层。
  - `Highlighter` 改用 `MaterialPropertyBlock` 或显式材质缓存与销毁策略，完整恢复原始颜色、透明度、blend、`renderQueue` 和 emission 状态。

- 编辑器与包结构整理为可复用插件形态。
  - 为 Runtime、Editor、Examples、Tests 建立独立 `asmdef`，示例代码不再和运行时代码混编。
  - `ButtonAttribute` 只处理无参实例方法，并改成低冲突的 fallback editor 方案；缓存反射结果，减少 Inspector 开销。
  - 清理危险文件名和目录命名，补 `package.json`、`CHANGELOG.md`、`Documentation~`、`Samples~`，保留现有 `unitypackage` 发布链路作为兼容输出。

## Test Plan
- EditMode：
  - `DI` 绑定解析、单例/瞬态、`FromInstance`、父子容器、未注册类型错误、循环依赖报错、`PostInjection` 只执行一次。
  - 生命周期接口在解析后能进入 `Initialize/Tick/Dispose`，Context 销毁时可正确清理。
  - 消息中心的订阅、取消订阅、回放、失效对象清理、异常隔离。
- PlayMode：
  - `UBehaviour` 的 `[Child]`、`[Comp]`、`[Resource]` 在真实场景中的自动绑定。
  - `InstantiateWithDependency` 对预制体根节点和子节点组件都能完成注入。
  - `Highlighter` 启停后材质数量不增长、视觉状态可完全恢复。
- 交付验证：
  - `dotnet build utools.sln`
  - Unity Test Runner 的 EditMode/PlayMode 测试
  - 示例场景一次 smoke test，确认 README 示例仍能运行

## Assumptions
- 保持 `UTools` 现有主要公开类名、特性名和基础调用方式不变，以兼容现有项目。
- 本轮不增加构造函数注入，不引入第三方依赖注入框架。
- `UDIInstallerBase` 不删除，只标记过时并收敛行为；推荐入口改为 `UDIContext`。
- 消息缓存语义先兼容现状并显式化，不在这一轮直接改成完全不同的事件总线模型。
