# Template Source-Family Map

This map records which inspected mod families informed each reusable public starter. It is provenance/routing, not a claim that the public template inherits private runtime validation.

| Public template | Source families / reusable mechanism |
| --- | --- |
| `mods/*` | 31 named cross-runtime mod-family starters; each now contains feature-specific portable shared logic plus Mono + IL2CPP hosts |
| `mono/harmony/basic` | common BepInEx 5 + Harmony lifecycle used across Mono mods |
| `mono/harmony/result-postfix` | narrow result overrides used by gameplay/config mods |
| `mono/harmony/action-guard` | Hold to Steal / guarded native action pattern |
| `mono/audio/replacement-gate` | Immersive Footsteps, world-action SFX, weapons/magic SFX replacement safety |
| `mono/combat/damage-observer` | Tainted Combat, Tainted Blood, creature combat SFX observation |
| `mono/combat/death-observer` | death-presentation / cleanup sidecar pattern |
| `mono/items/illegal-pickup-guard` | Hold to Steal illegal-interaction guard |
| `mono/magic/projectile-speed` | Magic Tweaks projectile tuning |
| `mono/rendering/skybox-ownership` | Tainted Skybox runtime ownership/restore pattern |
| `mono/ui/runtime-overlay` | diagnostic/tool overlays used across UI and diagnostics mods |
| `il2cpp/harmony/*` | BepInEx 6 IL2CPP host + Harmony lifecycle |
| `il2cpp/ui/runtime-overlay` | Tainted Performance / Tainted Music thin host + registered behaviour shape |
| `il2cpp/diagnostics/frame-sampler` | Tainted Performance read-only sampling pattern |
| `il2cpp/audio/replacement-gate` | Tainted Music plus replacement-audio fail-open invariant |
| `hybrid/tainted-framework-consumer` | Rich Merchant conditional runtime build; Avalon Exceptions, Immersive Backgrounds, Tainted Music, Tainted Performance and FoA Mod Manager shared-source/thin-host architecture |
| `hybrid/mono-merlin` | separate Merlin authoring and runtime integration lifecycles |

The named `mods/*` starters cover these source families:

`avalon-ai-runtime`, `avalon-cheat-panel`, `Avalon Stash`, `carry-weight-tweaks`, `dungeon-exit-helper`, `easy-avalon`, `foa-mod-manager`, `hold-to-steal`, `immersive-backgrounds`, `immersive-footsteps`, `always-show-hud`, `immersive-progression`, `jump-higher`, `lockpicking-reforged`, `magic-tweaks`, `multi-pin-map-notes`, `no-fall-damage`, `origins-of-avalon`, `rich-merchant`, `stamina-action-control`, `Tainted Combat`, `Tainted Interface`, `tainted-performance`, `wyrd-hunt`, `avalon-companions`, `tainted-music`, `Tainted-Diagnostic Tool`, `better-bonfire-menu`, `merchant-stock-tweaks`, `avalon-core`, and `avalon-human-companions`.

Private production implementation is not copied wholesale. The named starters extract portable feature rules such as guards, scaling, bounded state, readiness gates, ownership decisions and sampling logic.
