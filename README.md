# UTools

UTools is a lightweight Unity plugin that provides essential tools and patterns for game development. It includes dependency injection, a message center, GameObject/Component finders, and various utility functions to streamline your Unity development workflow.UTools has no third-party dependencies.

## Features

### 1. Dependency Injection (UDI)

- Lightweight dependency injection system
- Supports method and field injection
- Supports MonoBehaviour class injection
- Project-wide dependency injection support
- Automatic dependency resolution
- Singleton management

### 2. Message Center (UMessage)

- Event-driven communication system
- Type-safe message passing
- Subscribe/unsubscribe mechanism
- Global and scoped event handling

### 3. GameObject/Component/Resource Finder (UFind)

- Hierarchy-based object finding
- Attribute-based object finding
- Automatic Resource loading
- Component caching for performance improvement
- Type-safe component access

### 4. Utility Functions (UUtilis)

- Common Unity operations

## Installation

1. Download the UTools package from the Release page
2. Import UTools.unitypackage into your Unity project
3. Start using it!

## Quick Start

### Dependency Injection Example

1. Write a class that inherits from `UDIInstallerBase` and implement the `RegisterGlobalServices()` and `RegisterSceneServices()` methods for global and scene-specific injection, respectively.

   ```c#
   using UTools;

   public class _TestInstaller : UDIInstallerBase
   {
       // Register as global services
       protected override void RegisterGlobalServices()
       {
           Container.Register<NormalClass>();
           Container.Register<MonoBehaviourClass>();
       }
       // Register as scene-specific services
       protected override void RegisterSceneServices()
       {
           Container.Register<NormalClass>();
           Container.Register<MonoBehaviourClass>();
       }
   }
   ```

   > For globally registered services, the injected classes remain after loading a new scene and will not be destroyed.
   >
   > If a MonoBehaviour class already exists in the scene, it will be automatically found and registered. Otherwise, a new object with the same name will be created and the class will be attached.

2. Use the [Inject] attribute to inject services into any class.

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

3. You can also use the `[Inject]` attribute on any method, and dependencies will be injected as parameters into the class.

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

   > Note that in the above example, `TestServiceB` will be automatically registered without needing to use the `Container.Register()` method.

### Message Center Example

```c#
// Subscribe to a message
UMessageCenter.Instance.Subscribe<MyCustomMessage>(msg =>
{
    txtSubscriber1.text = txtSubscriber2.text = txtSubscriber3.text = msg.ToString();
});

// Publish a message
btnPublisher.onClick.AddListener(() =>
{
    UMessageCenter.Instance.Publish(new MyCustomMessage { Name = "MyCustomMessage" });
});
```

### GameObject/Component/Resource Finder Example

Write a class that inherits from `UBehaviour` and use the `[Child]` attribute to specify child objects. The script will automatically find and cache the object at game start. Child objects can be GameObjects or Components.

If the Child attribute does not pass any parameters, it will look for a child object with the same name as the variable.

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

If parameters are passed, the variable name is ignored, and the specified child object name is directly searched.

```c#
[Child("someObject")]
public GameObject childObject;
```

Use the [Resource] attribute to specify the resource path. The script will automatically find and cache the resource at game start.

```c#
// Find the testPrefab resource under the Resources folder and load it as a GameObject
[Resource]
public GameObject testPrefab;

// Find the Resources/Prefabs/TestPrefab resource and load it as a GameObject
[Resource("Prefabs/TestPrefab")]
public GameObject testPrefab;
```

### Utility Functions Example

Utility classes are encapsulated in a pure static class, and all methods are extension methods. You can call them by adding a `.` after the corresponding type instance.

#### [Common]

1. **Map**: Maps a value from one range to another, ensuring the value is within the original range.

#### [Strings]

1. **IsNullOrEmpty**: Checks if the string is null or empty.

2. **IsNotNullOrEmpty**: Checks if the string is not null and not empty.
3. **CheckUserName**: Checks if the input string consists only of letters, digits, and underscores.
4. **TrimLength**: Trims the string to a specified length, appending an ellipsis if truncated.
5. **CheckStrChinese**: Checks if the input string consists only of Chinese characters.
6. **ToBase64String**: Converts the string to a Base64 string.

#### [Date & Time]

1. **ToTimeString**: Converts an integer (seconds) to a time string, with optional Chinese format and custom time format.
2. **Tohhmmss**: Converts a `TimeSpan` to a formatted string, with optional Chinese format.
3. **CalculateTimeSpan**: Calculates the `TimeSpan` between two time strings.

#### [GameObject & Components]

1. **FindChild**: Finds a child `GameObject` by name, with options for searching all descendants and performing a fuzzy search.
2. **GetAllChildren**: Retrieves all children and descendants of a `GameObject`, including deactivated ones.
3. **ExistsChild**: Checks if a child `GameObject` with the specified name exists.
4. **ShowOneChild**: Shows only one specified child `GameObject` by name or index.
5. **HideOneChild**: Hides one specified child `GameObject` by name or index.
6. **ToggleAllChildren**: Toggles the visibility of all children `GameObjects`.
7. **ToggleMesh**: Toggles the visibility of `MeshRenderers` and `SkinnedMeshRenderers`.
8. **CloneMesh**: Clones the mesh of the `GameObject`, optionally applying a new material and name.
9. **CombineMesh**: Combines the meshes of all child `GameObjects` into a single mesh.
10. **GetMeshFilterBounds**: Calculates the bounds of a `MeshFilter` attached to the `Transform`, optionally including all child `Transforms`.
11. **FindNearestObject**: Finds the nearest `GameObject` with a component of type `T`.
12. **IsActiveAndMeshEnabled**: Checks if a `GameObject` is active and its `MeshRenderer` is enabled.
13. **HasComponent**: Checks if a `GameObject` has a specific component.
14. **EnsureComponent**: Ensures that a `GameObject` has a specific component, adding it if necessary.
15. **GetComponentInSelfThenParent**: Gets a component from the `GameObject` or its parent.
16. **FindChildByName**: Finds a child `Transform` by name.

#### [Color]

1. **ConvertColorToColor32**: Converts a `Color` to a `Color32`.
2. **ConvertColor32ToColor**: Converts a `Color32` to a `Color`.

#### [UI]

1. **ToggleAsCanvasGroup**: Toggles the visibility of a `RectTransform` as a `CanvasGroup`, with options for tweening and interactivity.
2. **ToggleAsCanvasGroupAuto**: Automatically toggles the visibility of a `RectTransform` as a `CanvasGroup`, with options for tweening.
3. **Show (Transform)**: Shows or hides a `Transform` by setting its local scale.
4. **Show (RectTransform)**: Shows or hides a `RectTransform` by setting its local scale.
5. **ToSprite**: Converts a `Texture2D` to a `Sprite`.
6. **ToTexture2D (Sprite)**: Converts a `Sprite` to a `Texture2D`.
7. **ToTexture2D (string)**: Converts a Base64 string to a `Texture2D`.
8. **ToBase64**: Converts a `Texture2D` to a Base64 string.
9. **ToTexture2D (Texture)**: Converts a `Texture` to a `Texture2D`.
10. **DecodeBase64Image**: Decodes a Base64 string to a `Texture2D`.
11. **TweenColor**: Tweens the color of a UI `Image`.
12. **MoveOutOfScreen**: Moves a UI element out of the screen in a specified direction, with options for tweening.
