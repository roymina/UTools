# Changelog

## 1.0.1
- Added `PointerEventListener` for dependency-free pointer click, hover, press, and drag handling in `UTools.Runtime`.
- Added transient `UMessageCenter.Publish(..., cacheIfNoSubscribers: false)` support so pointer events can broadcast globally without replaying stale messages.
- Added EditMode coverage for transient publish behavior and the new pointer listener callbacks and drag delta forwarding.

## 1.0.0
- Stabilized `UDI` scene startup with global fallback injection and `AsyncWaitRoot` subtree gating for required async services.
- Added `UFind` `[Children]` binding for collecting child GameObjects or component lists from a named parent.
- Refactored `UUtils` into clearer focused utility classes while keeping the same Unity-oriented helper surface.
- Polished the root documentation and usage guides for `UDI`, `UFind`, `UMessage`, `UUtils`, and editor helpers.

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
