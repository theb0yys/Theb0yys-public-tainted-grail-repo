# Avalon Broodmother Companion Design

The mod combines a small explicit-command spider-family companion owner for `Spec_Broodmother_CI4` and `Spec_Spider_CI4` with six mod-owned call spell items cloned from native `Wolf's Call`.

Design rules:
- Resolve Broodmother and smaller Spider companions only through Avalon Awakened's public resolvers.
- Do not use raw template references or fallback GUID loading in this mod.
- Register the six spider-family call items by cloning only native `Wolf's Call` and inserting the clones through the proven private loader-map route.
- Add the six spider-family call items only to the configured known merchant stock by patching `ShopUI.OnFullyInitialized()` before the shop item list is captured.
- Default merchant target is `75a071140bc819d4ab6e9e37abfdfa59` / `Shop_Vendor_Tier1`; do not add the spell to every loaded merchant.
- Use duplicate guards for already-present shop items and already-added-this-session stock keys.
- Mutate neither native `Wolf's Call` nor Avalon Awakened's Broodmother/Spider templates.
- Keep each call item's description aligned with the actual companion route: allied one-session selected companion, one active spider-family companion, and recast replacement.
- Keep each call item's light/heavy cast rows aligned with the active route: `Summon` type and selected-companion summon/replace descriptions, never inherited wolf-command text.
- Apply custom item icon overrides scoped only to the six call items; use the embedded red/black, pale, and gold Broodmother images for the three Broodmother variants and the embedded red/black, pale, and gold smaller Spider images for the three smaller Spider variants, then fall back to Tainted Interface reflection (`GetItemIcon(custom spell GUID)`, then `GetIcon(companion.creature)`) and the procedural spider icon without adding a hard Tainted Interface dependency.
- Embed the user-supplied spider creature audio as plugin-owned PCM WAV resources. Play those resources through FoA's game-shipped FMOD Core path as 3D one-shot sidecar cues on spider-family companion events only: summon/recall call, native combat handoff attack, and follow movement distance. Do not mutate native `AliveAudioAttachment`, `ItemAudioAttachment`, FMOD Studio banks/events, Wolf's Call audio, Avalon Awakened templates, or shared audio registries.
- Intercept only the exact custom spell items' `Skill.Perform()` calls and route those casts through the one-session companion summon path.
- Keep the legacy copied runtime `SkillReference` `SummonPrefab` rewrite disabled by default because the native `SkillSpawnLocation` route crashed with the Broodmother template.
- Keep spell grants disabled by default because adding the item to hero inventory is save-visible.
- Block hotkey and panel summon commands unless `General.Enabled=true` and `Safety.AllowRuntimeSummon=true`; spell casts are separately gated by `Spell.EnableBroodmotherCallCompanionRoute`.
- Poll only hotkeys that are not `KeyCode.None`; keep runtime summon/recall/defend/dismiss/panel hotkeys default-disabled, and keep companion dialogue entry on the native `Companion` prompt. If FoA attaches the action but does not start it, the Broodmother may open the same menu from the native `E` interaction key only while the active actor is near and camera-focused.
- Keep exactly one active spider-family companion reference in memory.
- Spawn the companion with the selected definition's display-name override and validate `Location.DisplayName` so native health-bar naming has the exact selected label.
- Before spawn, set the resolved `RepetitiveNpcAttachment` visual reference to the selected definition's exact visual address and read it back. Broodmother variants use the Broodmother CI4 template and `5.5` visual scale multiplier; smaller Spider variants use the Spider CI4 template and `1.0` visual scale multiplier.
- Mark the active location not saved immediately after spawn and before every recall, dismiss, failure cleanup, and shutdown cleanup.
- Reject and discard spawned locations without `NpcElement`, with unique NPCs, or with identity mismatch.
- Convert only the spawned instance through the native summon faction plus `NpcHeroPetAlly`.
- Immediately re-arm the spawned companion's native lifecycle surface: set and read back `NpcElement.KeepCorpseAfterDeath=true`, require native `DeathElement`, remove the stock summon `HideHealthBar` marker from the runtime location, remove `HeroSummonInvisibility` from the active NPC, re-enable native AlivePrefab colliders every maintenance tick, register enabled actor colliders as runtime `HealthElement` hitboxes, temporarily move those objects to FoA's `Hitboxes` layer, require at least one registered native health hitbox once the controller is ready, require initialized `PlayAnimationDeathBehaviour` linked from the active `DeathElement`, require `DeathElement.KeepBody=true`, clear only the documented `PostponedRagdollBehaviourBase._alivePrefab` suppression target, and accept death only through the native `NpcDummy` plus `Corpse` handoff with the selected definition's exact NPC-template GUID. A dead or discarded live `NpcElement` must enter a pending handoff state instead of immediately clearing the active location; the command surface/audio are closed, the same-session not-saved location is retained, `BROODMOTHER_COMPANION_NATIVE_DEATH_PENDING` is logged once, and accepted death clears command state while retaining the runtime material references needed by the visible corpse/body.
- Bypass native `NpcHeroSummon.TryPreventFriendlyFire` only when the summon instance is the active spider-family companion and the damage dealer is the current hero, so the companion can be hit and validated without changing other summons.
- Bridge only event-free ShortBite/LeapJump commits, not broad combat, by sending a normal native `Damage` object through the live target's `HealthElement.TakeDamage(Damage)` after the existing native `NpcAI.EnterCombatWith(...)` / `NpcHeroPetAlly.EnterCombat()` handoff succeeds. The bridge must not target the hero, must stay inside the documented spider release ranges, must use active companion NPC melee damage as the source value, and must log `BROODMOTHER_COMPANION_DAMAGE_BRIDGE`. Successful LeapJump bridge damage may spawn a transient Broodmother-owned Unity VFX sidecar; it must not mutate native VFX templates, shared registries, or asset bundles.
- Keep defend prompts disabled by default and only call native `NpcHeroPetAlly.EnterCombat()` when native combat candidate evidence exists.
- Advanced spider companion AI is allowed for native-recognized live combat candidates only. It must prioritize `Hero.PossibleAttackers`, then the companion `NpcElement.PossibleAttackers`, then the companion `NpcElement.PossibleTargets`; it may classify the attack envelope with the Avalon Awakened spider thresholds and hand combat to native `NpcAI.EnterCombatWith(ICharacter, bool)` plus `NpcHeroPetAlly.EnterCombat()`. Mid-range `Reposition` must drive native combat handoff instead of becoming a no-op `Hold`; ShortRange 16 remains restricted to valid bite/leap release windows.
- Immediate target pressure is allowed for native-recognized live combat candidates and may push the same native target combat handoff before the attack commit window so the companion starts pursuing a recognized live threat immediately.
- Keep the default maintenance tick and advanced combat handoff cadence responsive at `0.35` seconds, with migration from the previous `0.75` / `1.25` default values.
- Leap attack classification uses the proven ShortRange 16 / secondary short-range clip policy. Do not directly force `Spider_jump` or `Spider_jump_v2` until a native jump state is proven.
- V2 spawned Spider movement procedures must declare their movement animation stages explicitly before the root-motion stage and use the reviewed Spider movement state names. Root-motion stages must frame-advance while active from Unity update time; PlayMaker command polls observe completion and report telemetry but must not be the sole motion integrator. `leap.arc` remains Avalon-owned root motion/VFX only; it must not secretly bind a motion handle as an animation handle or inject clip playback. Jump animation belongs to the explicit `leap.animation` authored stage and may only pass when the spawned actor's live animator exposes the requested state.
- The Broodmother companion plugin may compile the primitive source-only `BroodmotherCompanionAiV1Contract` and `BroodmotherCompanionAiTypes` files directly from `mods/avalon-broodmother-companion/ai-package`. The AI authority contract must accept only the exact current Broodmother identity or exact current Spider identity. The live plugin must not add an Avalon AI runtime or package assembly dependency for this slice.
- `Dialogue.EnableNativeCompanionPrompt=true` may attach and re-synchronize one runtime-only native `Companion` `AbstractLocationAction` on the active verified companion. The `Companion` prompt opens this plugin's Unity UI companion menu using the Avalon Companions native command-dialogue layout: Follow, Hold Position, Defend, Keep Close, Keep Pace, Keep Distance, Come Close, Recall, Recover, Dismiss, Leave, selected mode/range highlights, bottom listening/status text, and a Tainted Interface companion portrait when available. It must not add an attack button, target selector, or separate command surface. If FoA does not start the attached action, `Dialogue.EnableNativeCompanionInteractFallback=true` opens the same menu from the native `E` interaction key only while the active companion is within distance and camera-focused. While this menu is open, the companion owner must not run follow catch-up, combat handoff, or AI movement ticks; command range choices must update spacing without hidden recall, leaving `Come Close` and `Recall` as the explicit relocation commands. The host must not keep a Broodmother-specific submit-release/debounce gate in front of command dispatch. Unity `Button.onClick` selection and manual hit-test selection both route through the same command handler, with only same-frame duplicate suppression so one click cannot double-run. The manual click fallback must copy the proven Avalon Companions path exactly: `Input.GetMouseButtonDown(0)`, one frame guard, real `Input.mousePosition`, visible enabled row hit-test, and direct dispatch through the same command handler, without Broodmother-specific pointer-release state. The UI owns cursor/input through the proven Avalon Companions scope order: Tainted Interface `BeginCustomUiScope` by soft reflection first, falling back to `FoAModManagerApi.SetCustomUiScope` by soft reflection if Tainted Interface cannot acquire the scope. While open it attaches FoA's native `ForceCursorVisibility` element to the current hero, captures/restores the real cursor state, reasserts the cursor as visible/unlocked through the same `EnsureInteractiveCursor` loop used by working companion menus, reasserts the same cursor/host/sync/manual-hit-test path from the `OnGUI` pass while visible, and does not draw a Broodmother-owned virtual cursor. Hover and the manual click fallback use Unity's real pointer position, matching the proven Avalon Companions Unity UI dialogue path. The UI uses soft Tainted Interface reflection for icons, with no Avalon Companions bridge/dependency, no hard FoA Mod Manager or Tainted Interface dependency, no story graph, no template edit, no native quick-command action stack, and no save-owned interaction.
- Non-terminal companion menu choices apply their command and keep the native command-dialogue menu open so selected mode/range highlights and bottom status text resync immediately. `Leave` closes the menu without running a command; `Dismiss` clears the active companion and closes the menu.
- The native prompt surface follows the proven companion process: keep one `Companion` menu action on the active actor, mark action and actor not saved, remove stale prompt/quick-command actions on invalid/dismiss/shutdown paths, set only the active runtime companion actor's `LocationInteractability` to `Active` while the prompt is attached, restore the original interactability during cleanup, and report whether the prompt surface is actually interactable.
- `Dialogue.ShowCompanionHud=true` draws a plugin-owned active companion HUD with display name, health percentage, follow/hold/defend mode, live threat count, native prompt status, and the selected available companion icon. The runtime also removes the native summon `HideHealthBar` marker so the game's own health-bar route is no longer deliberately suppressed for this actor.

## Approved Release-Polish Defaults (2026-08-09)

The current user approved these exact player-facing defaults for the remaining release-polish implementation:

- Smaller Spider visual multiplier: `5.5`, matching the Broodmother visual multiplier so the Spider-family call variants no longer spawn undersized.
- Broodmother names and primary combat functions:
  - `Broodmother Asha - Crimson Vanguard`: close bite pressure.
  - `Broodmother Velra - Pale Pouncer`: leap-focused flanking pressure.
  - `Broodmother Aurex - Gilded Spitter`: ranged spit pressure.
- Smaller Spider names and primary combat functions:
  - `Rook - Crimson Harrier Spider`: rapid bite and reposition pressure.
  - `Vesper - Pale Ambusher Spider`: wide-angle leap pressure.
  - `Nox - Gilded Finisher Spider`: weakened-target priority.

Each definition must receive a stable specialization input. Pack-index rotation may still be used inside a specialization, but it must not erase the six definition-level functions above. Broodmother display names must retain the word `Broodmother` plus their distinction.

## Offline Spider Animation/VFX Authoring Boundary (2026-08-09)

The current user authorizes offline Spider animation and VFX authoring only. This authoring slice may:

- extract one clean, event-free host-motion copy of the reviewed `Spider_jump_v2` source clip under a new stable address;
- keep that clip outside `ARStateToAnimationMapping` until a FoA-owned jump playback state or other complete runtime binding path is proven;
- create original ivory, amber, and gold particle prefabs for the Gilded Spitter muzzle, projectile, and impact phases;
- remove the current neon-green line language from the replacement asset specification;
- build an isolated Unity `6000.0.64f1` Addressables catalogue and validate two offline load/release cycles;
- render offline preview images for visual review.

This authoring slice must not launch FoA, access saves, deploy files to the live game, alter Spider/Broodmother templates, claim native jump-state ownership, add animation events, implement damage timing, or claim runtime integration. The companion remains the intended runtime binding owner, but the exact catalogue-loading, clip-playback, movement, damage-release, interruption, and cleanup path must be separately proven before compiled source changes.

Out of scope:
- Native spellbook UI editing.
- True native `VDialogue`, `StoryBookmark`, story graph, `DialogueAttachment`, `PetTalkAttachment`, or `StoryInteractAction` work.
- Recipe registration.
- All-merchant, loot, reward, or container distribution.
- Custom summon template registration.
- Shared Tainted Interface `companion.spider` / `companion.broodmother` icon catalog packaging.
- The second `skin1-2`/`skin2-2`/`skin3-2` Spider and Broodmother source roots.
- Shared/native audio bank editing or native actor-audio selector replacement.
- Random spawn, population, route, or scene integration.
- Companion persistence, restoration, and auto-respawn.
