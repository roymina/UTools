# UTools

UTools is a lightweight Unity toolkit focused on four areas:

- `UDI`: field/property/method injection and runtime lifecycle hooks
- `UMessage`: typed publish/subscribe with pending replay support
- `UFind`: attribute-driven child/component/resource lookup
- `UUtils`: Unity-oriented helper extensions

## Recommended Usage

1. Add a `UDIContext` to a scene root.
2. Attach `MonoInstaller` or `ScriptableObjectInstaller` instances.
3. Register services through `container.Bind<T>()`.
4. Use `[Inject]`, `[Child]`, `[Comp]`, and `[Resource]` in runtime scripts.

## Compatibility Notes

- `UDIContext` is the only recommended DI entry point.
- Constructor injection is not supported.
- Identifier-based resolution is not part of the recommended API surface.

## Testing

Use the Unity Test Runner for:

- `Packages/com.roymina.utools/Tests/EditMode`
- `Packages/com.roymina.utools/Tests/PlayMode`
