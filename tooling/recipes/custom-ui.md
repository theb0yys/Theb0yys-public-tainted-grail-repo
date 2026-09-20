# Recipe: Custom Modal UI

Use:

- **FoA Mod Manager** for global UI scope/input/cursor/freeze.
- **Tainted Interface** for shared styles/icons/textures.
- **your mod** for the actual view and commands.

```text
SetCustomUiScope(owner, true)
→ create feature view
→ resolve semantic styles/assets from Tainted Interface
→ process feature commands
→ destroy view/subscriptions
→ SetCustomUiScope(owner, false)
```

For UGUI, keep the required EventSystem/input-module path alive.

For IMGUI, follow the IMGUI-specific input-capture rules.
