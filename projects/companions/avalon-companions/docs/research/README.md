# Avalon Companions Research Index

## Document control

- Status: current mod-local research index
- Owner: `mods/avalon-companions/`
- Canonical branch: `companions`
- Scope: creature and animal companions only
- Research authority: claim-level only; file placement does not promote claims

## Ownership boundary

```text
Avalon AI owns AI.
The Dialogue Framework owns dialogue.
Avalon Human Companions owns human companions.
Avalon Companions owns creature and animal companions.
Tainted Weapons owns item and weapon truth for the Taming Bow and Taming Arrow.
```

Ownership attaches to domain truth, not to the caller of a service.

- Avalon Cheat Panel borrows FoA item and inventory services; it does not own the item catalogue.
- Tainted Weapons selects, clones, names and registers the Taming Bow and Taming Arrow.
- Avalon Companions requests the immutable Taming Tool receipt and owns only the creature/animal companion meaning of an exact-pair event.
- The shared `companions` branch coordinates companion-owned work; it does not absorb item, weapon, quest, UI, AI, dialogue or human-companion truth.

Research under this folder must not define human companion truth, AI framework internals, dialogue framework internals, item or weapon truth, quest truth, UI-framework truth, or a universal cross-owner companion platform.

## Current planning baseline

- [`../companion-1-0-design-baseline-2026-08-19.md`](../companion-1-0-design-baseline-2026-08-19.md) — owner-corrected Avalon Companions 1.0 baseline for creature and animal companions.
- [`../decisions/0008-companion-1-0-acquisition-dialogue-ai-ownership.md`](../decisions/0008-companion-1-0-acquisition-dialogue-ai-ownership.md) — corrected acquisition and framework-service boundaries.
- [`../decisions/0009-companion-competence-progression-animation-milestones.md`](../decisions/0009-companion-competence-progression-animation-milestones.md) — corrected creature/animal competence and animation milestone direction.

## Native and acquisition research

- [`native-unconscious-ai-ownership-2026-08-19.md`](native-unconscious-ai-ownership-2026-08-19.md) — static evidence for the native unconscious physical backbone and possible scoped ownership seams.
- [`native-bow-arrow-projectile-and-wolf-target-identity-2026-08-19.md`](native-bow-arrow-projectile-and-wolf-target-identity-2026-08-19.md) — fingerprinted `BowFSM -> ItemProjectile -> Arrow -> ProjectileContactParams` trace, shooter/weapon/ammunition/target attribution, reviewed wolf template, exact `Location.ID` binding and negative controls.
- [`projectile-acquisition-probe-implementation-2026-08-19.md`](projectile-acquisition-probe-implementation-2026-08-19.md) — strict exact-pair consumer implementation: request Tainted Weapons-owned Taming Tool identities, ignore every other projectile, record exact native contact and bind one reviewed wolf for the current scene.
- [`taming-eligibility-gate-2026-07-02.md`](taming-eligibility-gate-2026-07-02.md) — historical read-only creature taming eligibility gate.
- [`taming-encounter-evidence-probe-2026-07-09.md`](taming-encounter-evidence-probe-2026-07-09.md) — historical read-only wolf/bear encounter evidence design.
- [`wolf-acquisition-ai-dialogue-consumer-map-2026-08-19.md`](wolf-acquisition-ai-dialogue-consumer-map-2026-08-19.md) — current Avalon AI package/owner path, first-wolf package requirements, Dialogue Framework consumer requirements and inspected service boundaries.

## Exact Taming Tool dependency

The item-owner work is on the canonical `tainted-weapons` branch and draft PR #345.

Reviewed source pair:

```text
Shortbow:     Weapon_Bow_Tier1_Light_Shortbow
Wooden Arrow: Weapon_Ammo_Tier0_WoodenArrow
```

Registered owner identities are returned through:

```text
TaintedWeapons.TaintedWeaponsTamingToolsApi.EnsureRegistered(out receipt)
```

Avalon Companions does not duplicate those GUIDs as authoritative companion configuration. The receipt is the item-owner truth consumed at runtime.

## Current acquisition summary

- The native bow path is statically traced from equipped bow/quiver through `BowFSM`, `ItemProjectile`, combined projectile creation, launch attribution, contact, `IAlive` resolution and cleanup.
- The reviewed tutorial wolf family is `Spec_AnimalWolf` / `9086dee514edc644b9b55890d885db3f`.
- One tutorial target must be bound by exact `Location.ID` and exact `NpcElement` reference; template identity alone is insufficient.
- The current probe requests Tainted Weapons registration only while the default-off diagnostic is enabled.
- The probe arms only after the owner receipt reports both items registered with complete identities.
- A contact is recorded only when source weapon GUID equals the owner-supplied Taming Bow GUID and ammunition GUID equals the owner-supplied Taming Arrow GUID.
- Vanilla Shortbow/Wooden Arrow, mixed pairs and every unrelated projectile are ignored.
- The probe performs no damage, subdual, unconscious, faction, AI, dialogue, quest, save or companion mutation.

## Deep Research intake

- [`chatgpt-deep-research-brief-companion-1-0-implementation-architecture-2026-08-19.md`](chatgpt-deep-research-brief-companion-1-0-implementation-architecture-2026-08-19.md) — historical brief; its universal-platform and framework-selection premise was corrected by the owner.
- [`companion-1-0-implementation-architecture-deep-research-source-2026-08-19.md`](companion-1-0-implementation-architecture-deep-research-source-2026-08-19.md) — preserved returned-report source identity.
- [`companion-1-0-implementation-architecture-deep-research-cleaned-2026-08-19.md`](companion-1-0-implementation-architecture-deep-research-cleaned-2026-08-19.md) — cleaned E1 research derivative; not implementation authority.
- [`companion-1-0-implementation-architecture-deep-research-review-2026-08-19.md`](companion-1-0-implementation-architecture-deep-research-review-2026-08-19.md) — controlling owner-scope correction and planning-only disposition.

## Correct use of the Deep Research report

Retained for Avalon Companions planning:

- native unconscious as the first physical acquisition candidate;
- consumption of reviewed weapon/ammunition events through the owning service;
- hunter/notice-board wolf onboarding through the owning quest service;
- separate physical subdual and companion-domain acquisition state;
- exact captured-creature continuity;
- creature/animal persistence and reconciliation;
- creature competence and vanilla-animation action milestones;
- bonfire creature/animal progression through the owning UI service.

Rerouted or rejected for this owner:

- human companion architecture — Avalon Human Companions;
- AI technology selection or AI implementation — Avalon AI;
- dialogue technology selection or dialogue implementation — Dialogue Framework;
- Taming Bow/Arrow identity, cloning and registration — Tainted Weapons;
- hunter quest, notice-board and unlock truth — owning quest service;
- universal cross-owner companion profile/platform — rejected.

## Current evidence state

```text
owner scope correction = PASSED
borrowed-service ownership correction = PASSED
creature/animal design baseline = CURRENT_FOR_PLANNING
Avalon AI package/owner boundary map = PASSED
Dialogue Framework inspected service state = NOT_PRESENT_IN_INSPECTED_REF
native bow/projectile control path = PASSED_DECOMPILATION_STATIC
projectile owner/weapon/ammunition/target attribution = PASSED_DECOMPILATION_STATIC
reviewed source pair = PASSED_DESIGN
Tainted Weapons registration service = IMPLEMENTED_SOURCE_ON_SEPARATE_BRANCH
Tainted Weapons runtime registration = NOT_RUN
exact-pair owner-service consumer = IMPLEMENTED_SOURCE
ordinary-projectile exclusion = IMPLEMENTED_SOURCE
wolf template identity = PASSED
wolf same-session identity seam = PASSED
project build = NOT_RUN
plugin load = NOT_RUN
owner-service reflection = NOT_RUN
probe CSV output = NOT_RUN
exact-pair native attribution = NOT_RUN
negative controls = NOT_RUN
wolf target runtime binding = NOT_RUN
scene invalidation = NOT_RUN
runtime mutation absence = NOT_RUN
Deep Research RH1 = PASSED
Deep Research RH2 = PASSED_WITH_OWNER_CORRECTION
RH3 = NOT_RUN
RH4 = NOT_RUN
RH5 = NOT_PROVIDED
subdual implementation = NOT_RUN
unconscious implementation = NOT_RUN
companion acquisition implementation = NOT_RUN
```

## Next researched task

Build and deploy both canonical branches on the matching FoA Mono installation, enable `AcquisitionProbe.Enabled`, let Tainted Weapons register the exact Taming Bow and Taming Arrow, grant those named items through an existing item-service consumer, and execute the strict exact-pair plus ordinary/mixed-pair negative-control matrix before any subdual or unconscious implementation is authorised.
