# Avalon Broodmother Companion Native Spell Implementation Notes

Date: 2026-08-06

Status: PASS for source implementation and Release build. BLOCKED for live validation.

## Implemented route

- Register a new mod-owned spell item template:
  - display name: `Broodmother's Call`
  - template name: `ItemTemplate_Magic_Tier1_AvalonBroodmotherCall`
  - template GUID: `b7d0d4e6c0de4bb0a000000000000001`
- Clone native `Wolf's Call`, GUID `3bd577472a0191c44bf298a82553cf3b`.
- Keep the cloned native item components, icon, audio, stats, and `Magic_Summon_Ally` graph.
- Rewrite only copied runtime `SkillReference` data for the custom spell GUID.
- Replace only the `SummonPrefab` skill template with Broodmother LocationTemplate GUID `efa4bbbdea2fb744fa293152772cd544`.
- Keep `Spell.GrantBroodmotherCallOnLoad=false` by default because granting the spell to the hero is save-visible.

## Evidence used

- `mods/avalon-awakened/docs/research/broodmother-first-s1b-a-wolfs-call-burning-ember-trace-2026-08-06.md` records `Wolf's Call`, `Magic_Summon_Ally`, and the `SummonPrefab` override.
- `docs/research/frameworks/tainted-content-import-process-2026-08-05.md` records native `ItemTemplate` cloning, `TemplatesLoader.AddToMap`, `TemplatesProvider` lookup, `World.Add(new Item(...))`, and hero grants as source-confirmed patterns.
- `mods/tainted-lockpick/src/Patches/CustomLockpickTemplateRegistry.cs` provides the cloned item-template registration pattern.
- `mods/tainted-lockpick/src/Plugin.cs` provides the custom item grant pattern.
- `mods/food-drink-asset-proof/src/Patches/CustomItemNativeEffectPatch.cs` provides the custom-item-only `SkillReference` rewrite pattern.
- Local decompilation of installed `TG.Main.dll` confirmed:
  - `ItemEffectsSpec` owns private `List<SkillReference> skills`;
  - `ItemEffects.OnInitialize()` calls `SkillInitialization.Initialize(this, _spec.SkillRefsFromSpec(this), SkillState)`;
  - `SkillReference.Copy()` deep-copies `templates`;
  - `SkillTemplate` has public `name` and `TemplateReference templateReference`;
  - `TemplateReference(string guid)` is public.

## Still blocked

- FoA launch.
- Save access.
- Granting the custom spell on a real save.
- Live cast proof.
- Live Broodmother hostility, cleanup, transition, save/load, rest, and quit/reload claims.
- BetterSummon compatibility.
- Custom icon binding.
- Native spellbook UI/localization table edits.
- Public packaging.
