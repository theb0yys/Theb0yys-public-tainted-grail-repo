---
document_type: mechanic
scope: plugin-owned modal command/dialogue-style UI
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PARTIAL_BY_CONSUMER
last_verified: 2026-09-20
---

# Modal Command Surface

A visible panel is not a functioning UI path.

A reliable command surface needs:

```text
game/world entry
→ UI host opens
→ shared/global cursor-input scope acquired
→ correct screen-specific dispatch path remains available
→ button/selection event reaches handler
→ command executes
→ screen closes
→ screen destroys owned objects/subscriptions
→ shared scope releases/restores cursor/input/time
```

## Shared scope

Where FoA Mod Manager is an accepted dependency, [Shared Custom UI Scope](shared-custom-ui-scope.md) can own global cursor/input/controller/time-freeze state.

That does **not** replace the screen's own EventSystem/IMGUI dispatch contract.

## IMGUI vs UGUI

- IMGUI can intentionally disable Unity `BaseInputModule` paths.
- UGUI `Button` surfaces need a functioning EventSystem/input module.

The companion-dialogue correction remains the canonical example of this distinction.

## Native-dialogue boundary

A plugin-owned Unity UI dialogue-style host is not native Story Graph dialogue merely because it looks similar. Native Story Graph authoring/registration is a separate system.
