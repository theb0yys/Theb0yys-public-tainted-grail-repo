---
document_type: mechanic
scope: shared custom UI input/cursor/world-freeze ownership
runtime: mono
evidence:
  source: FOA_MOD_MANAGER_PROJECT_API
  runtime: MULTI_CONSUMER_LINEAGE
last_verified: 2026-09-20
---

# Shared Custom UI Scope

A modal mod screen needs an owner for cursor/input/freeze state.

The shared FoA Mod Manager contract uses an **owner ID** and reference-counted/owner-scoped state rather than each mod independently guessing the previous cursor/time/input state.

## Modal screen

```text
screen opens
→ SetCustomUiScope(ownerId, true)
→ gameplay input frozen
→ real cursor unlocked/shown
→ controller cursor enabled
→ world time frozen by default
→ screen closes
→ SetCustomUiScope(ownerId, false)
→ prior state restored when final freeze owner releases
```

For an interactive overlay that should keep the world running, request `freezeWorld:false`.

## Controller cursor

Internal controller-cursor sampling may need a narrow bypass through a mod's own Rewired suppression. Consumers can detect that internal read and let only that read through.

## Distinguish IMGUI and UGUI

Shared scope owns global input/cursor/freeze state. A UGUI screen still needs the real EventSystem path; an IMGUI screen may intentionally suppress Unity input modules.

Do not confuse shared scope with the screen's own dispatch/rendering ownership.
