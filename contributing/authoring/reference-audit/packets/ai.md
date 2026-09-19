# Wave 5 Packet — AI and Perception

## Reader job

Understand native AI/perception/combat-pressure ownership separately from mod intervention patterns.

## Legacy source pages

- `docs/reference/AI_PERCEPTION_BEHAVIOR.md`

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
| `AI_PERCEPTION_BEHAVIOR.md` | `systems/ai/README.md` | `mechanics/ai/intervention-patterns.md` |

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

- perception/alert ownership;
- target/combat-state ownership;
- native search/pursuit behaviour;
- narrow-intervention guidance;
- failure cases involving duplicated AI or over-broad patches;

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
