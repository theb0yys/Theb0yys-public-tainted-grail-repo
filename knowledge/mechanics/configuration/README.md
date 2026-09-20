# Configuration

Use this page when you are designing player settings, feature toggles, presets, live config changes, or migration of older cfg files for a FoA mod.
This page focuses on configuration patterns that have been useful in FoA mods. For the BepInEx API itself, use the upstream BepInEx documentation.

## Public FoA configuration patterns

Public FoA mods repeatedly use BepInEx `ConfigFile` / `ConfigEntry<T>` as the normal user-config surface.

A common binding pattern is:

~~~text
temporarily disable SaveOnConfigSet
→ bind the complete config surface
→ save once
→ restore SaveOnConfigSet
~~~

This avoids rewriting the cfg repeatedly while the plugin is creating many entries during startup.

Public FoA mods also demonstrate:

- logical sections such as `Weight`, `FallDamage`, `ItemSorting`, `Language` and feature-specific sections;
- enum-backed choices for mode/profile selections;
- `AcceptableValueRange<T>` for bounded numeric settings;
- descriptive text that explains game-facing consequences rather than only the raw value;
- separate language/display-name settings where a mod introduces its own UI text.

## Bound values at the configuration boundary

Prefer expressing safe ranges in the config metadata when a stable range is known:

~~~csharp
new AcceptableValueRange<float>(minimum, maximum)
~~~

Still validate or clamp again at the point where a value can affect a native runtime owner. Config files can be edited manually and old configs can survive upgrades.

A config entry accepting a value is not evidence that the native game system accepts that value safely.

## User-facing versus advanced settings

FoA mods with large configuration surfaces benefit from separating ordinary player controls from diagnostics/internal tuning.

Project-proven ConfigurationManager-compatible metadata is used to:

- expose normal controls such as `Enabled`, profiles and high-level modes;
- mark diagnostic, compatibility and internal tuning entries as non-browsable;
- keep hidden entries in the raw BepInEx cfg so existing configs and manual recovery/debugging remain possible.

Hiding a setting from a config UI should not silently delete or rename its persisted key.

## Presets and raw configuration

For systems with many coupled values, a useful pattern is:

~~~text
high-level preset
├─ stable named choices
└─ RawConfig / Custom
    └─ individual advanced values
~~~

This gives ordinary users one coherent control while retaining detailed tuning when required.

When an old preset/value name changes, preserve an explicit legacy alias or migration path rather than treating the old value as invalid without explanation.

## Runtime changes are not automatic

Changing a `ConfigEntry<T>.Value` does not by itself prove that a native FoA system has consumed the new value.

Classify settings by application behavior:

| Class | Expected behavior |
| --- | --- |
| read-on-use | runtime code reads the current config value whenever the native operation occurs |
| reapply-on-change | a `SettingChanged` handler reapplies mod-owned runtime state |
| patch-registration | changing the setting may require patch install/uninstall or another explicit transition |
| structural / startup | restart or controlled reinitialization required |

Examples of state that often needs explicit reapplication include stat tweaks, cached presentation state, active audio/UI state, and dynamically registered hooks.

Document restart-required settings as restart-required. Do not present them as live just because the cfg can be edited while the game is running.

## Config-change subscriptions

When a setting owns live runtime state:

1. subscribe only for settings that can actually be reapplied safely;
2. normalize/clamp the new value;
3. update the mod-owned runtime state;
4. avoid duplicating hooks/elements/listeners;
5. unsubscribe during plugin teardown.

A `SettingChanged` callback should not become an uncontrolled second initialization path.

## Schema changes and migration

Public and internal FoA mod source demonstrates several useful migration strategies:

- maintain an internal config schema/version marker;
- back up an old cfg before destructive schema reset;
- preserve selected compatible values across a schema reset;
- accept legacy enum/profile aliases when semantics are still equivalent;
- move a legacy plugin-GUID cfg to the current name and call `Config.Reload()`;
- explicitly migrate individual defaults when a previous value is known to be obsolete.

A migration should be deterministic and idempotent. Re-running startup must not keep rewriting a value that was already migrated successfully.

## Saving and reload

Useful operations seen in FoA mods include:

- `Config.Save()` after a player-triggered setting change that should be durable;
- `Config.Save()` during controlled teardown after normalization;
- `Config.Reload()` after an intentional file migration or validation-command update.

Do not use repeated file reloads as a substitute for a runtime state model.

## Configuration is not game persistence

BepInEx configuration and FoA save state are separate owners.

Use config for:

- player preferences;
- feature toggles;
- tuning values;
- mod-owned modes/profiles;
- diagnostic settings.

Do not use config as an implicit replacement for native game-owned progression, inventory, quest or world state simply because it persists across launches.

## Compatibility rules

- Keep section/key names stable unless there is a migration.
- Do not silently reuse one old key for a new semantic meaning.
- Treat configuration-manager metadata as presentation metadata, not the underlying source of truth.
- Preserve raw cfg recoverability for advanced/hidden settings when practical.
- If two mods affect the same native system, config names do not establish ownership; the runtime intervention still needs an interoperability decision.
- Record whether a setting is live, reapplied, or restart-required.

## Evidence scope

Public patterns are drawn from public FoA mod source including `jonanoj/FallOfAvalonMods`, `apodworny/FallOfAvalonMods` and `keenanselbee/grailwright`. Stronger migration/config-UI conventions are derived from internal project evidence and published under the repository's public-safe evidence rule.

See [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
