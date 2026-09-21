# Research: Pet and Summon Targets

Date: 2026-06-14
Scope: identify safe first technical targets for Avalon Companions.
Question: does FoA already have pet, summon, or ally systems that are safer than direct NPC spawning for a companion prototype?
Game version and branch: local Mono install at `<local-path>`; exact in-game version not verified.
Tools used: `ilspycmd` 10.1.0.8386 against `Fall of Avalon_Data\Managed\TG.Main.dll`; addressable catalog string search against `Fall of Avalon_Data\StreamingAssets\aa\catalog.json`.

## Evidence read

- Repo baseline modding research and engineering process.
- `mods/avalon-companions/docs/research.md`.
- `mods/living-avalon/docs/research/spawn-targets.md`.
- `mods/living-avalon/docs/research/contextual-spawn-diagnostics-2026-06-14.md`.
- Focused local decompilation of pet, summon, ally, story-step, template, and common-reference types.

## Decompiled or inspected targets

- Assembly: `TG.Main.dll`.
- Type: `Awaken.TG.Main.Locations.Pets.PetAttachment`.
  - Notes: attachment spawns a `PetElement`; identifies ownership by `element is PetElement`.
- Type: `Awaken.TG.Main.Locations.Pets.PetElement`.
  - Notes: owns saved follow state and saved weak target reference. On initialize, defaults target to `Hero.Current` and following enabled when no saved target exists. Listens for target teleports, recalls to coordinates, and can play pet/taunt animations. Uses `GameplayUniqueLocation` and `NpcPresence.AbyssPosition` during some scene/loading transitions.
- Type: `Awaken.TG.Main.Locations.Pets.VCPetController`.
  - Notes: requires `RichAI`; controls follow movement, run/sprint thresholds, rotation, visual-ground adjustment, and teleport-near-target behavior through A* pathfinding. Uses `GameplayUniqueLocation.TeleportIntoCurrentScene` when available, otherwise moves the location directly.
- Type: `Awaken.TG.Main.Locations.Pets.PetUtils`.
  - Notes: exposes `RecallPet(Vector3)` and `HasPetBeenLeftBehind()`. Recalls the active pet variant or the first world `PetElement`.
- Type: `Awaken.TG.Main.Locations.Pets.Variants.PetVariantAttachment`.
  - Notes: supports `Normal`, `AoE`, `Mount`, and `NpcAlly` variant element types.
- Type: `Awaken.TG.Main.Locations.Pets.Variants.PetVariantBase`.
  - Notes: saved timer/spawn/follow fields; variant transformation uses a `TemplateReference` to a `LocationTemplate`, calls `LocationTemplate.SpawnLocation(...)`, transfers follow state, and ends the previous variant. Ended variants are marked not saved before discard.
- Type: `Awaken.TG.Main.Locations.Pets.Variants.PetVariant`.
  - Notes: base pet variant forwards pet, taunt, feed, and follow-state changes to `PetElement`.
- Type: `Awaken.TG.Main.Locations.Pets.Variants.NpcAllyPetVariant`.
  - Notes: expects an `NpcElement`, overrides its faction to `Hero.Current.GetFactionTemplateForSummon()` with summon context, and adds `NpcHeroPetAlly`.
- Type: `Awaken.TG.Main.AI.SummonsAndAllies.NpcAlly`.
  - Notes: saved weak ally reference; changes faction to ally summon faction, adds movement provider, follows/patrols around ally, enters combat with ally attackers, teleports back when too far, and resets faction on discard.
- Type: `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroSummon`.
  - Notes: derives from `NpcAlly`; marks NPC as hero summon, registers summon-spawned event, initializes unique location, removes search/pickpocket actions, handles portal/fast-travel/long-teleport repositioning, prevents friendly fire, uses summon limit, and usually destroys on rest.
- Type: `Awaken.TG.Main.AI.SummonsAndAllies.NpcHeroPetAlly`.
  - Notes: derives from `NpcHeroSummon`; `DestroyOnRest=false`, `Type=CharacterLimitedLocationType.None`, and `LimitForCharacter(...)` returns 1.
- Type: `Awaken.TG.Main.AI.Utils.SummonUtils`.
  - Notes: `InitializeSummon(...)` applies summon faction and adds `NpcHeroSummon` or `NpcAISummon`; `DestroyHeroSummonsExceptPets()` skips `NpcHeroPetAlly`.
- Type: `Awaken.TG.Main.Stories.Steps.SPetInteract`.
  - Notes: story step supports pet, taunt, stay, and follow against matching locations with `PetVariantBase`.
- Type: `Awaken.TG.Main.Stories.Steps.SPetSetVariant`.
  - Notes: story step feeds/transforms a matching pet variant through a template reference.
- Type: `Awaken.TG.Main.Stories.Conditions.CPetStatus`.
  - Notes: story condition can check whether a pet follows `Hero.Current` and whether a location is a non-base pet variant.
- Type: `Awaken.TG.Main.Stories.Steps.STeleportSummons`.
  - Notes: story step teleports all `NpcHeroSummon` instances around the hero using `NpcTeleporter`.
- Type: `Awaken.TG.Main.Scenes.SceneConstructors.CommonReferences`.
  - Notes: `CommonReferences.Get` is available through `World.Services`; it exposes `PetBaseVariant` and `QrkoMountTemplates`.
- Type: `Awaken.TG.Main.Templates.TemplatesProvider`.
  - Notes: exposes `GetAllOfType<T>()` for loaded templates.
- Type: `Awaken.TG.Main.Locations.Setup.LocationTemplate`.
  - Notes: exposes overloads of `SpawnLocation(...)`; this is a real spawn API but should not be used until template safety and save/load behavior are validated.
- Addressable catalog:
  - Candidate paths found: `Assets/Data/LocationSpecs/AI/Cleanup/Spec_Pet_Qrko.prefab`, `Spec_Pet_Qrko_02.prefab`, `Spec_Pet_Qrko_03.prefab`, and `Spec_Pet_Qrko_05.prefab`.

## Findings

- Documented:
  - FoA already has a built-in pet system with saved follow state, hero targeting, teleport/catch-up behavior, pet/stay/follow commands, variant transformations, and a specific NPC-ally pet variant.
  - FoA already has ally/summon elements that handle ally following, combat target sharing, faction override, teleport-to-ally behavior, portal/fast-travel handling, collision/friendly-fire handling, summon limits, and rest destruction rules.
  - `NpcHeroPetAlly` is explicitly different from ordinary hero summons: it is not destroyed on rest, has no character-limited-location type, and is limited to one.
  - Existing story steps already express pet interaction and pet variant changes.
  - The addressable catalog contains Qrko pet location specs, but string presence is taxonomy evidence only.
- Inferred:
  - The safest first companion lane is an existing pet-system lane, not arbitrary NPC spawning or custom follower AI.
  - A non-combat or pet-companion prototype should start by observing existing pet references and loaded templates, then only use the game's own pet/variant mechanics after runtime validation.
  - A humanoid combat follower is possible in principle through `NpcAlly`/`NpcHeroSummon`/`NpcHeroPetAlly`, but it is higher risk than a pet because actor template choice, faction side effects, combat AI, and save/load behavior still need proof.
- Unknown:
  - Whether the player normally has a `PetElement` or active `PetVariantBase` in a fresh or mid-game save.
  - Which exact `Spec_Pet_Qrko*` template maps to `CommonReferences.Get.PetBaseVariant`.
  - Whether spawning a pet template from a plugin survives load, area transition, quit, reload, and return.
  - Whether a plugin can safely create or transform pet variants without story-owned state.
  - Whether a humanoid NPC template can be safely converted to `NpcHeroPetAlly` without quest/story ownership conflicts.

## Implementation boundary

- Implemented next code step: disabled-by-default diagnostics that log pet-system availability, current `PetElement`/`PetVariantBase`/`NpcHeroSummon` counts, `CommonReferences.Get.PetBaseVariant` availability, and loaded `Spec_Pet_*` template candidates with canonical GUIDs.
- Implemented disabled-by-default CSV output for loaded `Spec_Pet_*` `LocationTemplate` candidates. The dump is taxonomy evidence only and marks every row as not approved for spawn or transformation.
- Implemented opt-in Qrko pet companion roster after successful diagnostics. Do not call `LocationTemplate.SpawnLocation(...)` outside that explicit roster path.
- Do not add `NpcAlly`, `NpcHeroSummon`, or `NpcHeroPetAlly` to any runtime actor yet.
- Do not transform pet variants yet.
- Do not alter faction overrides, summon limits, story flags, NPC presence, unique NPC stash state, or save data.

2026-06-15 update: `mods/avalon-companions/docs/research/wolf-bear-native-ally-marker-2026-06-15.md` supersedes only the `NpcHeroPetAlly` and faction-override block for the two plugin-owned one-session wolf/bear candidates. The approved path mirrors `NpcAllyPetVariant.OnSpawned()` and remains blocked for humanoids, wild conversion, persistence, save/load support, custom defend behavior, and template expansion.

2026-06-15 defend updates: `mods/avalon-companions/docs/research/wolf-bear-native-defend-assist-2026-06-15.md` supersedes only the native attack-response and explicit panel `Defend` mode for the two plugin-owned one-session wolf/bear candidates. The approved path still calls `NpcHeroPetAlly.EnterCombat()` only when the hero already has live attackers and remains blocked for humanoids, wild conversion, persistence, save/load support, custom target selection, custom attack commands, defend hotkeys, and template expansion.

2026-06-15 animal expansion update: `mods/avalon-companions/docs/research/animal-roster-expansion-2026-06-15.md` extends the same explicit-command, one-session, not-saved native ally path to four manually reviewed animal/creature rows: deer, pig, cow, and bullrat. It does not approve persistence, custom target selection, custom attack commands, defend hotkeys, wild conversion, humanoid companions, or broad template expansion.

## Validation needed

- Build a diagnostic-only patch/config path.
- Launch on a throwaway save with diagnostics enabled.
- Confirm BepInEx log reports whether a pet exists and which pet templates are loaded.
- If a pet exists, test story/native pet commands before creating any plugin-owned pet behavior.
- Before any spawn/transform prototype: load, spawn or transform, follow, stay, pet/taunt, transition area, fast travel, rest, quit, reload, return, and inspect BepInEx log for errors.
