# Recipe: Custom Modal UI

Use:

- **FoA Mod Manager** for global cursor/input/controller/world-freeze scope.
- **Tainted Interface** for shared styles/icons/textures.
- **your mod** for the actual view, commands and gameplay state.

A complete source project is available at [examples/mono/infrastructure/shared-ui](../../examples/mono/infrastructure/shared-ui/README.md).

## Ownership flow

```text
SetCustomUiScope(owner, true)
→ create feature view
→ resolve semantic Tainted Interface styles/assets
→ process feature commands
→ destroy view/subscriptions
→ SetCustomUiScope(owner, false)
```

For a passive HUD use render-only resources/styles and **do not acquire modal scope**.

UGUI needs its EventSystem/input-module path alive.

IMGUI can use a stricter input-capture model.

If your mod locally patches Rewired/input, allow the Mod Manager controller-cursor read window when `FoAModManagerApi.IsControllerCursorInputReadActive` is true.

Do not let every mod invent its own global cursor/timeScale restoration system.
