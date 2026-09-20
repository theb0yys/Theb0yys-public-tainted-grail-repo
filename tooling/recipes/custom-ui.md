# Recipe: Custom Modal UI

Use:

- **FoA Mod Manager** for global cursor/input/controller/world-freeze scope.
- **Tainted Interface** for shared styles/icons/textures.
- **your mod** for the actual view, commands and gameplay state.

## Direct-reference shape

```csharp
private const string UiOwnerId = "author.foa.example.panel";

private void OpenPanel()
{
    FoAModManagerApi.SetCustomUiScope(UiOwnerId, true, freezeWorld: true);
}

private void ClosePanel()
{
    FoAModManagerApi.SetCustomUiScope(UiOwnerId, false, freezeWorld: true);
}

private void OnDestroy()
{
    FoAModManagerApi.SetCustomUiScope(UiOwnerId, false, freezeWorld: true);
}
```

Resolve visuals by semantic API rather than raw asset paths:

```csharp
Texture2D? icon = TaintedInterfaceApi.GetIcon("...");
Texture2D? frame = TaintedInterfaceApi.GetUiTexture("...");
```

For a passive HUD use render-only resources/styles and **do not acquire modal scope**.

## Input boundary

UGUI needs its EventSystem/input-module path alive.

IMGUI can use a stricter input-capture model.

If your mod locally patches Rewired/input, allow the Mod Manager controller-cursor read window when `FoAModManagerApi.IsControllerCursorInputReadActive` is true.

## Cleanup rule

```text
acquire shared scope
→ create owned view/subscriptions
→ process feature commands
→ destroy owned view/subscriptions
→ release shared scope
```

Do not let every mod invent its own global cursor/timeScale restoration system.
