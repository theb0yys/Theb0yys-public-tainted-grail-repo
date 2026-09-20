# Example Extraction Map

Status: **routing/provenance index, not implementation authority**

Last inspected: **2026-09-20**

This map routes reusable knowledge from the private engineering corpus into the public `examples/` taxonomy. It records inspected source families and a primary extraction domain. It does **not** publish private production source, prove runtime behaviour, or turn a private working mod into validation for a newly authored public example.

## Public taxonomy

```text
examples/
  mono/<domain>/<mechanism>/
  il2cpp/<domain>/<mechanism>/
  merlin/
  hybrid/
```

Runtime is expressed by the parent directory. Child folders do not repeat `mono` or `il2cpp`, and numeric ordering prefixes are not used.

A new domain folder is created only when at least one public-safe example exists.

## Direct IL2CPP implementation surfaces verified in the private repository

These rows require a committed IL2CPP project/source surface, not merely a compatibility note or research mention.

| Primary extraction domain | Private owner | Inspected implementation locator |
| --- | --- | --- |
| Audio | `tainted-music` | `mods/tainted-music/src-il2cpp/TaintedMusic.IL2CPP.csproj` |
| Presentation / backgrounds | `immersive-backgrounds` | `mods/immersive-backgrounds/src-il2cpp/ImmersiveBackgrounds.IL2CPP.csproj` |
| Diagnostics | `avalon-exceptions` | `mods/avalon-exceptions/src-il2cpp/AvalonExceptions.IL2CPP.csproj` |
| Performance | `tainted-performance` | `mods/tainted-performance/src-il2cpp/TaintedPerformance.IL2CPP.csproj` |
| Tooling | `foa-mod-manager` | `mods/foa-mod-manager/src-il2cpp/FoAModManager.IL2CPP.csproj` |
| Framework host | `tainted-framework` | `mods/tainted-framework/src/Tainted.Host.IL2CPP/Tainted.Host.IL2CPP.csproj` |
| Economy | `rich-merchant` | `mods/rich-merchant/src/RichMerchant.csproj` with an explicit `Il2Cpp` build branch |

`Immersive UI` surfaced in IL2CPP research evidence during this pass, but no separate committed IL2CPP project root was counted from that evidence alone.

This table is a verified committed-source set, **not a claim about the total number of IL2CPP mods in every local/private workspace**.

## Mono implementation source families

The following owners surfaced through direct `BaseUnityPlugin` source matches in the current private repository. The domain labels below are extraction routing labels; multi-system mods may legitimately feed more than one public domain.

### Frameworks, tooling, diagnostics

- `avalon-contracts`
- `avalon-core`
- `avalon-exceptions`
- `foa-mod-manager`
- `hello-avalon`
- `smart-save-backups`
- `Tainted-Diagnostic Tool`
- `tainted-performance`
- `template-diagnostics`

### UI, HUD, menus

- `always-show-hud`
- `avalon-cheat-panel`
- `better-bonfire-menu`
- `Immersive UI`
- `multi-pin-map-notes`
- `Tainted Interface`

### Combat and weapons

- `base-weapon-trails`
- `dragon-knight`
- `evil-greatsword-moveset`
- `last-breath`
- `no-fall-damage`
- `realistic-longsword`
- `stamina-action-control`
- `Tainted Combat`
- `tainted-weapons`

### Audio

- `creature-combat-sfx-proof`
- `immersive-footsteps`
- `tainted-music`
- `weapons-magic-sfx`
- `world-action-sfx`

### Items, economy, storage, crime

- `Avalon Stash`
- `carry-weight-tweaks`
- `crime-and-consequences`
- `food-drink-asset-proof`
- `hold-to-steal`
- `lockpicking-reforged`
- `merchant-stock-tweaks`
- `missing-pebbles`
- `rich-merchant`
- `tainted-gems`
- `tainted-lockpick`
- `TaintedEconomy`

### Magic and effects

- `avalon-spell-vfx`
- `magic-tweaks`
- `tainted-blood`
- `weapons-magic-sfx`
- `wyrd-decoy`

### World, rendering, environment

- `brighter-torches`
- `immersive-backgrounds`
- `immersive-water`
- `no-map-fog`
- `tainted-skybox`
- `views-of-avalon`
- `world-texture-replacements`

### Movement, camera, travel

- `avalon-flight`
- `avalon-mounts`
- `dungeon-exit-helper`
- `first-person-plus`
- `jump-higher`
- `true-third-person`
- `wyrdflight`

### Actors, companions, NPCs

- `avalon-bear-companion`
- `avalon-companions`
- `avalon-human-companions`
- `avalon-summons`
- `avalon-wolf-companion`
- `bandit-outlaw`
- `dragon-knight-companion`
- `living-avalon`
- `tainted-npcs`

### Progression and survival

- `avalon-progression-core`
- `easy-avalon`
- `immersive-progression`
- `tainted-survival`
- `theB0yysSkillUncapper`

### Story, quests, behaviour, multi-system

- `avalon-awakened`
- `do-not-use-unless-you-want-pain`
- `origins-of-avalon`
- `tainted-bugfixes`
- `tainted-instincts`
- `wyrd-hunt`
- `wyrd-whispers`

## Extraction rule

When a private mod yields a reusable public example:

1. select one bounded mechanism, not the whole private mod;
2. determine the runtime from inspected source/build evidence;
3. choose the functional domain;
4. publish under `examples/<runtime>/<domain>/<mechanism>/`;
5. use a concise PascalCase project filename matching the mechanism;
6. write the public example independently and keep private production source private unless publication of that exact code is explicitly authorised;
7. state the public example's own evidence status;
8. update this map and the publication ledger.

A source-family route is not a technical claim and does not grant runtime, persistence, compatibility, or release proof.
