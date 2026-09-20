---
document_type: mechanic
scope: modal IMGUI cursor/gameplay-input ownership
runtime: mono
evidence:
  static: SOURCE_INSPECTED_FROM_WORKING_FOA_MOD_MANAGER_PATTERN
last_verified: 2026-09-20
---

# Modal IMGUI Input Capture

Showing a cursor is not enough to make an IMGUI panel modal.

The reference FoA Mod Manager pattern captures/restores:

- previous `Cursor.lockState`;
- previous cursor visibility;
- active Unity `BaseInputModule` enabled states.

While visible it also blocks/zeros relevant game/Rewired input paths and resets axes.

## Reusable ownership pattern

```text
panel opens
→ capture previous cursor/input-module state
→ unlock/show cursor
→ suppress gameplay camera/movement input
→ process IMGUI
→ panel closes/disable/destroy
→ restore exactly the captured state
```

## Do not reuse blindly for UGUI

A Unity UI `Button` surface needs an active EventSystem/input module. The strict IMGUI suppression pattern is therefore different from the [modal Unity UI command surface](modal-command-surface.md).
