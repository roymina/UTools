---
name: unity-bookmark
description: "Scene view bookmarks. Use when users want to save, recall, or manage camera positions in the editor. Triggers: bookmark, save view, goto, camera position, viewpoint, 书签, 视角, 保存视图."
---

# Bookmark Skills

Save and recall Scene View camera positions.

## Guardrails

**Mode**: Full-Auto required

**DO NOT** (common hallucinations):
- `bookmark_save` does not exist → use `bookmark_set`
- `bookmark_load` / `bookmark_restore` do not exist → use `bookmark_goto`
- `bookmark_remove` does not exist → use `bookmark_delete`
- Bookmarks save Scene View position + current selection, not scene state

**Routing**:
- For workflow snapshots (object state undo) → use `workflow` module
- For scene save/load → use `scene` module

## Skills

### `bookmark_set`
Save current Scene View camera position as a bookmark.
**Parameters:**
- `name` (string): Bookmark name.

### `bookmark_goto`
Move Scene View camera to a saved bookmark.
**Parameters:**
- `name` (string): Bookmark name.

### `bookmark_list`
List all saved bookmarks.
**Parameters:** None.

### `bookmark_delete`
Delete a saved bookmark.
**Parameters:**
- `name` (string): Bookmark name.
