# Public Documentation Migration Manifest

Status: migration control for the researched information architecture.

## Wave 1 — architecture and unambiguous canonical moves

Create the mandatory top-level surfaces and move only pages whose information role is clear:

### Systems
- `docs/reference/GAME_RUNTIME_ARCHITECTURE.md` → `systems/core/game-runtime-architecture.md`
- `docs/reference/NATIVE_OBJECT_OWNERSHIP.md` → `systems/core/native-object-ownership.md`
- `docs/reference/TEMPLATES_REGISTRIES.md` → `systems/core/templates-and-registries.md`
- `docs/reference/MVC_MODELS_ELEMENTS_EVENTS.md` → `systems/core/mvc-models-elements-events.md`
- `docs/reference/SCENES_SERVICES_TEMPLATES.md` → `systems/core/scenes-services-templates.md`
- `docs/reference/UI_INPUT.md` → `systems/ui-input/README.md`
- `docs/reference/SAVING_PERSISTENCE.md` → `systems/persistence/README.md`

### Mechanics
- `docs/reference/HOOKS_AND_INTERVENTION_PROCESS.md` → `mechanics/intervention-selection.md`

### Investigate
- `investigate/research-method.md` — reconstructed from the accepted research because the referenced legacy `docs/reference/RESEARCH_METHOD.md` is absent on current `main`
- `docs/reference/REVERSE_ENGINEERING_DISCOVERY.md` → `investigate/reverse-engineering-discovery.md`

### Diagnose
- `docs/DEBUGGING.md` → `diagnose/README.md`
- `docs/reference/DIAGNOSTICS.md` → `diagnose/diagnostics.md`
- `diagnose/failures-and-constraints.md` — deferred; referenced legacy source is absent on current `main`

### Reference
- `docs/EVIDENCE.md` → `reference/evidence/README.md`
- `docs/reference/VALIDATION_AND_COMPATIBILITY.md` → `reference/evidence/validation-and-compatibility.md`
- `docs/reference/MECHANICS_CATALOGUE.md` → `reference/mechanics/README.md`
- `docs/reference/IDENTITY_CATALOGUE.md` → `reference/identities/README.md`
- `docs/reference/HOOK_CATALOGUE.md` → `reference/hooks/README.md`

### Examples
- `examples/failures-and-corrections/README.md` — deferred; referenced legacy source is absent on current `main`

### Authoring standards
- `contributing/authoring/PROPRIETARY_SYSTEMS.md` — reconstructed from the accepted research because the referenced legacy file is absent on current `main`

Legacy paths become redirects.

## Wave 2 — learning-route normalization

After Wave 1 links are validated:
- keep `00-never-made-a-mod-start-here/` and `01-basic/` as compatibility learning paths;
- add named `learn/` journeys;
- convert `02-foundational/` and `03-advanced/` into journey indexes;
- reclassify `04-framework/` and `05-infrastructure/` under tooling/specialist navigation.

## Wave 3 — domain chapter families

Do not split domain monoliths until their private/public source mapping is explicit.

Current Wave 3 status:

1. **Items — PASSED on this migration branch**
   - `docs/reference/ITEMS.md` → `mechanics/items/custom-item-integration.md`
   - `docs/reference/LOOT_REWARDS_ACQUISITION.md` → `mechanics/items/acquisition.md`
   - `docs/reference/DISTRIBUTION_MERCHANTS_LOOT.md` → `mechanics/items/distribution.md`
   - new `systems/items/README.md`, `mechanics/items/README.md`, and `learn/content-authoring/items/README.md`

2. **Weapons — PASSED on this migration branch**
   - `docs/reference/WEAPONS.md` → `mechanics/weapons/custom-weapon-integration.md`
   - legacy lifecycle shim → `systems/weapons/native-lifecycle.md`
   - new `systems/weapons/README.md`, `mechanics/weapons/README.md`, and `learn/content-authoring/weapons/README.md`

3. **Armour / Kandra — PASSED on this migration branch**
   - `docs/reference/ARMOUR.md` → `mechanics/armour/custom-armour-integration.md`
   - legacy lifecycle shim → `systems/armour/kandra-clothes-lifecycle.md`
   - new `systems/armour/README.md`, `mechanics/armour/README.md`, and `learn/content-authoring/armour/README.md`

4. **Creatures / actors — PASSED on this migration branch**
   - `docs/reference/CREATURES_NPCS.md` → `mechanics/creatures/custom-creature-injection.md`
   - `docs/reference/ACTORS_LOCATIONS_SPAWNING.md` → `systems/actors/location-and-session-lifecycle.md`
   - `docs/reference/SPAWNING_ENCOUNTERS.md` → `mechanics/actors/spawning-and-population.md`
   - legacy creature shim → `systems/creatures/README.md`
   - new creature/actor mechanics indexes and `learn/content-authoring/creatures/README.md`

5. **UI/input — canonical system move completed in Wave 1; case/diagnosis enrichment remains later work.**

6. **Persistence — canonical system move completed in Wave 1; no generic persistence mechanic is promoted.**

Every domain conversion above has a mandatory packet under `contributing/authoring/domain-packets/`.

Each split preserves a hub with a complete reading route and leaves redirects at legacy paths.

## Wave 4 — examples and diagnosis

Status: **PASSED on this migration branch** for the four researched cases.

Mandatory packets:
- `contributing/authoring/case-packets/weapon-presentation-failure.md`
- `contributing/authoring/case-packets/ui-dispatch-failure.md`
- `contributing/authoring/case-packets/mount-wrong-owner-discovery.md`
- `contributing/authoring/case-packets/runtime-vs-persistence.md`

Public case studies:
- `examples/failures-and-corrections/weapon-combat-works-render-invisible.md`
- `examples/failures-and-corrections/ui-opens-but-choice-does-not-dispatch.md`
- `examples/investigations/mount-wrong-owner-discovery.md`
- `examples/failures-and-corrections/runtime-success-persistence-unproven.md`

Symptom-first diagnosis:
- `diagnose/presentation/weapon-works-but-is-invisible.md`
- `diagnose/ui/opens-but-action-does-not-fire.md`
- `diagnose/ownership/expected-behaviour-missing.md`
- `diagnose/persistence/works-now-not-after-load.md`

Evidence boundaries:
- case pages preserve the underlying reviewed evidence strength separately from the new public rewrite;
- no public case inherits persistence/compatibility/release status;
- no private production source or proprietary asset is copied;
- no new runtime, save, decompilation, compatibility or release validation was performed by this migration.

## Wave 5 — legacy reference archetype audit

Status: **PASSED on this migration branch**, subject to final branch validation recorded in PR review.

Control artifacts:
- `contributing/authoring/reference-audit/WAVE5_REFERENCE_AUDIT.md`
- `contributing/authoring/reference-audit/packets/README.md`

Audit result:
- 18 mixed-role substantive legacy pages received packets and were split into canonical system + mechanic pages.
- 14 single-role substantive legacy pages migrated directly without artificial packets.
- 4 small aliases now redirect directly to canonical owners.
- previously migrated Waves 1–4 redirects were preserved.

Mixed-role families completed:
- AI;
- Assets;
- Audio;
- Combat;
- Crime;
- Interactions;
- Localisation;
- Map/travel/scenes;
- Progression;
- Magic/status effects;
- Story/quest/dialogue;
- Environment/world placement.

Single-role direct migrations completed:
- assemblies/system-owner lookup;
- content-domain map;
- game-knowledge index;
- game/system routing map;
- Golden Rules;
- identity kinds;
- lifecycle hooks;
- persistence architecture;
- private API compatibility;
- rendering ownership;
- crafting/economy boundaries;
- resource lifetime;
- serialization/archive boundary.

Legacy `docs/reference/` is retained only for compatibility redirects/aliases. New substantive documentation must use the canonical information surfaces.

Evidence boundary:
- Wave 5 reorganizes existing public knowledge by responsibility.
- It does not newly validate runtime, save, compatibility, performance, or release claims.
- No private production source, bulk decompilation, or proprietary assets were copied.

## Completion criteria for each migration

A migrated page must:
- have exactly one canonical destination;
- preserve material technical meaning;
- preserve evidence limitations;
- have working internal links;
- leave a compatibility redirect at the old path;
- be indexed from the correct new surface;
- not broaden version/runtime support;
- not inherit runtime status from a different artifact.
