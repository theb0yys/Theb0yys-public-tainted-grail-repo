<!-- Canonical Wave 5 native-system page split from docs/reference/SPELLS_EFFECTS.md. -->
# Spells, Magic, Effects, and VFX

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/magic/spell-and-vfx-intervention.md).

## What this system is

Current research exposes a chain roughly like:

~~~text
magic ItemTemplate
→ item effects / SkillReference
→ SkillGraph / runtime skill behavior
→ cast lifecycle
→ gameplay effects/projectiles/summons
→ VFX/audio presentation
~~~

A VFX hook does not prove the spell's gameplay graph or native visual ownership.

## Who owns it in FoA

Important researched owners include:

- `ItemTemplate_Magic_*` families
- `ItemEffectsSpec`
- `SkillReference`
- `SkillGraph`
- `ShareableARAssetReference`
- `PrefabPool`
- `MagicVFXWrapper`
- `VCCharacterMagicVFX`
- concrete spell/effect behavior owners
- status/effect systems

## Important identities, types, and methods

Evidence-backed surfaces:

- `VCCharacterMagicVFX.CastingBegun`
- native VFX lifecycle signals such as begun/cancelled/failed/ended
- `ShareableARAssetReference` VFX references
- exact magic item-template GUID/name
- exact SkillGraph GUID/identity where resolved

The working spell research found dozens of non-story `ItemTemplate_Magic_*` rows, but a complete item→SkillGraph→VFX join was not automatically present in the basic dump.

## Exact native spell traces

Two researched spells show why the whole chain must be traced instead of classifying spells by name.

### Wolf's Call — summon exemplar

~~~text
ItemTemplate_Magic_Tier1_SummonWolf
→ ItemActionType = CastSpell
→ Magic_Summon_Ally SkillGraph
→ item-effect SummonPrefab override
→ Spec_Summon_AnimalFrostWolf
→ NPCTemplate_AnimalFrostWolf_Summon
~~~

The spell item, SkillGraph, summon-spec identity and spawned actor template are separate links.

A prior comparison used a plain wolf spec as contextual evidence; exact serialized tracing corrected the direct Wolf's Call target to the **Frost Wolf** summon spec. That correction is why comparison/name evidence must not be promoted into an exact relationship.

### Burning Ember — projectile/status exemplar

~~~text
ItemTemplate_Magic_Tier1_BurningEmber
→ ItemActionType = CastSpell
→ Magic_Projectile_Pistol
→ projectile entries
→ Projectile_OnHit_ApplyStatus
→ Status_Fire1_Burn
~~~

Projectile entries can override graph defaults such as the status template and buildup values.

### Other researched runtime surfaces

- `MagicUtils.GetManaCostMultiplier`
- `MagicUtils.GetModifiedManaCost(...)`
- heavy mana-cost getters
- `MagicLightBase.OnPerformCast(...)`
- `MagicFSM.OnPerformCast` / `MagicFSM.EndCasting`
- projectile velocity/range owners
- `HealthElement.OnDamage`
- persistent AoE owners
- `CharacterStatuses.BuildupStatus(...)`

These surfaces support bounded tuning and diagnostics; they do not by themselves establish a durable custom-spell registration path.

## Where it exists in the lifecycle

A cast-facing VFX overlay can observe:

~~~text
native casting begins
→ VCCharacterMagicVFX.CastingBegun
→ verify player ownership / selected spell context
→ attach mod-owned short-lived VFX overlay
→ clean up after bounded lifetime
~~~

This proves an overlay route only.

Native replacement requires identifying the exact VFX reference owner inside the spell/SkillGraph/effect path.

## Current proof boundary

**Source-inspected:** magic item/effect/SkillGraph/VFX architecture and a bounded `CastingBegun` overlay hook.

**Runtime/evidence caution:** current overlay examples do not prove native per-spell VFX replacement or a generic custom spell registration path.
