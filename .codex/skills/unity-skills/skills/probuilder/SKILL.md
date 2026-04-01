---
name: unity-probuilder
description: "ProBuilder mesh modeling. Use when users want to create ProBuilder shapes, extrude faces, bevel edges, subdivide meshes, or perform procedural mesh operations. Triggers: ProBuilder, mesh modeling, extrude, bevel, subdivide, 建模, 拉伸, 倒角, 细分. Requires com.unity.probuilder package."
---

# Unity ProBuilder Skills

> **Requires**: `com.unity.probuilder` package (5.x+). If not installed, all skills return an install prompt.

## Guardrails

**Mode**: Full-Auto required (requires `com.unity.probuilder` package)

**DO NOT** (common hallucinations):
- `probuilder_create_mesh` does not exist → use `probuilder_create_shape` with `shapeType` parameter
- `probuilder_edit_face` does not exist → use specific face operations: `probuilder_extrude_faces`, `probuilder_delete_faces`, `probuilder_subdivide`
- `probuilder_set_material` does not exist → use `probuilder_set_face_material` (per-face) or `probuilder_set_object_material` (whole object)
- All mesh operations require `ToMesh()` + `Refresh()` — skills handle this automatically

**Routing**:
- For basic primitive GameObjects (without ProBuilder) → use `gameobject_create` with primitiveType
- For material assignment → `probuilder_set_face_material` / `probuilder_set_object_material` (this module), or `material_assign` (material module)

## Skills Overview

| Skill | Category | Description |
|-------|----------|-------------|
| `probuilder_create_shape` | Create | Create parametric ProBuilder shape |
| `probuilder_extrude_faces` | Face | Extrude faces along normals |
| `probuilder_delete_faces` | Face | Delete faces by index |
| `probuilder_merge_faces` | Face | Merge multiple faces into one |
| `probuilder_flip_normals` | Face | Flip face normal direction |
| `probuilder_detach_faces` | Face | Detach faces (split from mesh) |
| `probuilder_set_face_material` | Face | Assign material to specific faces |
| `probuilder_bevel_edges` | Edge | Bevel (chamfer) edges |
| `probuilder_extrude_edges` | Edge | Extrude edges outward (walls, rails) |
| `probuilder_bridge_edges` | Edge | Bridge two edges with a face (doorways) |
| `probuilder_subdivide` | Mesh | Subdivide mesh or selected faces |
| `probuilder_conform_normals` | Mesh | Make normals point consistently outward |
| `probuilder_combine_meshes` | Mesh | Combine multiple meshes into one |
| `probuilder_move_vertices` | Vertex | Move vertices by delta (ramps, slopes) |
| `probuilder_set_vertices` | Vertex | Set absolute vertex positions |
| `probuilder_get_vertices` | Query | Get vertex positions by index |
| `probuilder_weld_vertices` | Vertex | Weld nearby vertices within radius |
| `probuilder_project_uv` | UV | Box-project UVs onto faces |
| `probuilder_get_info` | Query | Get mesh info (vertices, faces, edges, materials) |
| `probuilder_center_pivot` | Transform | Center or reposition mesh pivot |
| `probuilder_create_batch` | Batch | Batch create multiple shapes (level design) |
| `probuilder_set_material` | Material | Set material on entire mesh (color shortcut) |

---

## Shape Creation

### probuilder_create_shape
Create a ProBuilder primitive shape with parametric size and position.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `shape` | string | No | "Cube" | Shape type (see below) |
| `name` | string | No | auto | GameObject name |
| `x/y/z` | float | No | 0 | World position |
| `sizeX/sizeY/sizeZ` | float | No | 1 | Shape dimensions |
| `rotX/rotY/rotZ` | float | No | 0 | Euler rotation |

**Available shapes**: `Cube`, `Sphere`, `Cylinder`, `Cone`, `Torus`, `Prism`, `Arch`, `Pipe`, `Stairs`, `Door`, `Plane`

**Returns**: `{success, name, instanceId, shape, position, size, vertexCount, faceCount}`

---

## Face Operations

### probuilder_extrude_faces
Extrude faces outward or inward along normals.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices, e.g. `"0,1,2"` |
| `distance` | float | No | 0.5 | Extrusion distance (negative = inward) |
| `method` | string | No | "FaceNormal" | `IndividualFaces` / `FaceNormal` / `VertexNormal` |

**Extrude methods**:
- `IndividualFaces` — each face extrudes independently, creating gaps between
- `FaceNormal` — faces extrude as a group along the averaged normal
- `VertexNormal` — faces extrude as a group along individual vertex normals (smoother)

**Returns**: `{success, name, extrudedFaceCount, method, distance, totalFaces, totalVertices}`

---

### probuilder_delete_faces
Delete faces from a ProBuilder mesh by index.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | Yes | — | Comma-separated face indices, e.g. `"0,1,2"` |

**Returns**: `{success, name, deletedCount, remainingFaces, remainingVertices}`

---

### probuilder_merge_faces
Merge multiple faces into a single face.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices (min 2) |

**Returns**: `{success, name, mergedFromCount, totalFaces, totalVertices}`

---

### probuilder_flip_normals
Flip face normals (reverses winding order).

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices |

**Returns**: `{success, name, flippedCount}`

---

### probuilder_detach_faces
Detach faces from a mesh — split shared vertices so faces can move independently.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices |
| `deleteSourceFaces` | bool | No | false | Delete the original faces after detaching |

**Returns**: `{success, name, detachedFaceCount, deleteSourceFaces, totalFaces, totalVertices}`

---

### probuilder_set_face_material
Assign a material to specific faces of a ProBuilder mesh.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices |
| `materialPath` | string | No** | — | Material asset path |
| `submeshIndex` | int | No** | -1 | Submesh index directly |

**Provide either `materialPath` or `submeshIndex`.

**Returns**: `{success, name, affectedFaces, materialCount}`

---

## Edge Operations

### probuilder_bevel_edges
Bevel (chamfer) edges to create smooth transitions.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `edgeIndexes` | string | No | all | Vertex index pairs, e.g. `"0-1,2-3"` |
| `amount` | float | No | 0.2 | Bevel size (0–1, relative to edge length) |

**Returns**: `{success, name, beveledEdgeCount, newFaceCount, amount, totalFaces, totalVertices}`

---

### probuilder_extrude_edges
Extrude edges outward to create walls, rails, or flanges.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `edgeIndexes` | string | Yes | — | Vertex index pairs, e.g. `"0-1,2-3"` |
| `distance` | float | No | 0.5 | Extrusion distance |
| `extrudeAsGroup` | bool | No | true | Extrude edges as a connected group |
| `enableManifoldExtrude` | bool | No | false | Allow non-manifold extrusion |

**Returns**: `{success, name, extrudedEdgeCount, newEdgeCount, distance, totalFaces, totalVertices}`

---

### probuilder_bridge_edges
Bridge two edges with a new face. Use to create doorways, windows, or connect geometry.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `edgeA` | string | Yes | — | First edge, e.g. `"0-1"` |
| `edgeB` | string | Yes | — | Second edge, e.g. `"4-5"` |
| `allowNonManifold` | bool | No | false | Allow non-manifold geometry |

**Returns**: `{success, name, bridgedEdge, totalFaces, totalVertices}`

---

## Mesh Operations

### probuilder_subdivide
Subdivide a mesh or selected faces (adds detail by splitting faces).

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices |

**Returns**: `{success, name, totalFaces, totalVertices}`

---

### probuilder_conform_normals
Make all face normals point consistently outward. Fixes inverted faces after complex edits.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices |

**Returns**: `{success, name, status, notification, faceCount}`

---

## Query & Transform

### probuilder_get_info
Get detailed information about a ProBuilder mesh.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |

**Returns**: `{name, instanceId, isProBuilder, vertexCount, faceCount, edgeCount, triangleCount, shapeType, position, bounds, materials[], submeshFaceCounts[]}`

---

### probuilder_center_pivot
Center the pivot point or move it to a specific world position.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | GameObject name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `worldX/worldY/worldZ` | float | No | center | Target world position (omit all for center) |

**Returns**: `{success, name, pivot}`

---

## Quick Example: Table (0.8m tall)

```python
# Table top: 1.2×0.6m surface at 0.8m height, 0.05m thick
# y = 0.8 - 0.05/2 = 0.775 (center of slab, top surface at 0.8m)
unity_skills.call_skill("probuilder_create_shape",
    shape="Cube", name="TableTop", sizeX=1.2, sizeY=0.05, sizeZ=0.6, y=0.775)

# 4 legs: 0.04m thick, 0.75m tall (table top bottom at 0.75)
# y = 0.75/2 = 0.375 (centered vertically)
for i, (lx, lz) in enumerate([(-0.5,-0.22), (0.5,-0.22), (-0.5,0.22), (0.5,0.22)]):
    unity_skills.call_skill("probuilder_create_shape",
        shape="Cylinder", name=f"Leg_{i}", sizeX=0.04, sizeY=0.75, sizeZ=0.04,
        x=lx, y=0.375, z=lz)
```

---

## Batch & Level Design Skills

### probuilder_create_batch
Batch create multiple ProBuilder shapes in one call. Essential for level design — create entire scenes efficiently.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `items` | string | Yes | — | JSON array of shape configs |
| `defaultParent` | string | No | null | Default parent for all items |

**Item properties**: `shape`, `name`, `x`, `y`, `z`, `sizeX`, `sizeY`, `sizeZ`, `rotX`, `rotY`, `rotZ`, `parent`, `materialPath`

```python
# Create a parkour course in one call
unity_skills.call_skill("probuilder_create_batch", items=[
    {"shape": "Cube", "name": "Ground", "sizeX": 30, "sizeY": 0.5, "sizeZ": 10, "y": -0.25},
    {"shape": "Cube", "name": "Platform_1", "sizeX": 3, "sizeY": 0.3, "sizeZ": 3, "x": 5, "y": 2},
    {"shape": "Cube", "name": "Platform_2", "sizeX": 2, "sizeY": 0.3, "sizeZ": 2, "x": 9, "y": 3.5},
    {"shape": "Stairs", "name": "Stairs_1", "sizeX": 2, "sizeY": 3, "sizeZ": 4, "x": -3},
    {"shape": "Cylinder", "name": "Pillar_1", "sizeX": 0.5, "sizeY": 4, "sizeZ": 0.5, "x": 12, "y": 2},
    {"shape": "Arch", "name": "Arch_1", "sizeX": 4, "sizeY": 3, "sizeZ": 1, "x": 15}
])
```

---

## Vertex Editing Skills

### probuilder_weld_vertices
Weld (merge) nearby vertices within a radius threshold. Cleans up mesh after complex edits.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | Mesh name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `vertexIndexes` | string | Yes | — | Comma-separated vertex indices |
| `radius` | float | No | 0.01 | Merge radius (vertices within this distance are welded) |

**Returns**: `{success, name, inputVertexCount, weldedVertexCount, radius, totalVertices}`

---

### probuilder_move_vertices
Move vertices by a delta offset. Use to transform cubes into ramps, slopes, or custom shapes.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | Mesh name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `vertexIndexes` | string | Yes | — | Comma-separated vertex indices |
| `deltaX/deltaY/deltaZ` | float | No | 0 | Movement offset |

**Tip**: Use `probuilder_get_vertices` first to identify which vertices to move. For a default Cube, vertices 4-7 are typically the top face.

```python
# Create a ramp: move top-right vertices of a Cube forward
unity_skills.call_skill("probuilder_create_shape", shape="Cube", name="Ramp", sizeX=3, sizeY=1, sizeZ=5)
verts = unity_skills.call_skill("probuilder_get_vertices", name="Ramp")
# Move top vertices down on one side to create slope
unity_skills.call_skill("probuilder_move_vertices", name="Ramp", vertexIndexes="...", deltaY=-0.8)
```

### probuilder_set_vertices
Set absolute positions for specific vertices.

| Parameter | Type | Required | Description |
|-----------|------|----------|-------------|
| `name` | string | No* | Mesh name |
| `instanceId` | int | No* | Instance ID |
| `path` | string | No* | Hierarchy path |
| `vertices` | string | Yes | JSON array of `{index, x, y, z}` |

### probuilder_get_vertices
Query vertex positions (all or by index). Use before vertex edits to understand mesh topology.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | Mesh name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `vertexIndexes` | string | No | all | Comma-separated indices to query |
| `verbose` | bool | No | true | Return all vertices (false = summary for large meshes) |

---

## UV Operations

### probuilder_project_uv
Box-project UVs onto faces for proper texture mapping.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | Mesh name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `faceIndexes` | string | No | all | Comma-separated face indices |
| `channel` | int | No | 0 | UV channel (0=primary, 1=lightmap, 2-3=custom) |

**Returns**: `{success, name, projectedFaceCount, channel, method}`

---

## Mesh Optimization

### probuilder_combine_meshes
Combine multiple ProBuilder meshes into one for optimization.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `names` | string | No | selected | Comma-separated mesh names, or "selected" for Selection |

### probuilder_set_material
Set material on an entire ProBuilder mesh. Supports asset path or quick RGB color for prototyping.

| Parameter | Type | Required | Default | Description |
|-----------|------|----------|---------|-------------|
| `name` | string | No* | — | Mesh name |
| `instanceId` | int | No* | 0 | Instance ID |
| `path` | string | No* | — | Hierarchy path |
| `materialPath` | string | No** | — | Material asset path |
| `r/g/b/a` | float | No** | 0.5 | Quick color (creates runtime material) |

**Provide either `materialPath` or `r/g/b` color values.

```python
# Quick color for prototyping
unity_skills.call_skill("probuilder_set_material", name="Platform_1", r=0.2, g=0.6, b=1)
unity_skills.call_skill("probuilder_set_material", name="Ramp", r=0.8, g=0.4, b=0.1)
```

---

## Scene Design Guide

> This section teaches **how to design** any 3D scene, not a fixed template. Follow these principles to generate correct, playable geometry from any user description.

### Step 1: Spatial Planning (Think Before Building)

**1 Unity unit = 1 meter.** Always plan on paper/ASCII first.

| Human Reference | Size | Use For |
|----------------|------|---------|
| Person standing | H 1.8m | Doorways ≥ 2.2m, ceilings ≥ 2.5m |
| Shoulder width | W 0.5m | Corridors ≥ 1.5m, doors ≥ 0.9m |
| Single step | H 0.18m, D 0.28m | Stairs: rise/run ratio |
| Dining table | 1.5×0.8m, H 0.75m | Furniture scale reference |
| Car | 4.5×1.8m, H 1.5m | Parking, roads ≥ 3.5m/lane |
| Single room | 4×4m | Interior spaces |
| Story height | 3m (floor-to-floor) | Multi-story buildings |

**Gameplay references:**

| Mechanic | Comfortable | Challenging | Impossible |
|----------|------------|-------------|------------|
| Jump height (vertical) | ≤ 1.0m | 1.0–1.2m | > 1.5m |
| Jump gap (horizontal) | ≤ 2.5m | 2.5–3.5m | > 4m |
| Step-up (no jump) | ≤ 0.3m | — | > 0.5m |
| Platform landing zone | ≥ 2m | 1.5m | < 1m |
| Drop without damage | ≤ 3m | 3–5m | > 8m |

### Step 2: Layout First, Details Later

**Workflow:**
1. **Root object** → `gameobject_create` as parent for the entire scene
2. **Blockout** → `probuilder_create_batch` for all major volumes (floors, walls, platforms)
3. **Connectivity check** → verify every platform is reachable from the previous one
4. **Shape refinement** → `move_vertices` for ramps/slopes, `extrude_faces` for ledges, `bevel_edges` for polish
5. **Material pass** → `set_material` with distinct colors per functional role
6. **Iteration** → user reviews, AI adjusts positions/sizes

### Step 3: Positioning Rules

**Y-axis = height (up), X/Z = horizontal ground plane.**

- `y` in parameters is the **center** of the shape, not the bottom
- A floor with `sizeY=0.3` at `y=0` has its top surface at `y=0.15`
- To place a floor with top surface at y=0: set `y = -sizeY/2`
- To stack object B on top of object A: `B.y = A.y + A.sizeY/2 + B.sizeY/2`

**Connectivity checklist for platformers:**
- Adjacent platforms: horizontal gap ≤ 2.5m AND vertical gap ≤ 1.0m
- Stairs: top surface aligns with next platform's top surface
- Ramps: low end at current ground level, high end at target platform level
- Bridges: same Y as the platforms they connect
- Pillars: `y = pillarHeight/2` (centered), `sizeY = pillarHeight`

### Step 4: Common Patterns

**Floor/Ground:** `sizeY=0.2~0.5`, generous X/Z
**Wall:** thin on one axis (`sizeZ=0.2~0.3`), tall Y, offset to edge of floor
**Pillar:** `Cylinder`, small X/Z (`0.3~0.6`), tall Y, `y = height/2`
**Ramp:** `Cube` + `move_vertices` — lower one side's top vertices
**Staircase:** `Stairs` shape — sizeY = total rise, sizeZ = total run
**Bridge:** thin Cube (`sizeY=0.15~0.2`), narrow Z, long X between supports
**Doorway:** two walls with gap, or `Arch` shape
**Room:** 4 walls + floor + ceiling, each as separate Cube

### Step 4b: Furniture & Props — Multi-Part Assembly

> **Golden Rule**: Real furniture is NEVER a single box. Decompose every object into its visible structural parts.

**Decomposition method** — ask "what would I see if I looked at this object?":
- **Desk** = 1 tabletop (thin slab) + 4 legs (tall thin cylinders/cubes)
- **Chair** = 1 seat (thin slab) + 4 legs + 1 backrest (thin tall slab)
- **Shelf** = 2 side panels + N shelves + optional back panel
- **Door** = 1 panel in the door frame
- **Window** = frame (4 thin cubes) + optional glass pane (Plane or thin Cube with transparent material)
- **Monitor** = screen slab + stand post + base plate
- **Bed** = mattress slab + headboard + 4 short legs

**Assembly pattern (parent → children):**
```python
# Create parent for the furniture piece
unity_skills.call_skill("gameobject_create", name="Desk_0")

# Build parts as children — all positions relative to parent
unity_skills.call_skill("probuilder_create_batch", defaultParent="Desk_0", items=[
    # Tabletop: 1.2m × 0.04m × 0.6m, top surface at 0.75m
    {"shape":"Cube", "name":"Desk_0_Top", "sizeX":1.2, "sizeY":0.04, "sizeZ":0.6, "y":0.73},
    # 4 Legs: 0.04m × 0.71m × 0.04m each, centered under corners
    {"shape":"Cube", "name":"Desk_0_Leg_FL", "sizeX":0.04, "sizeY":0.71, "sizeZ":0.04, "x":-0.55, "y":0.355, "z":0.25},
    {"shape":"Cube", "name":"Desk_0_Leg_FR", "sizeX":0.04, "sizeY":0.71, "sizeZ":0.04, "x":0.55,  "y":0.355, "z":0.25},
    {"shape":"Cube", "name":"Desk_0_Leg_BL", "sizeX":0.04, "sizeY":0.71, "sizeZ":0.04, "x":-0.55, "y":0.355, "z":-0.25},
    {"shape":"Cube", "name":"Desk_0_Leg_BR", "sizeX":0.04, "sizeY":0.71, "sizeZ":0.04, "x":0.55,  "y":0.355, "z":-0.25},
])
```

**Common furniture dimensions:**

| Furniture | Typical Size (W×D×H) | Key Parts |
|-----------|---------------------|-----------|
| Office desk | 1.2×0.6×0.75m | Tabletop (4cm thick) + 4 legs |
| Dining table | 1.5×0.8×0.75m | Tabletop (5cm) + 4 legs (6cm square) |
| Chair (no arms) | 0.45×0.45×0.45m seat, 0.85m total | Seat (4cm) + 4 legs + backrest (3cm×0.4m tall) |
| Armchair | 0.7×0.7×0.45m seat, 0.9m total | Seat + back + 2 armrests (5cm wide) |
| Bookshelf | 0.8×0.3×1.8m | 2 sides (3cm) + 4-5 shelves (2cm) |
| Bed (single) | 0.9×2.0×0.45m mattress | Mattress (20cm) + frame (25cm) + headboard (60cm) |
| Sofa | 2.0×0.8×0.45m seat, 0.85m back | Seat + back + 2 arms + base |
| Monitor | 0.5×0.02×0.35m screen | Screen + neck (cylinder) + base (0.25×0.2m) |
| Whiteboard | 1.5×0.03×1.0m | Board + optional frame |

**Detail enhancement techniques:**
- `bevel_edges` → **only use on chunky geometry** (sizeY ≥ 0.1m). NEVER bevel thin slabs (tabletops, shelves) — the bevel amount is relative to edge length, and on thin objects it creates ugly flared corners
- `extrude_faces` on chair seats → create slight lip/rim
- Different colors for same furniture → e.g. legs darker than top surface
- Group all parts under one parent → easy to move/duplicate the whole piece

**Mass production — create one, then duplicate:**
```python
# Build one detailed chair under "Chair_Template"
# ... (multi-part assembly)

# Duplicate to fill room
for i, (x, z) in enumerate(seat_positions):
    unity_skills.call_skill("gameobject_duplicate", name="Chair_Template",
        newName=f"Chair_{i}", x=x, z=z)
```

### Step 5: Color Coding for Prototyping

Use distinct colors by **function**, not by object:

| Role | RGB | Visual |
|------|-----|--------|
| Ground/floor | (0.4, 0.4, 0.4) | Dark gray |
| Walls | (0.6, 0.6, 0.65) | Light gray |
| Platforms | (0.2, 0.5, 0.8) | Blue |
| Hazard/challenge | (0.9, 0.3, 0.1) | Red-orange |
| Ramps/slopes | (0.8, 0.6, 0.2) | Yellow-brown |
| Goal/finish | (0.1, 0.8, 0.2) | Green |
| Decorative | (0.5, 0.3, 0.6) | Purple |

---

## Important Notes

1. **ProBuilder mesh ≠ regular mesh**: ProBuilder objects have a `ProBuilderMesh` component that maintains editable topology. Regular meshes won't work with these skills.
2. **Face indexes start at 0**: Use `probuilder_get_info` to check `faceCount` before operating.
3. **Vertex indexes**: Use `probuilder_get_vertices` to query positions before `probuilder_move_vertices` / `probuilder_set_vertices`.
4. **All modifications auto-rebuild**: Every skill calls `ToMesh()` + `Refresh()` internally — no manual rebuild needed.
5. **Undo support**: All modification skills register with Unity's Undo system and Workflow tracking.
6. **Quick color vs persistent material**: `probuilder_set_material` with `r/g/b` auto-detects render pipeline (URP/HDRP/Built-in). Use `material_create` + `materialPath` for production.
7. **Package not installed**: All skills gracefully return `{error: "ProBuilder package not installed..."}` with install instructions.
8. **Batch-first for level design**: Use `probuilder_create_batch` when creating 2+ shapes — one API call instead of many.
