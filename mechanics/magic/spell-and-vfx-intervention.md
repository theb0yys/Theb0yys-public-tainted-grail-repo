<!-- Canonical Wave 5 mechanic page split from docs/reference/SPELLS_EFFECTS.md. -->
# Spells, Magic, Effects, and VFX — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/magic/spells-and-effects.md) first for ownership, identities, lifecycle, and proof scope.

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

1. exact magic ItemTemplate GUID/name;
2. exact action type;
3. exact ItemEffectsSpec / SkillReference / SkillGraph;
4. item-level overrides;
5. cast trigger and animation/FSM timing;
6. target/cost/cooldown if modified;
7. projectile/AoE/summon identity where applicable;
8. gameplay damage/status/summon result;
9. VFX binding and renderer correctness;
10. audio separately;
11. cleanup;
12. acquisition/UI/localisation;
13. save/load only if durable;
14. disable/uninstall behavior.

## Evidence boundary

This split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/magic/spells-and-effects.md).
