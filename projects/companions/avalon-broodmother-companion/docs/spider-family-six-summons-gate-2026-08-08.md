# Spider Family Six Summons Gate

Date: 2026-08-08

Status: PASS for source implementation after the `0.1.30` no-stack merchant-stock correction and the `0.1.31` per-definition item-icon cache correction. Live validation remains separate.

## Requested outcome

Current user request: add the other two Broodmother spider texture variations, then add three smaller Spider summons, all available from the proven merchant, using the same one-session lifecycle, animation mapping, and embedded spider sound effects. Total requested result: three Broodmother summon items and three smaller Spider summon items.

This gate authorizes source changes only in `mods/avalon-broodmother-companion/` for the six summon-item route below. It does not authorize Avalon Awakened asset rebuilding, native spellbook UI edits, recipes, loot/reward/container distribution, random spawns, save-owned companions, persistent actors, public release claims, FoA launch, save access, or live validation.

## Locked checklist

- [x] Current user request selects the creature/runtime role/slice: six spell-triggered one-session spider-family companion summons, one active runtime companion at a time, merchant-stocked.
- [x] Source package and variant identity are recorded by the Spider/Broodmother CI4 proof: all six Spider source prefabs were selected for both common Spider and Broodmother tiers (`mods/avalon-awakened/docs/research/ci4-successor-spider-broodmother-template-proof-2026-08-03.md:14`, `mods/avalon-awakened/docs/research/ci4-successor-spider-broodmother-template-proof-2026-08-03.md:31`).
- [x] Native baseline, scale policy, and lifecycle state map are cited: the process record selected a Bear-derived hostile proof profile, common Spider and Broodmother scale tiers, and the Spider lifecycle states `Idle(1)`, `Movement(2)`, `ShortRange(16)`, `GetHit(32)`, and `Death(44)` (`mods/avalon-awakened/docs/current-creature-injection-process.md:343-344`, `mods/avalon-awakened/docs/research/ci4-successor-spider-broodmother-template-proof-2026-08-03.md:81-84`).
- [x] Current runtime template identity is source-confirmed, not taken from stale CI4 generated GUIDs: Broodmother is `Spec_Broodmother_CI4` / LocationTemplate `527d142b35e81a648b6e84ea921ac543` / NpcTemplate `7623d4729979e814ea64992d9a399e17`; Spider is `Spec_Spider_CI4` / LocationTemplate `bfcd6db0f66afc14c94fcb6a9dbde4e7` / NpcTemplate `5f36e2717dc68e04091fe7b0aa8bafe0` (`mods/avalon-awakened/src/BroodmotherCi4ControlledActorRuntime.cs:15-20`, `mods/avalon-awakened/src/SpiderCi4ControlledActorRuntime.cs:24-29`).
- [x] Resolver route is public and exact: Avalon Awakened exposes `TryResolveWorldBroodmotherTemplate` and `TryResolveWorldSpiderTemplate`, each delegating to the matching current runtime validator (`mods/avalon-awakened/src/AvalonCreatureWorldApi.cs:103-124`).
- [x] Companion route remains the locked one-session path: consume through public resolver, no raw fallback, explicit command/spell-like trigger, immediate not-saved spawn, exact identity validation, one active in-memory companion, native summon faction plus `NpcHeroPetAlly`, native prompt/custom UI path, cleanup, and no persistence (`mods/avalon-awakened/docs/canonical-custom-creature-importer-process.md:208-222`).
- [x] Visual-variant roots are exact and validated: common Spider `skin1-1`, `skin2-1`, `skin3-1` and Broodmother `skin1-1`, `skin2-1`, `skin3-1` are selected for this three-texture-family slice (`mods/avalon-awakened/docs/research/ci4-successor-spider-broodmother-template-proof-2026-08-03.md:54-72`).
- [x] Visual-variant route is source-backed: the generated template proof records both templates as canonical `skin1-1` visuals while retaining all explicit visual roots (`mods/avalon-awakened/docs/generated/spider-broodmother-ci4-template-finalized-2026-08-03.json:342-375`, `mods/avalon-awakened/docs/generated/spider-broodmother-ci4-template-finalized-2026-08-03.json:392-461`). Runtime code may set the in-memory `RepetitiveNpcAttachment` visual with `Setup(npcTemplate, new ARAssetReference(address))` and verify `VisualPrefab().Address`, following the local Goblin/Dragon Knight source precedent (`mods/avalon-awakened/src/GoblinCi4ControlledActorRuntime.cs:117-150`, `mods/dragon-knight-companion/src/Plugin.cs:374-402`).
- [x] Merchant route is the existing proven known-merchant route: `Shop_Vendor_Tier1` / `75a071140bc819d4ab6e9e37abfdfa59`, `ShopUI.OnFullyInitialized`, decompressed `RestockableStock`, `World.Add(new Item(...))`, `Stock.AddItem(...)`, duplicate guards, and visible-after-add logging; the `0.1.30` correction inserts the cloned custom call items with stock stacking disabled and logs a post-loop six-definition count (`mods/avalon-broodmother-companion/docs/research.md:10-11`, `mods/avalon-broodmother-companion/docs/research.md:55`, `mods/avalon-broodmother-companion/docs/validation-plan.md:20`).
- [x] Spell route is the existing custom item clone/register and exact custom-item cast intercept route. The current single-item route clones native `Wolf's Call`, registers through the private loader map, keeps grant disabled because it is save-visible, and intercepts exact custom item casts into the one-session companion path (`mods/avalon-broodmother-companion/docs/research.md:37-48`, `mods/avalon-broodmother-companion/docs/research.md:66`).
- [x] Embedded audio route is already spider-family scoped: nine validated PCM WAV spider resources are compiled into the plugin and played through FMOD Core 3D cues for summon/recall, attack, and follow movement, without native audio/template mutation (`mods/avalon-broodmother-companion/docs/research.md:62`, `mods/avalon-broodmother-companion/docs/validation-plan.md:18`).
- [x] Item icon route is six-definition safe: each definition maps to its matching embedded icon and owns an independent cached UI sprite, so refreshing one merchant/inventory row does not destroy another variant's assigned sprite (`mods/avalon-broodmother-companion/src/BroodmotherCallItemIconPatch.cs`, `mods/avalon-broodmother-companion/src/BroodmotherEmbeddedIconRuntime.cs`).
- [x] UI/command path remains the locked Broodmother command-dialogue process: source/deployed-binary assurance mapped spell intercept, active actor ownership, native `Companion` prompt, UI open/close, cursor/input routing, event dispatch, command execution, cleanup, and validation markers (`mods/avalon-broodmother-companion/docs/test-notes.md:13-24`).
- [x] Hard-stop scan result: this source slice has exact source package identity, native baseline, animation mapping, template GUIDs, resolver route, runtime ownership, companion path, merchant route, and validation commands. Live proof remains unrun and must not be claimed (`mods/avalon-awakened/docs/canonical-custom-creature-importer-process.md:288-322`).

## Six summon definitions

The request asks for three total variants per tier. This slice therefore uses the already active `skin1-1` plus the first root from the other two texture families, `skin2-1` and `skin3-1`. The second roots `skin1-2`, `skin2-2`, and `skin3-2` remain blocked until separately requested.

| Item | Template GUID | Target | Variant | Visual address |
|---|---|---|---|---|
| `Broodmother's Call` | `b7d0d4e6c0de4bb0a000000000000001` | Broodmother | `skin1-1` | `avalon-awakened/creatures/spider/broodmother/skin1-1/visual-v1` |
| `Broodmother's Call - Skin 2` | `b7d0d4e6c0de4bb0a000000000000002` | Broodmother | `skin2-1` | `avalon-awakened/creatures/spider/broodmother/skin2-1/visual-v1` |
| `Broodmother's Call - Skin 3` | `b7d0d4e6c0de4bb0a000000000000003` | Broodmother | `skin3-1` | `avalon-awakened/creatures/spider/broodmother/skin3-1/visual-v1` |
| `Spider's Call - Skin 1` | `b7d0d4e6c0de4bb0a000000000000004` | Spider | `skin1-1` | `avalon-awakened/creatures/spider/common/skin1-1/visual-v1` |
| `Spider's Call - Skin 2` | `b7d0d4e6c0de4bb0a000000000000005` | Spider | `skin2-1` | `avalon-awakened/creatures/spider/common/skin2-1/visual-v1` |
| `Spider's Call - Skin 3` | `b7d0d4e6c0de4bb0a000000000000006` | Spider | `skin3-1` | `avalon-awakened/creatures/spider/common/skin3-1/visual-v1` |

## Implementation bounds

- Register all six custom item templates through the existing Broodmother-owned custom spell registry.
- Add all six registered item templates to the configured known merchant through the existing duplicate-safe stock route, with stock stacking disabled for the custom call items.
- Route each exact custom item cast to the same one-session companion path, passing its selected definition.
- Keep one active runtime companion in memory. Casting a different variant dismisses/replaces the current active companion near the hero.
- Resolve Broodmother and Spider only through Avalon Awakened public resolvers. Do not add a raw template fallback.
- Before `SpawnLocation`, set and read back the in-memory `RepetitiveNpcAttachment` visual to the selected variant address. Do not mutate source assets, native bundles, Avalon Awakened files, or saves.
- Validate spawned `LocationTemplate`, `NpcTemplate`, display name, not-saved policy, ally marker, lifecycle surface, native prompt, and cleanup against the active definition.
- Generalize the source-only AI authority contract to accept either current Broodmother identity or current Spider identity. Keep direct native calls, native jump-state claims, random spawns, persistence, and save writes false.
- Use existing embedded spider audio for all six variants.

## Required validation

- `dotnet build mods/avalon-broodmother-companion/src/AvalonBroodmotherCompanion.csproj -c Release -p:FoAGameRoot=A:\SteamLibrary\steamapps\common\Tainted Grail FoA`
- `dotnet run --project mods/avalon-broodmother-companion/ai-package/tests/AvalonBroodmotherCompanion.AI.Package.V1.Fixtures/AvalonBroodmotherCompanion.AI.Package.V1.Fixtures.csproj -c Release`
- `git diff --check -- mods/avalon-broodmother-companion`
- Source scans must show no random spawn systems, no raw template fallback, no recipe/loot/reward/container distribution, no protected-file edits, no hard Avalon Companions bridge/API dependency, and no hard FoA Mod Manager or Tainted Interface reference.

## Remaining unproven claims

- No fresh FoA launch, merchant UI inspection, live cast, live visual-variant readback, live command-dialogue click, live combat, live death/corpse, save-exclusion readback, transition, shutdown cleanup, or BetterSummon compatibility claim is made by this source gate.
- The six summon items are source/build ready only after the required validation passes.
