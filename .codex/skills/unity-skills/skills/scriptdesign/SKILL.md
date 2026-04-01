---
name: unity-scriptdesign
description: "Script quality advisor for Unity gameplay code. Use when users want code review, reduce coupling, improve maintainability, or refactor scripts. Triggers: code review, coupling, maintainability, refactor, clean code, script quality, review my code, code smell, 代码审查, 代码质量, 重构, 低耦合, 可维护, 看看代码, 代码有问题."
---

# Unity Script Design Review

Use this skill before creating gameplay scripts, or after scripts are generated and need a design pass.

## Review Checklist

- Responsibility: does the script have one clear job?
- Role: should it really be a `MonoBehaviour`, `ScriptableObject`, or plain C# class?
- Coupling: are dependencies explicit instead of hidden globals or deep scene lookups?
- Communication: should this be a direct reference, interface call, or event?
- Performance: is there unnecessary `Update`, repeated `Find`, avoidable allocation, or reflection in hot paths?
- Lifecycle: are subscriptions, timers, and async work cleaned up clearly?
- Inspector UX: are serialized fields private, grouped, and explained?
- Testability: can the core logic move into a plain C# class?
- Naming: do class and field names explain intent without cryptic abbreviations?

## Guardrails

**Mode**: Both (Semi-Auto + Full-Auto) — advisory only, no REST skills

- Prefer descriptive names over local shorthand.
- Do not “optimize” readability away for imagined productivity gains.
- Do not recommend complex patterns if a smaller refactor fixes the real problem.

## Output Format

- Keep: what is already good
- Simplify: what should stay straightforward
- Refactor: the highest-value structural change
- Performance notes: only real hotspots, not theoretical micro-optimizations
- Maintainability notes: naming, ownership, coupling, editor usability
