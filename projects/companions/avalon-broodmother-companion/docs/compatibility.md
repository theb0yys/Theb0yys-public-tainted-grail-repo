# Avalon Broodmother Companion Compatibility

Current status: six custom spider-family spell items, known merchant stock, one-session companion cast-intercept, native `Companion` command-dialogue prompt, damage bridge, initialized native animated-death visibility correction, and same-session native death handoff implementation. Live recast/combat/death compatibility validation remains a separate gate.

- Avalon Awakened: required at runtime for the public Broodmother and Spider resolvers and the current Broodmother/Spider LocationTemplates.
- Avalon Summons: not modified. This mod does not register custom summon templates.
- BetterSummon: unknown until a separate live compatibility gate.
- Magic Tweaks: not modified. This mod does not tune spells.
- Native spells: registers six cloned `Wolf's Call` item templates as the Broodmother and smaller Spider call items; exact custom item casts are intercepted before native `SkillSpawnLocation` and routed through the one-session companion path.
- Native combat/damage: the active companion registers runtime `HealthElement` hitboxes only on its own spawned actor and uses the same bounded ShortBite/LeapJump bridge through native `HealthElement.TakeDamage(Damage)`. This can overlap with other mods that observe or alter `HealthElement.TakeDamage`, but it does not patch that method or change global NPC damage.
- Merchant stock: targets only configured shop `75a071140bc819d4ab6e9e37abfdfa59` / `Shop_Vendor_Tier1` through the decompressed `RestockableStock` route and adds all six call items there. Does not add all-merchant, loot, reward, recipe, or container distribution.
- FoA Mod Manager: basic settings are exposed through normal BepInEx `Config.Bind` entries. There is no hard FoA Mod Manager dependency.
- Tainted Interface: optional soft-reflection route for item/companion icon fallback and the first-choice companion menu custom-UI scope. The menu falls back to FoA Mod Manager custom UI scope by soft reflection if Tainted Interface cannot acquire the scope. The three Broodmother variant icons and three smaller Spider variant icons are embedded in this plugin. There is no hard project, package, or runtime dependency.
