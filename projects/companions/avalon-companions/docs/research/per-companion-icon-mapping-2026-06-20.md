# Per-Companion Icon Mapping - 2026-06-20

## Scope

User screenshot validation showed `Avalon Companions Debug` rendering a shared icon, but `Skeleton 1H Candidate` and other non-wolf/non-bear entries used the same generic `companion.creature` fallback. The approved correction is a UI-only mapping update from existing roster display/template names to Tainted Interface companion-family icon IDs.

## Research read

- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/research/shared-icon-consumer-proof-2026-06-19.md`
- `mods/Tainted Interface/docs/research/icon-catalog-layer-2026-06-19.md`
- `mods/Tainted Interface/docs/research/companion-icon-catalog-expansion-2026-06-20.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Approved route for 0.1.28

Avalon Companions may choose a shared icon ID using the existing `PetRosterEntry.TemplateName` and `PetRosterEntry.DisplayName` strings. This remains debug-panel presentation only. It must not alter roster approval, spawn behavior, lifecycle behavior, command routing, native prompt behavior, combat, targeting, persistence, or Core integration.

| Roster family | Shared icon ID |
| --- | --- |
| Qrko pet entries | `companion.pet` |
| Wolf | `companion.wolf` |
| Bear and Bear Interactions | `companion.bear` |
| Deer | `companion.deer` |
| Pig | `companion.pig` |
| Cow | `companion.cow` |
| Bullrat | `companion.bullrat` |
| Corpse Eater | `companion.corpse-eater` |
| Redcap | `companion.redcap` |
| Flamegobbler | `companion.flamegobbler` |
| Grindylow | `companion.grindylow` |
| Wyrdspirit | `companion.wyrdspirit` |
| Sharg | `companion.sharg` |
| Ogre | `companion.ogre` |
| Zombie | `companion.zombie` |
| Drowner variants | `companion.drowner` |
| Skeleton variants | `companion.skeleton` |
| Floatling | `companion.floatling` |
| Tadpole | `companion.tadpole` |
| Any future unmapped creature | `companion.creature` |

`Drowner` must be checked before `Zombie` because the drowner templates include `Spec_EnemyZombie` in the template name.

## Validation

- Release build passed with 0 warnings and 0 errors.
- Built DLL file version: `0.1.28.0`.
- Built DLL SHA-256: `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.
- Live deploy passed. Live DLL SHA-256 matched Release output: `36FF6A4F9BF1D9406A87A48ED7D3FE9B988F88B7D27283CD83E0B54B5A267577`.

## Pending

- BepInEx load validation for Avalon Companions 0.1.28.
- In-game smoke: open `Avalon Companions Debug`, cycle through Qrko, wolf, bear, skeleton, drowner, and at least one offensive creature row, and confirm distinct thematic icons render while all command buttons keep existing behavior.
