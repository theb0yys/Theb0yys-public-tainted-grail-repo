# Configuration

Use this page when you are designing **player settings, feature toggles, presets, hotkeys, or migration of old cfg files** for a FoA mod.

This is about patterns seen in FoA mods, not a replacement for generic BepInEx configuration documentation.

## A clean startup binding pattern

Many public FoA mods avoid saving the config file after every individual `Bind` during startup:

~~~text
SaveOnConfigSet = false
→ bind all settings
→ Save()
→ restore SaveOnConfigSet
~~~

That reduces repeated file writes while the plugin constructs its settings.

## Organize settings for the player

Useful public patterns include:

- clear sections such as `Weight`, `FallDamage`, `ItemSorting`, or `Language`;
- enum choices for modes/profiles;
- `AcceptableValueRange<T>` for stable numeric limits;
- descriptions that explain gameplay effects, not just the raw number;
- separate display-name/language settings for text introduced by the mod.

Prefer a small understandable player-facing surface over exposing every internal tuning value.

## Validate values twice

If a safe range is known, express it in the config metadata:

~~~csharp
new AcceptableValueRange<float>(minimum, maximum)
~~~

Still validate or clamp before applying the value to game state.

Users can edit cfg files manually, and old values can survive upgrades.

A BepInEx setting accepting a value does not prove the native FoA system will handle that value safely.

## Player settings vs advanced settings

Large mods often need more internal controls than normal players should see.

A useful pattern is:

- show `Enabled`, high-level modes, presets, and common user choices;
- hide diagnostics/internal tuning from ConfigurationManager-style UIs;
- keep hidden keys in the raw cfg for compatibility, debugging, and recovery.

Hiding a key from a UI should not silently rename or delete its persisted value.

## Presets

When many values work together, use a high-level preset plus an advanced/raw mode:

~~~text
Preset
├─ Subtle
├─ Standard
├─ Strong
└─ Custom / RawConfig
   └─ individual values
~~~

When preset names change, keep a legacy alias or explicit migration if the meaning is still compatible.

## Does a setting change live?

Do not assume that changing `ConfigEntry<T>.Value` automatically changes the running game.

Classify each setting by what the mod must do:

| Setting behavior | What your code must do |
| --- | --- |
| read on use | read the latest value whenever the native action happens |
| live reapply | handle `SettingChanged` and reapply owned runtime state |
| patch control | install/uninstall or otherwise transition the patch safely |
| structural/startup | require restart or controlled reinitialization |

State clearly in the README/config description whether a setting is live or restart-required.

## SettingChanged handlers

Use `SettingChanged` only when the feature can be safely reapplied.

A good handler:

1. validates/clamps the value;
2. updates only mod-owned state;
3. does not duplicate Elements, listeners, or patches;
4. uses a known invalidation/reapply path;
5. is unsubscribed during teardown.

Do not let config changes become a second uncontrolled initialization path.

## Migration

FoA mod source demonstrates several migration techniques:

- schema/version markers;
- backup before destructive reset;
- preserving compatible values across a schema change;
- legacy enum/profile aliases;
- moving an old plugin-GUID cfg to the new filename and calling `Config.Reload()`;
- one-time default migration when an old default is known to be undesirable.

Migration should be deterministic and idempotent: starting the game again should not keep rewriting already-migrated settings.

## Save and reload operations

Useful operations include:

- `Config.Save()` after a player-triggered change that must persist;
- `Config.Save()` during controlled teardown/normalization;
- `Config.Reload()` after an intentional file migration.

Do not repeatedly reload the file just to avoid maintaining correct runtime state.

## Config is not game-save data

Use BepInEx config for:

- preferences;
- feature toggles;
- tuning;
- mod-owned modes and presets;
- hotkeys;
- diagnostics.

Do not store game-owned progression, inventory, quest, or world state in config merely because cfg files survive restarts.

## Compatibility rules

- keep section/key names stable unless you migrate them;
- never reuse an old key for a different meaning without migration;
- treat config-UI metadata as presentation only;
- keep raw cfg recovery possible for advanced/hidden settings when practical;
- document live vs restart-required behavior;
- remember that two mods having different config keys does not prevent them from conflicting on the same native system.

## Evidence

Public patterns come from public FoA mod source including `jonanoj/FallOfAvalonMods`, `apodworny/FallOfAvalonMods`, and `keenanselbee/grailwright`.

Additional migration/config-UI patterns come from internal project evidence published under the repository's public-safe evidence rules.

See [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
