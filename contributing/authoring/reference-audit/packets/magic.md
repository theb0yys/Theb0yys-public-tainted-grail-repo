# Wave 5 Packet — Spells and Status Effects

## Reader job

Separate spell/status native ownership and lifecycle from VFX, cast, buildup, and status intervention mechanics.

## Legacy source pages

- `docs/reference/SPELLS_EFFECTS.md`
- `docs/reference/STATUS_EFFECTS.md`

## Why a packet is required

These pages use the old universal handbook body and combine at least two current archetypes:

1. native-system explanation/ownership/lifecycle;
2. actionable modding intervention;
3. failure/correction reasoning;
4. verification/proof-boundary guidance.

The split is by **information responsibility**, not page size.

## Canonical mapping

| Legacy source | Native-system destination | Mechanic destination |
|---|---|---|
| `SPELLS_EFFECTS.md` | `systems/magic/spells-and-effects.md` | `mechanics/magic/spell-and-vfx-intervention.md` |
| `STATUS_EFFECTS.md` | `systems/magic/status-effects.md` | `mechanics/magic/status-effect-intervention.md` |

## System-page responsibility

The system destination owns:

- what the subsystem is;
- native/game owners;
- identities/types/methods required to understand ownership;
- lifecycle/data flow;
- current technical proof boundary.

## Mechanic-page responsibility

The mechanic destination owns:

- how a mod may interact;
- why the intervention seam is chosen;
- failed/rejected approaches;
- verification of the intervention;
- a link back to the canonical system owner.

## Material that must survive the split

- spell item/skill/effect chain;
- status identity/stacking/buildup;
- VFX ≠ gameplay effect rule;
- source/target scoping;
- observation vs mutation;
- persistence boundary;

## Evidence / claim boundary

This packet authorizes **documentation-role separation only**.

- Existing public technical claims are not silently strengthened.
- The migration does not newly prove runtime, save, compatibility, performance, or release behaviour.
- Any claim that cannot be cleanly preserved from the current public body remains bounded/unknown rather than being invented.
- Private production source and proprietary assets remain out of scope.

## Legacy path

After both canonical pages exist, the legacy page becomes a compatibility redirect to the **system page**, which links the mechanic.

## Completion checks

- no duplicate canonical explanation;
- system ownership/lifecycle is readable without the procedure;
- mechanic is actionable without restating the complete system model;
- failures remain attached to the intervention they constrain;
- evidence limits remain explicit;
- internal links resolve after relocation.
