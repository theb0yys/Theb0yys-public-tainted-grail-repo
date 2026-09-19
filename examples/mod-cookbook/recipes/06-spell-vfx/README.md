# Recipe — Spell VFX Overlay

**Category:** VFX / magic presentation  
**Source-path evidence:** SOURCE_CONFIRMED  
**Public recipe:** NOT_RUN

The inspected spell-VFX path hooks:

```text
Awaken.TG.Main.Heroes.Combat.VCCharacterMagicVFX.CastingBegun
```

with a postfix.

The production source then checks that the cast belongs to `Hero.Current`, resolves a spell-family mapping, loads a mod-owned prefab, instantiates it under the native magic-VFX transform, and destroys the supplemental object after a bounded lifetime.

## Minimal architecture

1. Native cast begins.
2. Verify owner is the player.
3. Resolve **your** effect definition.
4. Instantiate a mod-owned prefab as a child of the native cast transform.
5. Set local position/rotation/scale.
6. Destroy or pool the object after its intended lifetime.

## Keep native lifecycle native

For a visual-only mod, do not replace:

- projectile movement;
- collision;
- damage;
- native hit logic;
- audio;
- pooling/destruction;

unless those are separately researched features.

## Asset rule

The public repository should contain only VFX source/assets you have the right to redistribute. A target hook does not grant rights to the game's VFX assets.

## Evidence boundary

The exact cast hook and overlay implementation exist in inspected source. This cookbook does not promote the owner's finished visual designs or asset bundle.
