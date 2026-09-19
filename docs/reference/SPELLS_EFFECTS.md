# Spells, Magic, Effects, and VFX

> **Reference page.** Spell identity, gameplay effect ownership, and visual effects are separate layers.

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

## How we interact with it

### For VFX experiments

Use a short-lived, script-free, mod-owned prefab built for the correct render pipeline.

Patch the smallest cast lifecycle point and keep the effect an **overlay** until exact native ownership is proven.

### For exact spell changes

Resolve the full chain:

- exact spell item;
- effect spec;
- SkillReference;
- SkillGraph;
- concrete runtime behavior;
- VFX/audio references;
- costs/cooldowns/targeting;
- persistence/acquisition.

### Keep name classification diagnostic-only

Name fragments can help group candidates, but do not turn them into authoritative spell-family/effect mappings.

## Why this route

The spell VFX work demonstrated a key failure:

default-pipeline VFX prefabs could load but render incorrectly in FoA's HDRP environment.

It also exposed that:

- a visible overlay is not native VFX replacement;
- multiple `VCCharacterMagicVFX` components/callbacks can make "one trigger per cast" nontrivial;
- a spell template name does not prove the SkillGraph/VFX relationship.

## What goes wrong

- AssetBundle VFX loads but shader/render pipeline is wrong;
- cast hook fires multiple times for what the mod considers one cast;
- string heuristic chooses the wrong spell family;
- overlay is described as native replacement;
- new effect/SkillGraph identity is assumed registered because a prefab exists;
- summon/actor behavior is inferred from visual effect;
- acquisition/save behavior is ignored.

## How to verify

For a spell/effect feature, independently verify:

1. exact spell item identity;
2. effect/SkillGraph identity;
3. one-cast event semantics;
4. target/cost/cooldown if modified;
5. gameplay effect result;
6. VFX binding;
7. VFX renderer correctness;
8. audio if relevant;
9. cleanup;
10. save/acquisition only if claimed.

## Current proof boundary

**Source-inspected:** magic item/effect/SkillGraph/VFX architecture and a bounded `CastingBegun` overlay hook.

**Runtime/evidence caution:** current overlay examples do not prove native per-spell VFX replacement or a generic custom spell registration path.

Exact spell/effect additions require a separate proven process.
