# Spells, Magic, and Effects

Use this page when you want to change **spell costs, casting, projectiles, summons, status application, or spell VFX**.

The key rule is:

> Spell identity, gameplay behavior, and visual effects are separate layers.

A VFX hook firing does not tell you which SkillGraph or gameplay effect actually owns the spell.

## Native spell chain

A useful model is:

~~~text
magic ItemTemplate
→ ItemEffectsSpec / SkillReference
→ SkillGraph
→ cast lifecycle
→ projectile / summon / status / other gameplay effect
→ VFX and audio
~~~

## Important types and surfaces

Useful researched pieces include:

- `ItemTemplate_Magic_*` families;
- `ItemEffectsSpec`;
- `SkillReference`;
- `SkillGraph`;
- `ShareableARAssetReference`;
- `PrefabPool`;
- `MagicVFXWrapper`;
- `VCCharacterMagicVFX`;
- `VCCharacterMagicVFX.CastingBegun`;
- `MagicUtils.GetManaCostMultiplier`;
- `MagicUtils.GetModifiedManaCost(...)`;
- `MagicLightBase.OnPerformCast(...)`;
- `MagicFSM.OnPerformCast`;
- `MagicFSM.EndCasting`;
- projectile velocity/range owners;
- `CharacterStatuses.BuildupStatus(...)`.

## Trace the exact spell, not the name

### Wolf's Call example

~~~text
ItemTemplate_Magic_Tier1_SummonWolf
→ CastSpell
→ Magic_Summon_Ally SkillGraph
→ SummonPrefab override
→ Spec_Summon_AnimalFrostWolf
→ NPCTemplate_AnimalFrostWolf_Summon
~~~

The item, SkillGraph, summon spec, and spawned actor are separate identities.

Earlier contextual comparison pointed toward a plain wolf; exact serialized tracing corrected the real target to the Frost Wolf summon spec.

That is why names and visual similarity are not enough.

### Burning Ember example

~~~text
ItemTemplate_Magic_Tier1_BurningEmber
→ CastSpell
→ Magic_Projectile_Pistol
→ projectile entries
→ Projectile_OnHit_ApplyStatus
→ Status_Fire1_Burn
~~~

Projectile entries can override defaults such as status/buildup values.

## VFX overlay vs native replacement

A bounded overlay route is:

~~~text
native casting begins
→ VCCharacterMagicVFX.CastingBegun
→ verify player/spell context
→ spawn short-lived mod-owned VFX
→ clean it up
~~~

That proves an overlay only.

To replace native spell VFX, identify the exact VFX reference owner in the item/SkillGraph/effect path.

## Keep VFX compatible with FoA rendering

One failure from spell-VFX work was loading effects authored for the wrong render pipeline.

A prefab can load successfully and still render incorrectly in FoA's HDRP environment.

Use script-free, correctly built mod assets for early VFX proofs.

## Exact spell changes need the full chain

For a gameplay change, resolve:

- exact magic item;
- action type;
- effect spec;
- `SkillReference`;
- `SkillGraph`;
- item-level overrides;
- concrete runtime effect;
- costs/cooldowns/targeting;
- projectile/AoE/summon/status identity;
- VFX/audio;
- acquisition/persistence if relevant.

## Common mistakes

- classifying spells only by template-name fragments;
- calling a VFX overlay a native VFX replacement;
- treating one cast callback as exactly one cast without checking duplicate components/callbacks;
- assuming a prefab means a new SkillGraph/effect is registered;
- inferring summon gameplay from its visual;
- ignoring acquisition/save behavior for a new spell.

## How to verify

Check:

1. exact magic `ItemTemplate`;
2. action type;
3. exact effect spec / `SkillReference` / `SkillGraph`;
4. item-level overrides;
5. cast/FSM timing;
6. cost/cooldown/targeting if changed;
7. projectile/AoE/summon/status identity;
8. actual gameplay result;
9. VFX binding/rendering;
10. audio separately;
11. cleanup;
12. acquisition/UI/localisation;
13. save/load only if durable;
14. disable/uninstall behavior.

## Evidence limits

The item/effect/SkillGraph/VFX architecture and a bounded `CastingBegun` overlay route are source-inspected.

There is not yet a generic public process for durable custom spell registration or universal native per-spell VFX replacement.
