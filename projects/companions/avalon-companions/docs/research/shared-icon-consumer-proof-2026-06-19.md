# Research: Shared Icon Consumer Proof

Date: 2026-06-19
Scope: Display shared Tainted Interface companion icons in Avalon Companions without changing companion behavior.
Question: Can Avalon Companions consume a shared `companion.wolf` icon through Tainted Interface instead of owning raw icon assets?
Game version and branch: FoA Mono branch; no new game target inspection needed because this is UI-only and uses existing companion roster state.
Tools used: repo docs, Tainted Interface API/source review, companion UI source review, read-only icon asset inspection.

## Evidence read

- `docs/engineering-process.md`
- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research/vanilla-style-dialogue-layout-2026-06-19.md`
- `mods/Tainted Interface/docs/research/icon-catalog-layer-2026-06-19.md`
- `mods/Tainted Interface/src/TaintedInterfaceApi.cs`
- `mods/avalon-companions/src/Patches/TaintedInterfaceBridge.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Findings

- Avalon Companions already has an optional reflection bridge to Tainted Interface for shared styles, cursor helper, and input scope.
- The debug panel already reports selected companion metadata and is the lowest-risk first visual consumer.
- The selected companion icon can be derived from the existing `PetRosterEntry.TemplateName` without changing roster data or actor behavior.
- A direct project reference to Tainted Interface is unnecessary; reflection keeps Tainted Interface optional.

## Approved route

- Add optional reflection access to `TaintedInterfaceApi.GetIcon(string iconId)`.
- Render the selected roster icon in the existing debug panel when Tainted Interface is available.
- Map Qrko entries to `companion.pet`, wolf templates to `companion.wolf`, bear templates to `companion.bear`, and all other one-session candidates to `companion.creature`.
- Keep the panel fully functional when Tainted Interface is absent or an icon is missing.

## Not approved

- New companion commands.
- Roster expansion.
- Actor spawning changes.
- Targeting, custom AI, persistence, restoration, healing, resurrection, taming, training, loyalty, or active squad behavior.
- Copying icon assets into Avalon Companions.
- Reading icon files directly from Avalon Companions.

## Validation needed

- Build Tainted Interface.
- Build Avalon Companions.
- In game, open the debug panel with Tainted Interface installed and confirm the selected companion icon renders.
- Select `Wolf Candidate` and confirm the wolf icon appears.
- Confirm the panel still opens without Tainted Interface installed or when icons are missing.
