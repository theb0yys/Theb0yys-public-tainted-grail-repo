# Tainted Gems 0.1.8 Live Deployment Validation

## Scope

Version `0.1.8` replaces the repeated effect-lane assignment with an explicit 42-entry paired effect table. This validation records the live DLL deployment and the runtime evidence available on 2026-08-16.

## Research and Process Read

- `mods/tainted-gems/docs/research.md`: 0.1.8 paired-table instruction and safe runtime channels.
- `mods/tainted-gems/docs/design.md`: 42-entry table, skill/perk active gameplay boundary, shop route, and inert trinket/generic entries.
- `mods/tainted-gems/docs/validation-plan.md`: launch, BepInEx log, shop batch, card text, and socketed-effect validation steps.
- `codex/skills/tainted-grail-foa-modding/SKILL.md`: live process control rule and DLL freshness gate.

## Build Artifact

- Built DLL: `mods/tainted-gems/src/bin/Release/netstandard2.1/TaintedGems.dll`
- Built DLL version: `0.1.8.0`
- Built DLL SHA-256: `99A8CECE9CED888FAFD74D43148BDA2BE39CF325CE275416CADF87D8407DC3D3`
- Built DLL size: `17227776` bytes

## Live Deployment

- Live target: `<local-path>`
- Previous live DLL version: `0.1.7.0`
- Previous live DLL SHA-256: `C37A022D5A823D17267349953C189921EBAF89D39FF467C4D15E079963C494E7`
- Deployed live DLL version: `0.1.8.0`
- Deployed live DLL SHA-256: `99A8CECE9CED888FAFD74D43148BDA2BE39CF325CE275416CADF87D8407DC3D3`
- Deployed live DLL size: `17227776` bytes
- Result: live deployed DLL hash matched the built repository DLL hash.

## Runtime Evidence Captured

BepInEx loaded the deployed `0.1.8` plugin:

```text
[Info   :   BepInEx] Loading [Tainted Gems 0.1.8]
[Info   :Tainted Gems] Tainted Gems 0.1.8 loaded. Enabled=True; preferEmbeddedBundle=True; embeddedResource=TaintedGems.Assets.tainted_gems.bundle; bundleFallback=assets/tainted_gems.bundle; shapeSet1=TaintedGems_DiamondShapeSet1; shapeSet2=TaintedGems_DiamondShapeSet2; placement=auto-hero-ground-generic-valuable-gem; autoPlacement=True; autoForwardDistance=2.25; autoRetrySeconds=3; hotkeyPlacement=False; hotkey=None; shopStock=True; targetShop=; quantity=1; templatesPerOpen=128; templatesPerOpenConfigured=42; allGemDefaultOverride=True; customTemplates=True; nativeFallback=True; gameplayPerks=forty-two-entry-paired-variable-table-one-positive-skill-one-negative-perk-per-two-base-gems; storeVersions=skill-perk-trinket-generic-valuable-generic-junk.
```

Observed running process during the log check:

```text
ProcessName: Fall of Avalon
Responding: True
Path: <local-path>
```

## Runtime Evidence Not Captured

The following markers were not observed during the polling window:

- `TaintedGemsCustomTemplate registered`
- `TaintedGemsCustomTemplate early registration completed`
- `TaintedGemsShopStock added`
- `TaintedGemsShopStock summary; ... candidateCount=128`

The available evidence proves live plugin load and startup configuration for `0.1.8`. It does not prove shop card display or socketed gameplay effects. Those still require a loaded save, opening an Avalon shop, and socketing representative skill/perk Tainted Gems in game.
