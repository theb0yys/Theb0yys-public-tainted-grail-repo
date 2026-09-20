---
document_type: troubleshooting
scope: visible UI with missing click/selection/command execution
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: REPRESENTATIVE_CORRECTIONS
last_verified: 2026-09-20
---

# UI Opens but the Action Does Not Fire

If the surface is visible but the click, selection, dialogue choice or command-handler marker never fires, treat this as an **upstream input/control ownership problem** until proven otherwise.

## Compare the full path

```text
open entry
→ screen/UI owner
→ cursor/focus ownership
→ input module / Rewired path
→ event dispatch
→ button/selection handler
→ command execution
→ close
→ input/cursor restoration
```

## Companion example

A plugin-owned Unity UI dialogue host must keep Unity event modules available. Disabling `BaseInputModule` is valid for an IMGUI debug panel but can leave a Unity `Button` visible and unclickable.

The working companion route split those two surfaces:

- strict input-module suppression for IMGUI debug UI;
- EventSystem/Rewired path retained for Unity UI dialogue;
- gameplay input still frozen while modal UI is active;
- cursor/input scope released on close.

Do not patch the downstream command handler until you have proven the event reaches it.
