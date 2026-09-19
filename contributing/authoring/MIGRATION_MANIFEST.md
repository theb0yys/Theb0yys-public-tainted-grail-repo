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
- `docs/reference/RESEARCH_METHOD.md` → `investigate/research-method.md`
- `docs/reference/REVERSE_ENGINEERING_DISCOVERY.md` → `investigate/reverse-engineering-discovery.md`

### Diagnose
- `docs/DEBUGGING.md` → `diagnose/README.md`
- `docs/reference/DIAGNOSTICS.md` → `diagnose/diagnostics.md`
- `docs/reference/FAILURES_CONSTRAINTS.md` → `diagnose/failures-and-constraints.md`

### Reference
- `docs/EVIDENCE.md` → `reference/evidence/README.md`
- `docs/reference/VALIDATION_AND_COMPATIBILITY.md` → `reference/evidence/validation-and-compatibility.md`
- `docs/reference/MECHANICS_CATALOGUE.md` → `reference/mechanics/README.md`
- `docs/reference/IDENTITY_CATALOGUE.md` → `reference/identities/README.md`
- `docs/reference/HOOK_CATALOGUE.md` → `reference/hooks/README.md`

### Examples
- `docs/reference/WORKS_FAILS_WHY.md` → `examples/failures-and-corrections/README.md`

### Authoring standards
- `docs/reference/PROPRIETARY_SYSTEMS.md` → `contributing/authoring/PROPRIETARY_SYSTEMS.md`

Legacy paths become redirects.

## Wave 2 — learning-route normalization

After Wave 1 links are validated:
- keep `00-never-made-a-mod-start-here/` and `01-basic/` as compatibility learning paths;
- add named `learn/` journeys;
- convert `02-foundational/` and `03-advanced/` into journey indexes;
- reclassify `04-framework/` and `05-infrastructure/` under tooling/specialist navigation.

## Wave 3 — domain chapter families

Do not split domain monoliths until their private/public source mapping is explicit.

Priority domains:
1. Items
2. Weapons
3. Armour / Kandra
4. Creatures / actors
5. UI/input
6. Persistence

Each split must preserve a hub with a complete reading route.

## Wave 4 — examples and diagnosis

Create evidence-backed case studies from public-safe material:
- presentation works/doesn't work;
- UI opens but input path fails;
- wrong native owner discovery;
- runtime state works but persistence is unproven.

Do not copy private production source.

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
