# Avalon Broodmother Companion

Standalone spider-family companion mod for the Avalon Awakened Spider/Broodmother lane.

## Scope

- Uses Avalon Awakened's public `TryResolveWorldBroodmotherTemplate` and `TryResolveWorldSpiderTemplate` routes.
- Registers six mod-owned spell item templates cloned from native `Wolf's Call`: three Broodmother calls and three smaller Spider calls.
- Intercepts only those custom spell items' `Skill.Perform()` calls and routes casts through the guarded one-session spider-family companion summon path.
- Rewrites each custom call item and cast-row tooltip text so the inventory UI describes the selected companion route, not Wolf's Call.
- Overrides the Broodmother and smaller Spider call inventory/loadout icons with the matching embedded red/black, pale, and gold images.
- Embeds the supplied spider creature audio as validated PCM WAV resources and plays spider-family FMOD Core 3D cues for summon/recall calls, combat attacks, and follow movement.
- Adds all six custom call items to the known Tier 1 vendor stock when that shop opens.
- Can grant the custom spell item to the current hero when explicitly enabled.
- Spawns only by explicit player command.
- Keeps runtime summon disabled by default through `Safety.AllowRuntimeSummon=false`.
- Allows spell-triggered companion summoning through `Spell.EnableBroodmotherCallCompanionRoute=true`.
- Keeps spell grant disabled by default through `Spell.GrantBroodmotherCallOnLoad=false`.
- Uses no default summon, recall, defend, dismiss, or panel hotkeys. The active companion menu is one runtime-only native `Companion` prompt matching the Avalon Companions native command-dialogue surface, with a scoped native-interact recovery path on `E` only when the player is focused on the active companion and FoA does not start the attached action.
- Owns command-menu input through the proven Avalon Companions scope path: Tainted Interface custom UI scope first, FoA Mod Manager custom UI scope fallback, native `ForceCursorVisibility`, and real cursor capture/reassert/restore while the menu is visible.
- Tracks only one active spider-family companion in memory for the current session.
- Marks the spawned location not saved immediately and before recall, dismiss, failure cleanup, and shutdown cleanup.
- Applies the native summon faction plus `NpcHeroPetAlly`.
- Re-arms the active spider-family companion as a damageable native actor by setting and reading back `KeepCorpseAfterDeath=true`, removing the stock summon `HideHealthBar` and `HeroSummonInvisibility` markers, re-enabling native AlivePrefab colliders, registering runtime `HealthElement` hitboxes, requiring initialized native animated-death visibility, waiting for the native `NpcDummy` plus `Corpse` death handoff, and retaining same-session death visual material references after acceptance.
- Allows hero damage to the active companion by bypassing native summon-friendly-fire prevention for that actor only.
- Bridges event-free ShortBite/LeapJump commits through native `HealthElement.TakeDamage(Damage)` after the companion has accepted the native combat target, with a transient plugin-owned LeapJump visual effect on successful ranged/leap damage.
- Supports summon/swap, recall, follow, hold position, defend, close/normal/far follow range, come close, recover, and dismiss. Native menu range choices update spacing without hidden recall; `Come Close` and `Recall` remain the explicit relocation commands. Non-terminal native menu choices keep the menu open and resync selected highlights/status immediately; `Leave` and `Dismiss` close it.
- Embeds the current red/black, pale, and gold Broodmother icons plus matching red/black, pale, and gold smaller Spider icons for all six call items.
- Keeps native defend assist disabled by default.
- Includes a source-only Avalon AI Contracts V2 package for Broodmother and smaller Spider companion decisions. The package owns decision vocabulary only and makes no Unity, BepInEx, FoA, native, save, spawn, or persistence calls.

This mod does not add random spawning, recipe injection, custom summon template registration, the second `skin1-2`/`skin2-2`/`skin3-2` source roots, population rows, route rows, shared Tainted Interface icon catalog edits, a hard Tainted Interface dependency, native spellbook UI edits, native template audio mutation, persistence, actor restoration, or auto-respawn.

Known merchant:

- Default `MerchantStock.TargetShopGuid`: `75a071140bc819d4ab6e9e37abfdfa59` / `Shop_Vendor_Tier1`.
- Route: `ShopUI.OnFullyInitialized()` prefix, decompressed `RestockableStock`, `World.Add(new Item(...))`, then `Stock.AddItem(..., allowStacking: false)` with a six-definition stock count log.
- The plugin skips duplicates already in the opened shop or already added in the current session.

## Settings

- `General.Enabled`: enables the plugin.
- `Safety.AllowRuntimeSummon`: allows runtime summon commands when intentionally enabled.
- `Spell.RegisterBroodmotherCallSpell`: registers the six mod-owned spider-family call spell item templates.
- `Spell.EnableBroodmotherCallCompanionRoute`: intercepts spider-family call casts and uses the one-session companion route, default `true`.
- `Spell.EnableBroodmotherCallSummonTarget`: legacy native `Magic_Summon_Ally` target rewrite, default `false`.
- `Spell.GrantBroodmotherCallOnLoad`: grants the default `Broodmother's Call` item to the current hero when templates and inventory are ready, default `false`.
- `MerchantStock.Enabled`: adds all six spider-family call items to the configured merchant when that shop opens.
- `MerchantStock.TargetShopGuid`: exact `ShopTemplate` GUID to stock, default `75a071140bc819d4ab6e9e37abfdfa59`.
- `MerchantStock.Quantity`: quantity to add to merchant stock, default `1`.
- `Panel.TogglePanelHotkey`: optional command panel key, default `None`.
- `Input.SummonOrSwapHotkey`: optional summon/swap key, default `None`.
- `Input.RecallHotkey`: optional recall key, default `None`.
- `Input.DismissHotkey`: optional dismiss key, default `None`.
- `Input.DefendToggleHotkey`: optional follow/defend toggle key, default `None`.
- `Summon.DistanceMeters`: placement distance near the hero.
- `Summon.RightOffsetMeters`: side offset near the hero.
- `Follow.EnableCatchUpRecall`: allows follow catch-up recall.
- `Follow.CatchUpDistanceMeters`: catch-up threshold.
- `Defend.EnableNativeDefendAssist`: allows native defend prompts while the hero has live attackers.
- `CompanionAI.EnableAdvancedSpiderAI`: enables bounded spider companion decisions for native-recognized live combat candidates, default `true`.
- `CompanionAI.EnableLeapAttackRequests`: allows leap opportunity classification through the ShortRange 16 policy, default `true`.
- `CompanionAI.CombatPromptSeconds`: minimum seconds between advanced spider combat handoff prompts, default `0.35`.
- `CompanionAI.EnableImmediateTargetPressure`: immediately pushes native target combat handoff while the hero or Broodmother has a native-recognized live combat candidate, default `true`.
- `CompanionAI.ImmediateTargetPressureSeconds`: minimum seconds between immediate target pressure prompts, default `0.2`.
- `Dialogue.EnableNativeCompanionPrompt`: attaches one runtime-only native `Companion` prompt to the active companion, default `true`.
- `Dialogue.EnableNativeCompanionInteractFallback`: opens the same companion menu from native `E` interaction only when the active companion action is attached, nearby, and camera-focused, default `true`.
- `Dialogue.NativeCompanionInteractFallbackDistanceMeters`: maximum distance for the native-interact recovery path, default `8.0`.
- `Dialogue.NativeCompanionInteractFallbackAngleDegrees`: maximum camera angle for the native-interact recovery path, default `42.0`.
- `Dialogue.ShowCompanionHud`: shows the active companion name, health, mode, native-recognized threat count, and native prompt status, default `true`.
- `Audio.EnableEmbeddedSpiderAudio`: enables embedded Broodmother spider audio cues, default `true`.
- `Audio.EmbeddedSpiderAudioVolume`: volume multiplier for embedded audio cues, default `0.72`.
- `Audio.EmbeddedSpiderAudioMinDistance`: minimum 3D attenuation distance, default `1.0`.
- `Audio.EmbeddedSpiderAudioMaxDistance`: maximum 3D attenuation distance, default `28.0`.
- `Audio.EmbeddedSpiderCallCooldownSeconds`: minimum seconds between long call cues, default `60.0`.
- `Audio.EmbeddedSpiderAttackCooldownSeconds`: minimum seconds between attack cues, default `1.2`.
- `Audio.EmbeddedSpiderWalkCooldownSeconds`: minimum seconds between walking cues, default `4.0`.
- `Runtime.TickSeconds`: maintenance tick interval, default `0.35`.
- `Diagnostics.WriteCommandLog`: writes plugin-owned command decisions to `broodmother-command-log.csv`.
