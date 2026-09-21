# Companion HUD Icon Overlay - 2026-06-20

## Scope

User validation accepted the per-companion panel icons and requested the active companion icon on the gameplay screen, either top-left or bottom-right, using a polished icon background from `Assets/User Interface/icon and dialouge backgrounds/Dragon`.

## Research read

- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research/per-companion-icon-mapping-2026-06-20.md`
- `mods/Tainted Interface/docs/research/companion-icon-catalog-expansion-2026-06-20.md`
- `mods/Tainted Interface/docs/research/hud-icon-background-dragon-2026-06-20.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Approved route for 0.1.29

Avalon Companions may draw a passive IMGUI HUD badge for the active managed roster companion while normal gameplay HUD is visible. The badge uses the existing `GetRosterIconId` mapping for the companion icon and requests `hud.companion-icon-background` from Tainted Interface for the Dragon frame/background.

The overlay is configurable:

- `Companions.ShowCompanionHudIcon`, default `true`.
- `Companions.CompanionHudIconCorner`, default `TopLeft`; supports `TopLeft` and `BottomRight`.
- `Companions.CompanionHudIconSize`, default `88`, clamped from `56` to `128`.

The active companion lookup is throttled to twice per second and texture lookups are cached. The overlay does not acquire input scope, does not capture cursor state, and does not run command logic.

## Boundary

- No spawn, summon, recall, dismiss, lifecycle, dialogue, native prompt, targeting, combat, persistence, or Avalon Core behavior changes.
- No per-frame file checks.
- No full asset-library scan.
- No debug-panel-only selected roster badge behavior changes.
- No overlay while the companion debug panel or custom dialogue is visible.

## Validation

- 0.1.31 placement hotfix: after user screenshot validation showed the top-left badge overlapping the Wyrd Scent/status overlay lane, the top-left anchor now places the badge just above that lane. Bottom-right placement is unchanged.
- 0.1.31 Release build passed with 0 warnings and 0 errors.
- Built and live 0.1.31 DLL file version: `0.1.31.0`.
- Built and live 0.1.31 DLL SHA-256: `E47203C8C83A862F0FD01A2DFEB602E34FC4801E4911D71CD39EA03E3298DDB2`.
- Avalon Companions Release build passed with 0 warnings and 0 errors.
- Built current Avalon Companions DLL file version: `0.1.30.0`.
- Built current Avalon Companions DLL SHA-256: `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`.
- Tainted Interface embedded Release build passed with 0 warnings and 0 errors.
- Built Tainted Interface DLL file version: `0.1.4.0`.
- Built Tainted Interface DLL SHA-256: `BDCC7657D563FA43EE66E04456FFEF872C590D476E287A3C586202D1D0591525`.
- Tainted Interface embedded resource check: `DarkFantasy=12`, `Icons=50`, including `TaintedInterface.Assets.Icons.HudCompanionIconBackground.png`.
- Live deploy passed. Live Avalon Companions DLL SHA-256 matched `6FB8BD9A027FC450A042E401C53B85C7FBC00CFC415A9DBA0BD8601700F3007A`; live Tainted Interface DLL SHA-256 matched `BDCC7657D563FA43EE66E04456FFEF872C590D476E287A3C586202D1D0591525`.
- BepInEx load validation passed. `LogOutput.log` showed Avalon Companions 0.1.30 loaded with `ShowCompanionHudIcon=True`, `CompanionHudIconCorner=TopLeft`, and `CompanionHudIconSize=88`.
- Required in-game smoke: summon or swap to at least one managed companion, close panels/dialogue, confirm the HUD badge appears in the configured corner with the Dragon background, dismiss the companion, and confirm the badge disappears without command regressions.
