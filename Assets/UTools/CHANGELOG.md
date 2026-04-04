# Changelog

## 0.5.0
- Reorganized the repository into a Git URL friendly UPM package under `Packages/com.roymina.utools`.
- Moved example assets into `Samples~` and declared `com.unity.textmeshpro` as an explicit package dependency.

## 0.2.0
- Reworked `UDIContainer` lifecycle registration, `TryResolve`, and non-lazy binding finalization.
- Updated `UMessageCenter` to use typed subscriptions with disposable handles and pending replay support.
- Hardened `UBehaviour`, `UUtils`, `Highlighter`, and editor drawers against common runtime/editor pitfalls.
- Added package metadata, assembly definitions, and Unity Test Framework entry points.
