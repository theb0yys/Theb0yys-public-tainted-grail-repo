# Tainted Grail Mod Cookbook

This folder answers a different question from the minimal templates:

> What does a small **real Tainted Grail mod** look like for the kinds of things people actually want to change?

The examples here use real FoA Mono symbols where the maintainer's working mod workspace provides enough evidence to identify the path. They are rewritten as small clean-room teaching examples; they do not copy the finished mod's design.

## Using this cookbook on IL2CPP

If your installed game is IL2CPP, start with **[First IL2CPP Game Change](../il2cpp-first-game-change/README.md)** before using the game-target examples below.

The code examples in this cookbook are primarily **Mono/BepInEx 5 target investigations**. A type or method shown here can be a useful research lead for IL2CPP, but it is **not automatically proof that the same generated IL2CPP type, signature, or patch shape is correct on your current build**.

For an IL2CPP game-specific patch:

1. inspect your local generated interop;
2. verify the exact current type and method;
3. reference only the local interop assembly you need;
4. adapt the example;
5. build and test that exact IL2CPP version.

## Testing status

Every example keeps two things separate: how far the underlying source path was checked in the maintainer's working environment, and whether the exact rewritten public example has itself been run.

The table below uses the repository's formal status labels so those distinctions stay precise. See [Testing and Evidence Status](../../docs/EVIDENCE.md) for the definitions.

The exact rewritten public examples are **NOT_RUN** until somebody builds and tests those specific examples.

## Code examples

| Example | Category | Source-path evidence |
| --- | --- | --- |
| [01 Stamina drain](01-stamina-drain-mono/README.md) | player stats / stamina | SOURCE_BUILD_EVIDENCED |
| [02 Carry capacity](02-carry-capacity-mono/README.md) | player stats / inventory | LOAD_EVIDENCED |
| [03 No fall damage](03-no-fall-damage-mono/README.md) | damage | SOURCE_BUILD_EVIDENCED |
| [04 Magic projectile speed](04-magic-projectile-speed-mono/README.md) | magic / projectiles | RUNTIME_EVIDENCED |
| [05 Force hero HUD bars visible](05-force-hero-hud-mono/README.md) | UI / HUD | RUNTIME_EVIDENCED |
| [06 Modifier-gated illegal pickup](06-illegal-pickup-guard-mono/README.md) | interaction / theft | RUNTIME_EVIDENCED |
| [07 Hero footstep beep replacement](07-footstep-beep-replacement-mono/README.md) | audio / event replacement | RUNTIME_EVIDENCED |
| [08 One extra airborne jump](08-extra-air-jump-mono/README.md) | movement | LOAD_EVIDENCED |
| [09 Player magic mana cost](09-mana-cost-mono/README.md) | magic / mana | SOURCE_BUILD_EVIDENCED |
| [10 Player magic damage](10-magic-damage-mono/README.md) | combat / magic | SOURCE_BUILD_EVIDENCED |
| [11 Merchant gold floor](11-merchant-gold-floor-mono/README.md) | economy / merchant wealth | SOURCE_CONFIRMED |
| [12 Restock on shop open](12-merchant-restock-on-open-mono/README.md) | economy / merchant stock | SOURCE_BUILD_EVIDENCED |
| [13 Status buildup](13-status-buildup-mono/README.md) | statuses | SOURCE_BUILD_EVIDENCED |
| [14 Save-slot observer](14-save-slot-observer-mono/README.md) | persistence observation | LOAD_EVIDENCED |
| [15 Dialogue-choice observer](15-dialogue-choice-observer-mono/README.md) | dialogue observation | LOAD_EVIDENCED |
| [16 Quest-completion observer](16-quest-completion-observer-mono/README.md) | quest observation | LOAD_EVIDENCED |
| [17 Movement FOV kick](17-fov-kick-mono/README.md) | camera / comfort | LOAD_EVIDENCED |
| [18 Personal helper light](18-personal-helper-light-mono/README.md) | environment / lighting | SOURCE_BUILD_EVIDENCED |
| [19 Context lane observer](19-context-lane-observer-mono/README.md) | contextual routing | RUNTIME_EVIDENCED for selected inputs |
| [20 Camera shake strength](20-camera-shake-strength-mono/README.md) | camera / comfort | LOAD_EVIDENCED |
| [21 FOV transition duration](21-fov-transition-duration-mono/README.md) | camera / comfort | SOURCE_BUILD_EVIDENCED |
| [22 Native music mute](22-native-music-mute-mono/README.md) | audio / native music | LOAD_EVIDENCED |
| [23 Character damage observer](23-character-damage-observer-mono/README.md) | damage / VFX foundation | RUNTIME_EVIDENCED |

Also see [proven-path mechanism templates](../proven-paths/README.md) for generic patching, UI, audio-gating and skybox ownership shapes.

## Content-authoring examples

These use the Merlin Workshop authoring path rather than a BepInEx code plug-in:

- [Item stats](content/01-item-stats/README.md)
- [Weapon](content/02-weapon/README.md)
- [Armour](content/03-armour/README.md)
- [Creature / NPC](content/04-creature/README.md)

Their current evidence level is **STATIC_CONFIRMED** because the authoring contracts exist in inspected Merlin source. The exact public recipes remain **NOT_RUN**.

## Larger-system recipes and scaffolds

These are intentionally not all compileable one-file mods. They document the smallest honest route for systems where a tiny snippet would hide important lifecycle, persistence or evidence requirements.

- [Skill caps and progression](recipes/01-skill-caps/README.md)
- [Held-light / helper-light mods](recipes/02-helper-light/README.md)
- [Contextual music routing](recipes/03-contextual-music/README.md)
- [Loot and corpse loot](recipes/04-loot/README.md)
- [Crafting and runtime recipe prototypes](recipes/05-crafting/README.md)
- [Spell VFX overlays](recipes/06-spell-vfx/README.md)
- [Safe save-backup architecture](recipes/07-save-backup/README.md)
- [Dialogue and quest mutation boundary](recipes/08-dialogue-quest-mutation/README.md)
- [Combat VFX sidecars](recipes/09-combat-vfx/README.md)

For the full category/testing map, see [CATEGORY_INDEX.md](CATEGORY_INDEX.md).

## Category map

### Fundamentals
- configuration and hotkeys: level 00/01 beginner guides;
- Harmony postfix/prefix: proven-path templates;
- diagnostics/logging: all examples;
- small runtime UI: proven-path runtime UI example.

### Player stats
- stamina drain: cookbook example 01;
- carry capacity: cookbook example 02;
- skill caps/uncapping: maintainer path has load evidence, but the real XP/cap path is too invasive to compress into a beginner snippet without reimplementing native XP handling; not promoted yet;
- movement/traversal: extra-air-jump example 08; camera-relative movement remains a later advanced example;
- camera comfort: examples 17, 20 and 21 cover movement FOV kick, camera shake and FOV transition duration.

### Damage and combat
- fall damage: example 03;
- magic projectile speed: example 04;
- item/weapon combat stats: content example 01/02;
- general damage/status examples now include 10 (magic damage), 13 (status buildup) and 23 (character damage observation);
- combat VFX lifecycle guidance lives under recipes/09-combat-vfx/.

### Items, equipment and creatures
- item stats: content example 01;
- weapon authoring: content example 02;
- armour authoring: content example 03;
- creature/NPC authoring: content example 04;
- custom runtime weapon registration/presentation remains outside the beginner cookbook until its full live registration/equip path is proved.

### Interaction and economy
- illegal pickup guard: example 06;
- merchant restock/gold paths exist, but current feature evidence is not strong enough for a "proven" public recipe;
- lockpicking has source/build evidence but not game-feature evidence, so it is not promoted yet.

### UI
- simple overlay: proven-path UI example;
- hero HUD decision patch: example 05;
- complex native menu extension is deliberately excluded until a smaller complete focus/input/close lifecycle can be published without dragging in a finished mod's design.

### Audio
- direct replacement gate: proven-path audio example;
- real FoA hero footstep target: example 07;
- contextual routing starts at example 19;
- example 22 demonstrates the separate native-music-start suppression seam without shipping replacement music;
- the complete contextual music system remains intentionally larger than a single cookbook mod.

### Environment and visuals
- skybox apply/restore: proven-path skybox example;
- personal/helper lighting: example 18;
- torch detection and weather systems still contain wider visual/runtime matrices and remain larger recipes.

### Persistence
- a save-backup plug-in reached build/load/config/folder creation, but actual backup archive creation remained unproved in the inspected evidence. It is intentionally not presented as a working save example yet.

## Build assumption

These game-target examples are for the **Mono / BepInEx 5** lane because that is where these particular source paths were investigated.

Typical build:

~~~powershell
dotnet build .\Example.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Do not install a Mono example into an IL2CPP game setup.

## Rule for adapting an example

1. Get the example running unchanged where practical.
2. Check the testing-status label.
3. Change one behavior.
4. Rebuild and retest.
5. If you change the target type/method/field, treat that as new research.
6. Never turn a source/build example into a runtime claim without running it.
