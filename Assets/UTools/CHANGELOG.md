# Changelog

## 0.5.1
- Restored the Unity project layout so example scenes continue to run from `Assets/UTools/Example`.
- Kept Git URL installation support by exposing `Assets/UTools` as the package root with `?path=/Assets/UTools`.
- Clarified installation documentation and package metadata for the hybrid project-plus-package workflow.

## 0.5.0
- Reorganized the repository into a Git URL friendly UPM package under `Packages/com.roymina.utools`.
- Moved example assets into `Samples~` and declared `com.unity.textmeshpro` as an explicit package dependency.

## 0.2.0
- Reworked `UDIContainer` lifecycle registration, `TryResolve`, and non-lazy binding finalization.
- Updated `UMessageCenter` to use typed subscriptions with disposable handles and pending replay support.
- Hardened `UBehaviour`, `UUtils`, `Highlighter`, and editor drawers against common runtime/editor pitfalls.
- Added package metadata, assembly definitions, and Unity Test Framework entry points.
