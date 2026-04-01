---
name: unity-xr
description: "XR Interaction Toolkit skills for VR/AR development. Use when users want to set up XR rigs, add grab interactions, configure teleportation, continuous locomotion, XR UI, or diagnose XR scenes. Triggers: XR, VR, AR, grab, teleport, XR Origin, XR Rig, hand tracking, controller, interactor, interactable, locomotion, 抓取, 传送, 手柄, 头盔, 虚拟现实. Requires com.unity.xr.interaction.toolkit package. Reflection-based: compatible with XRI 2.x (Unity 2022) and XRI 3.x (Unity 6+)."
---

# Unity XR Interaction Toolkit Skills

> **Requires**: `com.unity.xr.interaction.toolkit` (2.x or 3.x). If not installed, all skills return an install prompt.
> **Cross-version**: All API calls use reflection — compatible with XRI 2.x (Unity 2022) and 3.x (Unity 6+) without recompilation.

> **MANDATORY**: Read this document BEFORE calling any `xr_*` skill. It contains verified property names, Collider rules, and class name references extracted from XRI source code. Using incorrect property names causes **silent failures** (reflection returns false, no error). The "Guardrails" section at the bottom lists common hallucinated class names that DO NOT EXIST in XRI.

## Skills Overview

| Skill | Category | Description |
|-------|----------|-------------|
| `xr_check_setup` | Setup | Comprehensive XR project validation |
| `xr_setup_rig` | Setup | Create complete XR Origin rig with Camera and Controllers |
| `xr_setup_interaction_manager` | Setup | Add or get XRInteractionManager |
| `xr_setup_event_system` | Setup | Set up EventSystem with XRUIInputModule |
| `xr_get_scene_report` | Diagnostic | Generate XR scene diagnostic report |
| `xr_add_ray_interactor` | Interactor | Add XRRayInteractor with line visual |
| `xr_add_direct_interactor` | Interactor | Add XRDirectInteractor for close-range grab |
| `xr_add_socket_interactor` | Interactor | Add XRSocketInteractor for snap-to slots |
| `xr_list_interactors` | Query | List all interactors in scene |
| `xr_add_grab_interactable` | Interactable | Make object grabbable (Rigidbody + Collider + config) |
| `xr_add_simple_interactable` | Interactable | Add simple hover/select interactable |
| `xr_configure_interactable` | Interactable | Configure interactable properties |
| `xr_list_interactables` | Query | List all interactables in scene |
| `xr_setup_teleportation` | Locomotion | Add TeleportationProvider to XR Origin |
| `xr_add_teleport_area` | Locomotion | Create teleportation area on surface |
| `xr_add_teleport_anchor` | Locomotion | Create teleport anchor at position |
| `xr_setup_continuous_move` | Locomotion | Add continuous joystick movement |
| `xr_setup_turn_provider` | Locomotion | Add snap or continuous turn |
| `xr_setup_ui_canvas` | UI | Make Canvas XR-compatible |
| `xr_configure_haptics` | Feedback | Configure haptic feedback on interactor |
| `xr_add_interaction_event` | Event | Wire up interaction event callbacks |
| `xr_configure_interaction_layers` | Config | Configure interaction layer mask |

---

## Quick Start: Complete VR Scene in 5 Steps

```python
import unity_skills as u

# Step 1: Validate project
u.call_skill("xr_check_setup")

# Step 2: Create XR rig
u.call_skill("xr_setup_rig", name="XR Origin")

# Step 3: Add interactors to controllers
u.call_skill("xr_add_ray_interactor", name="Right Controller")
u.call_skill("xr_add_direct_interactor", name="Left Controller")

# Step 4: Set up locomotion
u.call_skill("xr_setup_teleportation")
u.call_skill("xr_setup_turn_provider", turnType="Snap", turnAmount=45)

# Step 5: Make objects interactive
u.call_skill("xr_add_grab_interactable", name="MyCube", movementType="VelocityTracking")
```

---

## Workflow 1: XR Rig Setup (Project Initialization)

### When to use
Starting a new VR/AR project, or converting an existing scene to XR.

### Step-by-step

```python
# 1. Check current state — identifies missing components
result = u.call_skill("xr_check_setup", verbose=True)
# Returns: xriVersion, interactionManagerCount, xrOriginCount, mainCamera, issues[]

# 2. Create the XR Origin rig (Camera + Controllers hierarchy)
u.call_skill("xr_setup_rig", name="XR Origin", cameraYOffset=1.36)
# Creates:
#   XR Origin (XROrigin + XRInteractionManager)
#   ├── Camera Offset (y=1.36)
#   │   └── Main Camera (Camera + AudioListener + TrackedPoseDriver)
#   ├── Left Controller (TrackedPoseDriver)
#   └── Right Controller (TrackedPoseDriver)

# 3. Ensure EventSystem has XR input
u.call_skill("xr_setup_event_system")
# Replaces StandaloneInputModule with XRUIInputModule

# 4. Add interactors to controllers
u.call_skill("xr_add_ray_interactor", name="Right Controller", maxDistance=30)
u.call_skill("xr_add_direct_interactor", name="Left Controller", radius=0.1)
```

### Checklist after setup
- `xr_check_setup` returns 0 issues
- Scene has exactly 1 XRInteractionManager
- Scene has exactly 1 XR Origin with MainCamera
- Both controllers have at least one interactor
- EventSystem has XRUIInputModule

### Common Issues
| Problem | Cause | Fix |
|---------|-------|-----|
| "No XRInteractionManager" | Missing manager | `xr_setup_interaction_manager` |
| "No XR Origin" | Missing rig | `xr_setup_rig` |
| "EventSystem lacks XRUIInputModule" | Old input module | `xr_setup_event_system` |
| Objects not interactive | No collider | Ensure target has Collider |

---

## Workflow 2: Grab Interaction

### Basic Direct Grab (close range)

```python
# On the controller — add direct interactor
u.call_skill("xr_add_direct_interactor", name="Left Controller", radius=0.1)

# On the object — make it grabbable
u.call_skill("xr_add_grab_interactable", name="MyCube",
    movementType="VelocityTracking",
    throwOnDetach=True,
    useGravity=True)
```

### Remote Ray Grab (point and grab from distance)

```python
# On the controller — add ray interactor
u.call_skill("xr_add_ray_interactor", name="Right Controller",
    maxDistance=30, lineType="StraightLine")

# On the object — same grab setup
u.call_skill("xr_add_grab_interactable", name="DistantObject",
    movementType="Instantaneous")
```

### Socket Interaction (snap-to-slot placement)

```python
# Create a socket on a shelf/holder
u.call_skill("xr_add_socket_interactor", name="ItemSlot",
    showHoverMesh=True, recycleDelay=1.0)

# Make the item grabbable (it will snap into the socket when released near it)
u.call_skill("xr_add_grab_interactable", name="KeyItem",
    movementType="VelocityTracking")
```

### movementType Selection Guide

| Type | Behavior | Best For |
|------|----------|----------|
| `VelocityTracking` | Object follows hand via physics velocity. Collides with other objects naturally. | Most general-purpose grab. Items that should bump into walls/tables. |
| `Kinematic` | Object follows hand via Rigidbody.MovePosition. Ignores some physics. | Objects that must NOT get stuck in geometry. UI handles, tools. |
| `Instantaneous` | Object teleports to hand position each frame. No physics delay. | Instant precision. Remote ray grab. Snappy feel. |

### Attach Point Configuration

```python
# Offset where the hand grips the object (local space)
u.call_skill("xr_add_grab_interactable", name="Sword",
    movementType="VelocityTracking",
    attachTransformOffset="0,-0.3,0")  # grip near the handle base
```

### Advanced: Fine-tune grab physics

```python
u.call_skill("xr_configure_interactable", name="HeavyBox",
    movementType="VelocityTracking",
    smoothPosition=True,
    smoothPositionAmount=5,
    smoothRotation=True,
    smoothRotationAmount=5,
    throwOnDetach=True)
```

---

## Workflow 3: Teleportation

### Basic Setup

```python
# 1. Add teleportation provider to the XR Origin
u.call_skill("xr_setup_teleportation")

# 2. Add a teleport ray to one controller
u.call_skill("xr_add_ray_interactor", name="Right Controller",
    lineType="ProjectileCurve")  # curved ray for natural aiming

# 3. Mark surfaces as teleportable
u.call_skill("xr_add_teleport_area", name="Floor",
    matchOrientation="WorldSpaceUp")
```

### Teleport Areas vs Anchors

| Feature | TeleportationArea | TeleportationAnchor |
|---------|------------------|---------------------|
| Landing spot | Anywhere on surface | Fixed point |
| Use case | Floors, open terrain | Specific positions (waypoints, chairs) |
| Orientation | Configurable | Can match anchor rotation |

```python
# Area: player can land anywhere on this floor
u.call_skill("xr_add_teleport_area", name="MainFloor",
    matchOrientation="WorldSpaceUp")

# Anchor: player teleports to this exact spot, facing the specified direction
u.call_skill("xr_add_teleport_anchor",
    name="Desk Seat", x=2, y=0, z=3, rotY=180,
    matchOrientation="TargetUpAndForward")
```

### matchOrientation Options

| Value | Behavior |
|-------|----------|
| `WorldSpaceUp` | Keep player upright, preserve facing direction |
| `TargetUp` | Align player up-axis to surface normal |
| `TargetUpAndForward` | Full orientation match (position + rotation) |
| `None` | No orientation change |

---

## Workflow 4: Continuous Locomotion

### Joystick Movement + Snap Turn

```python
# Smooth movement on left stick
u.call_skill("xr_setup_continuous_move",
    moveSpeed=2.0, enableStrafe=True, enableFly=False)

# Snap turn on right stick
u.call_skill("xr_setup_turn_provider",
    turnType="Snap", turnAmount=45)
```

### Smooth Movement + Smooth Turn

```python
u.call_skill("xr_setup_continuous_move",
    moveSpeed=2.0, enableStrafe=True)

u.call_skill("xr_setup_turn_provider",
    turnType="Continuous", turnSpeed=90)
```

### Comfort Considerations

| Setting | Comfortable | Motion-sick prone |
|---------|------------|-------------------|
| Turn type | Snap (45 deg) | Snap preferred |
| Move speed | 1.5–2.0 m/s | Lower = less sickness |
| Fly mode | Disabled | Avoid for most users |
| Strafe | Enabled | May cause discomfort |

> **Recommendation**: Default to **Snap Turn** + moderate **moveSpeed (2.0)**. Offer smooth turn as an option in settings.

---

## Workflow 5: XR UI

### Make existing Canvas work in VR

```python
# Convert Canvas to World Space + add XR raycaster
u.call_skill("xr_setup_ui_canvas", name="MyCanvas")
# - Sets renderMode = WorldSpace
# - Removes GraphicRaycaster, adds TrackedDeviceGraphicRaycaster
# - Sets scale to 0.001 for readable size

# Ensure XR event system
u.call_skill("xr_setup_event_system")

# Player needs a Ray Interactor to interact with UI
u.call_skill("xr_add_ray_interactor", name="Right Controller")
```

### World Space Canvas Best Practices

| Property | Recommended Value | Reason |
|----------|------------------|--------|
| Scale | 0.001 per axis | 1 pixel = 1mm, readable at arm's length |
| Size | 400×300 (pixels) | ~40cm×30cm real-world |
| Distance | 1.5–2.5m from player | Comfortable reading distance |
| Position Y | 1.3–1.6m | Eye level when standing |
| Facing | Toward player spawn | Auto-visible on start |

---

## Workflow 6: Interaction Events & Haptic Feedback

### Wire up events

```python
# When player grabs the lever, call a method on the door
u.call_skill("xr_add_interaction_event",
    name="Lever",
    eventType="selectEntered",
    targetName="Door",
    targetMethod="Open")
```

### Available Event Types

| Event | Fires When |
|-------|-----------|
| `selectEntered` | Interactor starts selecting (grab/click) |
| `selectExited` | Interactor releases |
| `hoverEntered` | Interactor starts hovering over |
| `hoverExited` | Interactor stops hovering |
| `firstSelectEntered` | First interactor selects (multi-select) |
| `lastSelectExited` | Last interactor releases |
| `activated` | Trigger/activate button pressed while selected |
| `deactivated` | Trigger/activate button released |

### Haptic Feedback

```python
# Strong click on grab, light buzz on hover
u.call_skill("xr_configure_haptics", name="Right Controller",
    selectIntensity=0.7, selectDuration=0.15,
    hoverIntensity=0.1, hoverDuration=0.05)
```

| Intensity | Feel |
|-----------|------|
| 0.0–0.1 | Barely noticeable |
| 0.2–0.4 | Subtle feedback |
| 0.5–0.7 | Clear tactile click |
| 0.8–1.0 | Strong vibration |

---

## XR Component Dependency Map

Understanding which components must coexist:

```
Scene Requirements:
  XRInteractionManager (exactly 1)     ← required by ALL interactors/interactables
  EventSystem + XRUIInputModule        ← required for UI interaction

XR Origin Rig:
  XROrigin                             ← required for locomotion
  ├── Camera (TrackedPoseDriver)       ← required for head tracking
  ├── Left Controller (TrackedPoseDriver)
  │   └── Interactor (Ray/Direct)      ← needs Collider (trigger)
  └── Right Controller (TrackedPoseDriver)
      └── Interactor (Ray/Direct)      ← needs Collider (trigger)

Grabbable Object:
  XRGrabInteractable                   ← needs Rigidbody + Collider
  ├── Rigidbody                        ← physics simulation
  └── Collider (non-trigger)           ← interaction detection

Teleport Target:
  TeleportationArea/Anchor             ← needs Collider for ray detection
  └── Collider                         ← raycast target surface

XR UI Canvas:
  Canvas (WorldSpace)
  ├── TrackedDeviceGraphicRaycaster    ← replaces GraphicRaycaster
  └── UI Elements (Button, etc.)

Locomotion (on XR Origin):
  TeleportationProvider                ← for teleport
  ContinuousMoveProvider               ← for joystick movement
  SnapTurnProvider / ContinuousTurnProvider  ← for rotation
```

---

## Interaction Layer Configuration

Interaction layers control which interactors can interact with which interactables. Use them to separate concerns:

```python
# Example: Teleport ray should only interact with teleport surfaces
u.call_skill("xr_configure_interaction_layers",
    name="TeleportRay", layers="Teleport", isInteractor=True)

u.call_skill("xr_configure_interaction_layers",
    name="Floor", layers="Teleport", isInteractor=False)

# Grab interactor only interacts with grabbable layer
u.call_skill("xr_configure_interaction_layers",
    name="Left Controller", layers="Default", isInteractor=True)
```

---

## Version Compatibility Notes

| Feature | XRI 2.x (Unity 2022) | XRI 3.x (Unity 6+) |
|---------|----------------------|---------------------|
| Package version | 2.6.4 | 3.4.0 |
| Namespace | Root (`UnityEngine.XR.Interaction.Toolkit`) | Sub-namespaces (`.Interactors`, `.Interactables`, `.Locomotion.*`) |
| Rig component | XROrigin (from xr.core-utils) | XROrigin (same) |
| Locomotion system | LocomotionSystem | LocomotionMediator |
| Controller | ActionBasedController | ActionBasedController (moved namespace) |
| New features | — | NearFarInteractor, GravityProvider |
| Deprecated | — | AffordanceSystem |

> **All XR skills handle this automatically** via `XRReflectionHelper` — no manual version handling needed. The helper tries 3.x namespaces first, then falls back to 2.x.

---

## Important Notes

1. **Package required**: All XR skills require `com.unity.xr.interaction.toolkit`. Install via Package Manager.
2. **XR Origin required**: Most locomotion skills auto-find the XR Origin. If none exists, they prompt to create one.
3. **Colliders are essential**: Interactors need trigger Colliders to detect targets. Interactables need Colliders for ray/overlap detection. Skills auto-add appropriate Colliders when missing.
4. **One InteractionManager**: The scene should have exactly one `XRInteractionManager`. Multiple managers cause undefined behavior.
5. **TrackedPoseDriver**: Controllers need `TrackedPoseDriver` for position/rotation tracking. `xr_setup_rig` adds these automatically.
6. **Domain Reload**: Installing the XRI package may trigger a Domain Reload. Wait for Unity to finish compiling before calling XR skills.
7. **Reflection safety**: If a type is not found (wrong XRI version or missing sub-package), skills return a clear error message instead of crashing.
8. **Undo support**: All modification skills register with Unity Undo and Workflow tracking.

---

## API Reference: XRRayInteractor (Complete Properties)

> **Anti-hallucination**: These are the REAL property names from XRI source code. Do NOT invent properties.

### Core Properties

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `maxRaycastDistance` | float | 30 | Maximum ray length in meters |
| `raycastMask` | LayerMask | Everything | Physics layers the ray detects |
| `raycastTriggerInteraction` | QueryTriggerInteraction | Ignore | Whether ray hits trigger colliders |
| `hitClosestOnly` | bool | true | Only return the closest hit |

### Line Configuration

| Property | Type | Values | Description |
|----------|------|--------|-------------|
| `lineType` | LineType enum | `StraightLine`, `ProjectileCurve`, `BezierCurve` | Ray shape |
| `velocity` | float | 16 | Projectile launch velocity (ProjectileCurve only) |
| `acceleration` | float | 9.8 | Gravity for projectile (ProjectileCurve only) |
| `additionalGroundHeight` | float | 0.1 | Extra ground offset for ProjectileCurve |
| `additionalFlightTime` | float | 0.5 | Extra flight time for ProjectileCurve |
| `endPointDistance` | float | 30 | Bezier end point distance |
| `endPointHeight` | float | -10 | Bezier end point drop height |
| `controlPointDistance` | float | 10 | Bezier control point distance |
| `controlPointHeight` | float | 5 | Bezier control point height |
| `sampleFrequency` | int | 20 | Points sampled along curve |

### Hit Detection

| Property | Type | Values | Description |
|----------|------|--------|-------------|
| `hitDetectionType` | HitDetectionType enum | `Raycast`, `SphereCast`, `ConeCast` | Detection method |
| `sphereCastRadius` | float | 0 | Radius for SphereCast |
| `coneCastAngle` | float | 6 | Angle for ConeCast (degrees) |

### UI & Selection

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `enableUIInteraction` | bool | true | Allow interaction with World Space UI |
| `useForceGrab` | bool | true | Pull object to hand (true) or move hand to object (false) |
| `anchorControl` | bool | true | Allow push/pull/rotate of selected object |
| `translateSpeed` | float | 1 | Push/pull speed when anchorControl enabled |
| `rotateSpeed` | float | 180 | Rotation speed when anchorControl enabled |

---

## API Reference: XRInteractorLineVisual

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `lineWidth` | float | 0.02 | Line width in meters |
| `overrideInteractorLineLength` | bool | true | Override ray length for visual |
| `lineLength` | float | 10 | Visual line length override |
| `validColorGradient` | Gradient | white→white | Color when pointing at valid target |
| `invalidColorGradient` | Gradient | red→red | Color when pointing at nothing |
| `blockedColorGradient` | Gradient | yellow | Color when blocked by object |
| `stopLineAtFirstRaycastHit` | bool | true | Truncate line at hit point |
| `stopLineAtSelection` | bool | true | Truncate line at selected object |
| `treatSelectionAsValidState` | bool | true | Show valid color when something selected |
| `smoothMovement` | bool | false | Smooth line endpoint movement |
| `reticle` | GameObject | null | Reticle prefab at hit point |

---

## API Reference: XRGrabInteractable (Complete Properties)

> **Critical**: These property names are verified from source. Using wrong names causes silent failures via reflection.

### Movement

| Property | Type | Values / Default | Description |
|----------|------|-----------------|-------------|
| `movementType` | MovementType enum | `Instantaneous`, `Kinematic`, `VelocityTracking` | How object follows hand |
| `trackPosition` | bool | true | Track interactor position |
| `trackRotation` | bool | true | Track interactor rotation |
| `trackScale` | bool | false | Track interactor scale (multi-hand) |
| `throwOnDetach` | bool | true | Apply velocity on release |
| `forceGravityOnDetach` | bool | false | Force Rigidbody.useGravity=true on release |

### Smoothing (reduce jitter)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `smoothPosition` | bool | true | Enable position smoothing |
| `smoothPositionAmount` | float | 5 | Smoothing factor (higher = smoother, more lag) |
| `tightenPosition` | float | 0.5 | Tighten position to target (0=loose, 1=tight) |
| `smoothRotation` | bool | true | Enable rotation smoothing |
| `smoothRotationAmount` | float | 5 | Rotation smoothing factor |
| `tightenRotation` | float | 0.5 | Tighten rotation to target |

### Throw Physics

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `throwSmoothingDuration` | float | 0.25 | Window for averaging throw velocity |
| `throwSmoothingCurve` | AnimationCurve | — | Weight curve for velocity samples |
| `throwVelocityScale` | float | 1.5 | Multiply throw velocity |
| `throwAngularVelocityScale` | float | 1 | Multiply throw angular velocity |

### Attach

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `attachTransform` | Transform | null | Custom grab point (null = object center) |
| `secondaryAttachTransform` | Transform | null | Second hand attach point |
| `useDynamicAttach` | bool | false | Grab at contact point instead of center |
| `attachEaseInTime` | float | 0 | Ease-in duration when grabbed (seconds) |
| `snapToColliderVolume` | bool | true | Snap grab to nearest collider surface |
| `reinitializeEveryGrab` | bool | true | Reset attach on each new grab |

### Selection

| Property | Type | Values | Description |
|----------|------|--------|-------------|
| `selectMode` | InteractableSelectMode enum | `Single`, `Multiple` | Allow multi-hand grab |

### Grab Transformers

| Property | Type | Description |
|----------|------|-------------|
| `startingSingleGrabTransformers` | List | Transformers for single-hand grab |
| `startingMultipleGrabTransformers` | List | Transformers for two-hand grab |
| `addDefaultGrabTransformers` | bool | Auto-add XRGeneralGrabTransformer |

---

## API Reference: XRSocketInteractor (Complete Properties)

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `showInteractableHoverMeshes` | bool | true | Show ghost mesh when hovering |
| `interactableHoverMeshMaterial` | Material | null | Material for hover ghost |
| `interactableCantHoverMeshMaterial` | Material | null | Material when can't hover |
| `socketActive` | bool | true | Enable/disable socket |
| `recycleDelayTime` | float | 1 | Cooldown before socket accepts new object |
| `socketSnappingRadius` | float | 0.1 | How close object must be to snap |
| `socketScaleMode` | SocketScaleMode enum | `None` | `None`, `Fixed`, `StretchedToFitSize` |
| `targetBoundsSize` | Vector3 | (1,1,1) | Target size for scaled objects |
| `startingSelectedInteractable` | XRBaseInteractable | null | Pre-placed object in socket |

---

## API Reference: XRDirectInteractor

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `improveRaycastBySortingOrder` | bool | true | Sort results by distance |
| `keepSelectedTargetValid` | bool | true | Keep selected object valid even outside range |

> **Collider requirement**: XRDirectInteractor MUST have a **trigger Collider** on the same GameObject. Use `SphereCollider` with `isTrigger=true`, radius 0.1–0.25m.

---

## Collider Configuration Matrix (CRITICAL)

> **Most common XR setup error**: Wrong Collider trigger setting. This table is authoritative.

| Component | Collider Required | isTrigger | Recommended Type | Why |
|-----------|:-:|:-:|---------|-----|
| **XRDirectInteractor** | YES | **TRUE** | SphereCollider (r=0.1–0.25) | Detects overlap with interactables |
| **XRRayInteractor** | NO | — | None needed | Uses raycast, not collider |
| **XRSocketInteractor** | YES | **TRUE** | SphereCollider (r=0.1–0.3) | Defines snap-to zone |
| **XRGrabInteractable** | YES | **FALSE** | BoxCollider / MeshCollider(convex) | Physics interaction + ray target |
| **XRSimpleInteractable** | YES | **FALSE** | BoxCollider | Ray/overlap detection |
| **TeleportationArea** | YES | **FALSE** | MeshCollider / BoxCollider | Raycast target surface |
| **TeleportationAnchor** | YES | **FALSE** | BoxCollider (thin, 1×0.01×1) | Raycast hit zone |

### Critical Rules

1. **XRGrabInteractable requires Rigidbody** — without it, physics-based movementType won't work
2. **XRGrabInteractable Collider must NOT be trigger** — trigger colliders cannot participate in physics simulation
3. **XRDirectInteractor Collider MUST be trigger** — it uses OnTriggerStay to detect interactables
4. **MeshCollider on grabbables must be convex=true** — non-convex MeshCollider cannot be Dynamic Rigidbody
5. **Socket Collider must be trigger** — overlap detection, not physics blocking

### Common Mistakes

```
❌ GrabInteractable + Collider(isTrigger=true) → Object cannot be grabbed
❌ GrabInteractable without Rigidbody → VelocityTracking silently fails
❌ DirectInteractor + Collider(isTrigger=false) → Cannot detect any targets
❌ MeshCollider(convex=false) on Rigidbody → Unity error, physics broken
❌ Socket Collider(isTrigger=false) → Objects bounce off instead of snapping
```

---

## API Reference: Event System (Complete)

### Interactable Events (on XRBaseInteractable / XRGrabInteractable)

| Event Property | Event Args Type | When Fired |
|---------------|----------------|------------|
| `firstHoverEntered` | HoverEnterEventArgs | First interactor starts hovering |
| `hoverEntered` | HoverEnterEventArgs | Any interactor starts hovering |
| `hoverExited` | HoverExitEventArgs | Any interactor stops hovering |
| `lastHoverExited` | HoverExitEventArgs | Last interactor stops hovering |
| `firstSelectEntered` | SelectEnterEventArgs | First interactor selects (grabs) |
| `selectEntered` | SelectEnterEventArgs | Any interactor selects |
| `selectExited` | SelectExitEventArgs | Any interactor deselects (releases) |
| `lastSelectExited` | SelectExitEventArgs | Last interactor deselects |
| `activated` | ActivateEventArgs | Trigger pressed while selected |
| `deactivated` | DeactivateEventArgs | Trigger released while selected |

### Event Args Properties

```csharp
// SelectEnterEventArgs
args.interactorObject   // IXRSelectInteractor — who grabbed
args.interactableObject // IXRSelectInteractable — what was grabbed

// SelectExitEventArgs
args.interactorObject   // IXRSelectInteractor
args.interactableObject // IXRSelectInteractable
args.isCanceled         // bool — was it force-canceled (not natural release)

// HoverEnterEventArgs / HoverExitEventArgs
args.interactorObject   // IXRHoverInteractor
args.interactableObject // IXRHoverInteractable

// ActivateEventArgs / DeactivateEventArgs
args.interactorObject   // IXRActivateInteractor
args.interactableObject // IXRActivateInteractable
```

### Usage Pattern (from XRI source examples)

```csharp
// CORRECT pattern — subscribe in OnEnable, unsubscribe in OnDisable
protected override void OnEnable()
{
    base.OnEnable();
    selectEntered.AddListener(OnGrab);
    selectExited.AddListener(OnRelease);
}

protected override void OnDisable()
{
    selectEntered.RemoveListener(OnGrab);
    selectExited.RemoveListener(OnRelease);
    base.OnDisable();
}

// CORRECT — use ProcessInteractable for continuous updates
public override void ProcessInteractable(XRInteractionUpdateOrder.UpdatePhase updatePhase)
{
    base.ProcessInteractable(updatePhase);
    if (updatePhase == XRInteractionUpdateOrder.UpdatePhase.Dynamic)
    {
        if (isSelected)
        {
            // Track hand position, update slider/knob/etc.
            var attachPos = m_Interactor.GetAttachTransform(this).position;
        }
    }
}
```

### UpdatePhase Values

| Phase | Use Case |
|-------|----------|
| `Dynamic` | **Primary**: Position updates, interaction logic |
| `Fixed` | Physics-dependent calculations |
| `Late` | Post-processing, visual follow |
| `OnBeforeRender` | Last-moment visual sync for VR rendering |

---

## API Reference: TrackedPoseDriver

> **Namespace**: `UnityEngine.InputSystem.XR.TrackedPoseDriver` (Input System package)
> **Legacy fallback**: `UnityEngine.SpatialTracking.TrackedPoseDriver`

| Property | Type | Values | Description |
|----------|------|--------|-------------|
| `trackingType` | TrackingType enum | `RotationAndPosition`, `RotationOnly`, `PositionOnly` | What to track |
| `updateType` | UpdateType enum | `UpdateAndBeforeRender`, `Update`, `BeforeRender` | When to update |
| `positionInput` | InputActionProperty | — | Position input action |
| `rotationInput` | InputActionProperty | — | Rotation input action |

### Typical Bindings

```
Camera:           <XRHMD>/centerEyePosition, <XRHMD>/centerEyeRotation
Left Controller:  <XRController>{LeftHand}/devicePosition, deviceRotation
Right Controller: <XRController>{RightHand}/devicePosition, deviceRotation
```

---

## API Reference: Input Action Map (XRI Default)

> **Source**: XRI Default Input Actions.inputactions (from Starter Assets)

### XRI LeftHand / RightHand Interaction

| Action | Type | Typical Binding | Use |
|--------|------|----------------|-----|
| Select | Button | `{Grip}` | Grab / Select |
| Select Value | Axis | `{Grip}` | Grip analog value |
| Activate | Button | `{Trigger}` | Trigger / Activate |
| Activate Value | Axis | `{Trigger}` | Trigger analog value |
| UI Press | Button | `{Trigger}` | UI click |
| Rotate Anchor | Vector2 | `{Primary2DAxis}` | Rotate held object |
| Translate Anchor | Vector2 | `{Primary2DAxis}` | Push/pull held object |

### XRI LeftHand / RightHand Locomotion

| Action | Type | Typical Binding | Use |
|--------|------|----------------|-----|
| Move | Vector2 | `{Primary2DAxis}` | Continuous movement |
| Turn | Vector2 | `{Primary2DAxis}` | Continuous rotation |
| Snap Turn | Vector2 | `{Primary2DAxis}` | Snap rotation |
| Teleport Mode Activate | Vector2 | `{Primary2DAxis}` | Activate teleport ray |
| Teleport Mode Cancel | Button | `{GripButton}` | Cancel teleport |
| Grab Move | Button | `{GripButton}` | Hand-based locomotion |

### Standard Controller Layout

```
Left Controller:                    Right Controller:
┌──────────────────┐               ┌──────────────────┐
│   [Thumbstick]   │ ← Move       │   [Thumbstick]   │ ← Turn/Teleport
│                  │               │                  │
│ [Grip] ← Select │               │ [Grip] ← Select  │
│                  │               │                  │
│ [Trigger]←Activate              │ [Trigger]←Activate│
└──────────────────┘               └──────────────────┘
```

---

## Guardrails: Preventing AI Hallucination

**Mode**: Full-Auto required (requires `com.unity.xr.interaction.toolkit` package)

### DO NOT assume these exist (they don't in XRI)

```
❌ XRHand                    → Use XRController or ActionBasedController
❌ XRPlayer                  → Use XR Origin (XROrigin)
❌ XRTeleporter              → Use TeleportationProvider + TeleportationArea/Anchor
❌ GrabInteractor            → Use XRDirectInteractor or XRRayInteractor
❌ XRButton                  → Use XRSimpleInteractable + custom script
❌ VRController              → Use ActionBasedController
❌ XRLocomotion              → Use ContinuousMoveProvider / TeleportationProvider
❌ interactable.OnGrab()     → Use selectEntered event
❌ interactable.OnRelease()  → Use selectExited event
❌ controller.vibrate()      → Use haptic properties on interactor
❌ XRManager                 → Use XRInteractionManager
```

### Correct class names (verified from source)

```
✅ XRInteractionManager      (exactly 1 per scene)
✅ XROrigin                   (from Unity.XR.CoreUtils)
✅ XRRayInteractor            (remote interaction)
✅ XRDirectInteractor          (close-range interaction)
✅ XRSocketInteractor          (snap-to placement)
✅ XRGrabInteractable          (grabbable object)
✅ XRSimpleInteractable        (basic hover/select)
✅ TeleportationProvider       (on XR Origin)
✅ TeleportationArea           (surface teleport target)
✅ TeleportationAnchor         (point teleport target)
✅ ContinuousMoveProvider      (joystick movement)
✅ SnapTurnProvider             (snap rotation)
✅ ContinuousTurnProvider      (smooth rotation)
✅ TrackedDeviceGraphicRaycaster (XR UI raycast)
✅ XRUIInputModule              (XR event system)
✅ ActionBasedController        (Input System controller)
✅ TrackedPoseDriver            (position/rotation tracking)
✅ XRInteractorLineVisual       (ray line rendering)
```

### Script writing rules for XR

1. **Inherit from XRBaseInteractable** for custom interactive objects, NOT MonoBehaviour
2. **Use `ProcessInteractable(UpdatePhase.Dynamic)`** for continuous tracking, NOT Update()
3. **Use `selectEntered`/`selectExited` events**, NOT OnTriggerEnter/OnCollisionEnter
4. **Use `GetAttachTransform(this).position`** to get hand position, NOT controller.transform.position
5. **Use `isSelected` / `isHovered`** readonly properties, NOT custom flags
6. **Always call `base.OnEnable()` / `base.OnDisable()`** when overriding in interactable subclasses
7. **Use InteractionLayerMask** for filtering, NOT physics Layer or Tag

---

## Load Related Advisory Modules When Needed

- Architecture & module design: see [`../architecture/SKILL.md`](../architecture/SKILL.md)
- Performance review (Update, allocations): see [`../performance/SKILL.md`](../performance/SKILL.md)
- Script quality (coupling, naming): see [`../scriptdesign/SKILL.md`](../scriptdesign/SKILL.md)
- Async & lifecycle (coroutine vs UniTask in XR): see [`../async/SKILL.md`](../async/SKILL.md)
- Design patterns (Observer for XR events, State Machine for locomotion modes): see [`../patterns/SKILL.md`](../patterns/SKILL.md)

