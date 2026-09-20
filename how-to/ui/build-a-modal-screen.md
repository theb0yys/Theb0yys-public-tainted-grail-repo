# Build a Modal Custom Screen

Use this when a mod needs an interactive custom screen rather than a passive overlay.

Canonical ownership: [UI, Cursor, Focus, and Input Ownership](../../systems/presentation/ui-input.md).

Shared infrastructure:

- [FoA Mod Manager](../../tooling/foa-mod-manager/README.md)
- [Tainted Interface](../../tooling/tainted-interface/README.md)

## Complete ownership path

A modal screen needs more than rendering:

```text
open
→ acquire shared UI/input/cursor scope
→ capture previous state
→ establish cursor/focus
→ suppress gameplay input as required
→ keep UI input alive
→ dispatch one command path
→ close/forced close
→ release scope
→ restore previous state
```

## Procedure

1. Give the screen a stable owner ID.
2. Acquire the shared custom-UI scope once when opening.
3. Capture any local state you personally own before changing it.
4. Render through your own screen code; use Tainted Interface semantic resources if desired.
5. Route mouse/keyboard/controller activation to one handler.
6. Keep gameplay truth outside the UI layer.
7. Handle normal close, Back/Escape, scene transition, dependency loss and plugin teardown.
8. Release the shared scope exactly once.
9. Restore only state your screen actually owned/changed.

## Avoid competing cursor owners

Do not independently force `Cursor.visible`, lock state, input suppression and time scale every frame while a shared UI owner is already managing that scope.

Competing ownership commonly produces recentering, flashing cursors, lost activation or broken restoration.

## Validate actions, not just rendering

A complete proof should show:

- screen opens;
- cursor is stable;
- mouse click reaches handler;
- keyboard navigation/activation works if claimed;
- controller navigation/activation works if claimed;
- gameplay camera/movement is blocked when required;
- handler executes exactly once;
- close restores state;
- forced close/scene change cleans up.

“Panel visible” is not a modal-UI proof.

For copyable infrastructure use, see [Custom modal UI recipe](../../tooling/recipes/custom-ui.md).
