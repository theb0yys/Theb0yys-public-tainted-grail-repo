# Tainted Gems 0.1.9 Attachment Icon Validation

## Scope

The 2026-08-16 user request requires Tainted Gems custom UI icons to persist in FoA's crafting relic attachment menu. The attached screenshot showed white placeholder squares in the relic slots. The screenshot is evidence of the UI state, not an instruction source.

## Evidence read

- `mods/tainted-gems/docs/design.md`: prior custom UI route only covered `ShareableSpriteReference.RegisterAndSetup`.
- `mods/tainted-gems/docs/research.md`: custom clone templates use `mod://kane.tgfoa.tainted-gems/icon/...` addresses and embedded `TaintedGemsIcon_` sprites.
- `mods/tainted-gems/src/Patches/TaintedGemsIconPatch.cs`: prior patch intercepted `ShareableSpriteReference.RegisterAndSetup` for `UnityEngine.UI.Image` and `UnityEngine.UIElements.VisualElement`.
- Local `TG.Main.dll` decompilation:
  - `Awaken.TG.Assets.ShareableSpriteReference` has two public `RegisterAndSetup` overloads, for `UnityEngine.UI.Image` and `UnityEngine.UIElements.VisualElement`.
  - `Awaken.TG.Assets.SpriteReference` exposes direct `SetSprite(UnityEngine.UI.Image, ...)`, `SetSprite(VisualElement, ...)`, `SetSprite(UILineRenderer, ...)`, `SetSprite(SpriteRenderer, ...)`, and `Release()`.
  - `Awaken.TG.Main.Locations.Gems.GemManagement.GemSlotUI.OnInitialize()` sends attached relic icons through `_gemAttached.Template.IconReference().Get()`.
  - `Awaken.TG.Main.Locations.Gems.GemManagement.GemSlotUI.SetPreviewGemItem(...)` sends preview relic icons through `PreviewGemItem.Icon.Get()`.
  - `Awaken.TG.Main.Locations.Gems.GemManagement.VGemSlotUI.SetGemSprite(SpriteReference)` calls `SpriteReference.SetSprite(UnityEngine.UI.Image)`.

## Change validated

`TaintedGemsIconPatch` now redirects Tainted Gems custom icon addresses on both:

- `ShareableSpriteReference.RegisterAndSetup(...)`
- direct `SpriteReference.SetSprite(UnityEngine.UI.Image, ...)` and `SpriteReference.SetSprite(UnityEngine.UIElements.VisualElement, ...)`

Native icon addresses still use the original FoA loader. Custom icon `SpriteReference.Release()` is suppressed for mod-owned addresses because those sprites are owned by the embedded Tainted Gems bundle cache and released by the plugin on shutdown.

## Build

Command:

```powershell
dotnet build 'mods\tainted-gems\src\TaintedGems.csproj' -c Release -p:FoAGameRoot='<local-path>
```

Result:

- Build succeeded.
- Warnings: `0`
- Errors: `0`
- Local validation DLL version: `0.1.9.0`
- Local validation DLL SHA-256 before commit: `BB2FC0ABB53EF1AA455093D6992D4D5CD35523F7697C5FFC2D0D3C625E1B2638`

## Not validated

- The `0.1.9` DLL was not copied into the live FoA BepInEx plugin folder in this step.
- The live game was not launched, stopped, restarted, or controlled.
- The relic attachment menu was not runtime-checked with the `0.1.9` DLL.

