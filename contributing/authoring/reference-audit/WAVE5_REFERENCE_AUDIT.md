# Wave 5 — Legacy `docs/reference/` Archetype Audit

Status: **PASSED on `docs/researched-information-architecture` — audit and migration complete**  
Scope: remaining legacy `docs/reference/` files on `docs/researched-information-architecture` after Waves 1–4.

## Audit rule

A page receives an authoring packet **only** when it still combines multiple documentation responsibilities, for example:

- native-system architecture;
- actionable modding procedure;
- failure/correction analysis;
- verification/evidence guidance.

A page that already has one clear responsibility migrates directly to its canonical surface without an unnecessary packet.

Previously migrated compatibility redirects are excluded from reprocessing.

## Result

- **18 mixed-role substantive pages** require packets before migration.
- **14 substantive single-role pages** may migrate directly.
- **4 small alias pages** should become direct compatibility redirects to the new canonical owner.
- Existing Waves 1–4 redirects remain untouched.

---

## A. Mixed-role pages — packet required

| Legacy page | Mixed responsibilities | Packet family | Canonical system owner | Canonical mechanic owner |
|---|---|---|---|---|
| `AI_PERCEPTION_BEHAVIOR.md` | AI architecture + intervention guidance + failure/verification | AI | `systems/ai/README.md` | `mechanics/ai/intervention-patterns.md` |
| `ASSETS.md` | asset ownership/lifecycle + loading/integration procedure + failures | Assets | `systems/assets/README.md` | `mechanics/assets/loading-and-presentation.md` |
| `AUDIO_FMOD_INTEGRATION.md` | FMOD ownership/identity + replacement procedure + failures | Audio | `systems/audio/fmod-event-identity.md` | `mechanics/audio/fmod-replacement.md` |
| `AUDIO_MUSIC.md` | audio/music ownership + playback procedure + failures | Audio | `systems/audio/README.md` | `mechanics/audio/music-and-playback.md` |
| `COMBAT_DAMAGE.md` | damage/death architecture + hook guidance + failures | Combat | `systems/combat/damage-death-attribution.md` | `mechanics/combat/damage-and-death-hooks.md` |
| `COMBAT_STATS_EFFECTS.md` | stat/poise architecture + tuning procedure + failures | Combat | `systems/combat/README.md` | `mechanics/combat/stat-and-poise-tuning.md` |
| `CRIME_STEALTH_BOUNTY.md` | crime authority + intervention patterns + failures | Crime | `systems/crime/README.md` | `mechanics/crime/intervention-patterns.md` |
| `INTERACTIONS_USABLES.md` | interaction ownership + hold/authorization process + correction history | Interactions | `systems/interactions/README.md` | `mechanics/interactions/interaction-modification.md` |
| `LOCALISATION_BABEL.md` | localisation architecture + custom-text procedures + failures | Localisation | `systems/localisation/README.md` | `mechanics/localisation/custom-text.md` |
| `MAP_DISCOVERY_FAST_TRAVEL.md` | map/discovery architecture + fast-travel intervention + failures | World/travel | `systems/world/map-discovery-fast-travel.md` | `mechanics/world/map-discovery-fast-travel.md` |
| `MAP_TRAVEL.md` | portal/travel architecture + observation/intervention + failures | World/travel | `systems/world/map-portals-travel.md` | `mechanics/world/map-portals-travel.md` |
| `WORLD_SCENES_TRAVEL.md` | scene/travel architecture + integration procedure + failures | World/travel | `systems/world/scenes-and-travel.md` | `mechanics/world/scene-and-travel-intervention.md` |
| `PROGRESSION_SKILLS.md` | progression ownership + tuning procedure + persistence pitfalls | Progression | `systems/progression/README.md` | `mechanics/progression/tuning-and-effects.md` |
| `SPELLS_EFFECTS.md` | spell/effect architecture + intervention/VFX procedure + failures | Magic | `systems/magic/spells-and-effects.md` | `mechanics/magic/spell-and-vfx-intervention.md` |
| `STATUS_EFFECTS.md` | status/buildup architecture + mutation procedure + failures | Magic | `systems/magic/status-effects.md` | `mechanics/magic/status-effect-intervention.md` |
| `STORY_QUEST_DIALOGUE.md` | Story architecture + choice/intervention procedure + failures | Story | `systems/story/README.md` | `mechanics/story/observation-and-intervention.md` |
| `WEATHER_ENVIRONMENT.md` | environment/weather ownership + provider/tuning procedure + failures | Environment | `systems/environment/README.md` | `mechanics/environment/weather-and-presentation.md` |
| `WORLD_PLACEMENT_NAVIGATION.md` | placement/navigation architecture + spawn-safety procedure + failures | World/placement | `systems/world/placement-and-navigation.md` | `mechanics/world/placement-and-spawn-safety.md` |

### Packet families

1. AI
2. Assets
3. Audio
4. Combat
5. Crime
6. Interactions
7. Localisation
8. World/travel
9. Progression
10. Magic
11. Story
12. Environment/world placement

The World/travel packet owns the three travel/scene pages plus world-placement boundaries where they intersect, but `WORLD_PLACEMENT_NAVIGATION.md` retains its own canonical system/mechanic pair because placement safety is a distinct responsibility.

---

## B. Single-role pages — direct migration, no packet

| Legacy page | Archetype / responsibility | Canonical destination |
|---|---|---|
| `ASSEMBLIES_SYSTEM_OWNERS.md` | reference lookup: assembly → native owner | `reference/assemblies-system-owners.md` |
| `CONTENT_DOMAINS.md` | reference/navigation: content-domain owner map | `reference/content-domains.md` |
| `GAME_KNOWLEDGE_INDEX.md` | reference index: factual subject domains | `reference/game-knowledge/README.md` |
| `GAME_SYSTEMS_MAP.md` | reference/navigation: knowledge/system/mechanic routing map | `reference/game-systems-map.md` |
| `GOLDEN_RULES.md` | cross-cutting learning/explanation | `learn/understand-foa/golden-rules.md` |
| `IDENTITY_GUIDS_NAMES.md` | reference: identity kinds and stable IDs | `reference/identities/identity-kinds.md` |
| `LIFECYCLE_HOOKS.md` | mechanic: lifecycle-aware Harmony hook selection | `mechanics/harmony-hooks/lifecycle-hooks.md` |
| `PERSISTENCE_ARCHITECTURE.md` | system explanation: persistence ownership | `systems/persistence/architecture.md` |
| `PRIVATE_APIS_COMPATIBILITY.md` | compatibility/evidence reference | `reference/compatibility/private-apis.md` |
| `PROPRIETARY_RENDERING_SYSTEMS.md` | native-system hub: specialised rendering ownership | `systems/rendering/README.md` |
| `RECIPES_CRAFTING.md` | native-system boundary: recipe/crafting ownership | `systems/crafting/README.md` |
| `RECIPES_ECONOMY.md` | native-system boundary: crafting/economy ownership | `systems/crafting/economy-boundary.md` |
| `RESOURCE_LIFETIME.md` | native-system explanation: asset/resource lifetime | `systems/assets/resource-lifetime.md` |
| `SERIALIZATION_ARCHIVES.md` | native-system explanation: serialization/container boundary | `systems/core/serialization-and-archives.md` |

These pages may retain their current technical body because the audit found one dominant reader job rather than competing system/mechanic/case responsibilities.

---

## C. Small aliases — direct redirect

| Alias | Current target | Wave 5 canonical target |
|---|---|---|
| `LOCALIZATION_BABEL.md` | `LOCALISATION_BABEL.md` | `systems/localisation/README.md` |
| `SPELLS_MAGIC_EFFECTS.md` | `SPELLS_EFFECTS.md` | `systems/magic/spells-and-effects.md` |
| `STORY_DIALOGUE_CHOICES.md` | `STORY_QUEST_DIALOGUE.md` | `systems/story/README.md` |
| `UI_INPUT_INTEGRATION.md` | `UI_INPUT.md` | `systems/ui-input/README.md` |

---

## D. Already-migrated redirects — excluded

Wave 5 does not reprocess the existing redirect files created by Waves 1–4, including the legacy paths for:

- Items, Weapons, Armour, Creatures;
- actor/spawning pages;
- UI/input;
- persistence;
- runtime/core ownership pages;
- diagnostics;
- evidence/validation;
- hooks/identity/mechanics catalogues;
- reverse-engineering and intervention-selection pages.

Their current role is compatibility only.

---

## Mixed-page split rule

For each mixed legacy page:

### System page owns

- what the system is;
- native/game owners;
- exact identities/types/methods needed to understand ownership;
- lifecycle/data flow;
- system-scoped proof boundary.

### Mechanic page owns

- how a mod may interact;
- why the chosen intervention is appropriate;
- rejected/failed approaches;
- how to verify the intervention;
- explicit link back to the canonical system owner.

### Legacy page

Becomes a compatibility redirect to the system page, which in turn links the mechanic.

No page is split merely because of file size.

---

## Evidence boundary for this migration

Wave 5 is an **information-responsibility migration**, not a fresh technical-claim promotion event.

- Existing technical wording is preserved or cleanly separated.
- No stronger runtime/save/compatibility/release status may be introduced.
- New system/mechanic pages must identify that the split does not itself revalidate every underlying technical claim.
- Private production source, bulk decompilation and proprietary assets remain excluded.
- Any new technical claim beyond the current public body requires a separate evidence review.


## Completion result

Wave 5 completed the audit-driven migration exactly as classified above:

- **18 / 18 mixed-role substantive pages: PASSED**
  - every page received a packet before migration;
  - every page was split into a canonical native-system page plus a canonical mechanic page;
  - every legacy path now redirects.
- **14 / 14 single-role substantive pages: PASSED**
  - migrated directly to their canonical surface;
  - no artificial packet was created;
  - every legacy path now redirects.
- **4 / 4 small aliases: PASSED**
  - each alias now redirects directly to its canonical owner.
- **Previously migrated Waves 1–4 redirects: preserved.**

### Legacy-tree validation

Validation against `docs/researched-information-architecture` found:

- legacy `docs/reference/` file count: **61**;
- files larger than 500 bytes: **0**;
- largest remaining legacy file: **406 bytes**;
- sampled mixed-role legacy pages begin with `# Moved`;
- sampled alias pages begin with `# Moved`;
- therefore `docs/reference/` is now a **compatibility-only surface**, not a substantive knowledge owner.

### Branch / execution boundary

- branch comparison to `main`: ahead, not behind at the validation point;
- repository mutation in this wave: documentation/navigation only;
- new runtime validation: **NOT_RUN**;
- new save/persistence validation: **NOT_RUN**;
- new decompilation/static extraction: **NOT_RUN**;
- compatibility/release validation: **NOT_RUN**;
- hosted commit-status entries at the validation point: **none reported**.

This audit does not convert existing technical claims into stronger evidence. It only establishes their canonical public documentation ownership.
