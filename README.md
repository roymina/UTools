# UTools

UTools is a lightweight Unity plugin that provides essential utilities and patterns for game development. It includes dependency injection, message center, GameObject/Component finder, and various utility functions to streamline your Unity development workflow.

## Features

### 1. Dependency Injection (UDI)

- Lightweight dependency injection system
- Method and field injection support
- MonoBehaviour class injection support
- Project-wide dependency injection support
- Automatic dependency resolution
- Singleton management

### 2. Message Center (UMessage)

- Event-driven communication system
- Type-safe message passing
- Subscribe/unsubscribe mechanism
- Global and scoped event handling

### 3. GameObject/Component Finder (UFind)

- Easy child GameObject lookup
- Easy attribute-based object finding
- Component caching for better performance
- Hierarchy-based object finding
- Type-safe component access

### 4. Utility Functions (UUtilis)

- Common Unity operation

## Installation

1. Download the UTools package from releases page
2. Import the UTools.unitypackage into your Unity project
3. You're ready to go!

## Quick Start

### Dependency Injection Example

1. Write a class that inherits `UDIInstallerBase` and implement the `RegisterGlobalServices()` and `RegisterSceneServices()` methods, corresponding to current scene injection and global injection respectively

   ```c#
   using UTools;
   
   public class _TestInstaller : UDIInstallerBase
   {
       //register as global service
       protected override void RegisterGlobalServices()
       {
           Container.Register<NormalClass>();
           Container.Register<MonoBehaviourClass>();
       }
   	 //register as current scene service
       protected override void RegisterSceneServices()
       {
           Container.Register<NormalClass>();
           Container.Register<MonoBehaviourClass>();
       }
   }
   ```

   > After loading a new scene for a globally registered service, the injected class still exists and will not be destroyed.
   >
   > If a MonoBehaviour class already exists in the scene, this class will be automatically found and registered. Otherwise, a new object with the same name will be created and this class will be mounted.

2. Use [Inject] attribute to inject services to any class.

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

3. You can also use the `[attribute]` attribute on any method, and the dependency will be injected into this class as a parameter

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

   > Note that in the above example, `TestServiceB` will be automatically registered, and there is no need to use the `Container.Register()` method to register it.
