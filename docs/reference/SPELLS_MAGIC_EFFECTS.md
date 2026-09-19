# Spells, Magic, Skill Graphs, Projectiles, Status Effects, and VFX

> **Reference page.** Use this when changing spell costs, cast timing, projectile behavior, summon spells, status application, or visual effects.

## What this system is

FoA spell behavior crosses several layers:

~~~text
magic ItemTemplate
→ item effects / SkillReference
→ skill graph
→ magic FSM / animation-event cast lifecycle
→ projectile / persistent AoE / summon / direct effect
→ damage or status owner
→ VFX/audio presentation
~~~

Those layers should be researched independently.

A spell template name is not the whole spell.

## Who owns it in FoA

Important owners/surfaces include:

- magic `ItemTemplate`;
- item effect/spec components;
- `SkillReference`;
- `SkillInitialization`;
- character skill owners;
- magic FSM/cast state;
- projectile types;
- status templates/effect owners;
- `VCCharacterMagicVFX` for one cast-VFX lifecycle surface;
- persistent AoE owners for area effects.

## Important identities, types, and methods

Two exact native traces in the working research are especially useful.

### Wolf's Call — summon exemplar

Researched native representation:

~~~text
ItemTemplate_Magic_Tier1_SummonWolf
→ ItemActionType = CastSpell
→ Magic_Summon_Ally skill graph
→ item-effect SummonPrefab override
→ Spec_Summon_AnimalFrostWolf
→ NPCTemplate_AnimalFrostWolf_Summon
~~~

This demonstrates that the spell item, skill graph, and spawned actor identity are separate links.

### Burning Ember — projectile/status exemplar

Researched shape:

~~~text
ItemTemplate_Magic_Tier1_BurningEmber
→ ItemActionType = CastSpell
→ Magic_Projectile_Pistol
→ projectile entries
→ Projectile_OnHit_ApplyStatus
→ Status_Fire1_Burn
~~~

Projectile entries can override graph defaults such as status identity and buildup.

Other researched runtime surfaces include:

- `MagicUtils.GetManaCostMultiplier`;
- `MagicUtils.GetModifiedManaCost(...)`;
- heavy mana-cost getters;
- `MagicLightBase.OnPerformCast(...)`;
- `MagicFSM.EndCasting`;
- magic projectile velocity/range;
- `HealthElement.OnDamage` / damage subtype data;
- `PersistentAoE` lifecycle;
- `VCCharacterMagicVFX.CastingBegun`.

## Where it exists in the lifecycle

### General cast

~~~text
equipped/usable magic Item
→ Item.Use / cast action
→ item SkillReference resolved
→ native Skill initialized/executed
→ magic FSM / animation-event cast timing
→ effect owner runs
→ projectile/AoE/summon/status/damage
→ VFX/audio
→ cooldown/recovery
~~~

### Projectile spell

~~~text
cast
→ projectile created
→ native projectile movement
→ collision/hit
→ on-hit skill/effect
→ status/damage
→ projectile cleanup
~~~

### Summon spell

~~~text
cast
→ Magic_Summon_Ally
→ SummonPrefab/reference resolved
→ actor/location construction path
→ summon/ally ownership
→ actor lifecycle
~~~

The summon spell does not eliminate the need to understand actor ownership.

## How we interact with it

### Trace an exact spell before generalising

Record:

- ItemTemplate GUID/name;
- action type;
- SkillReference/graph;
- item-level overrides;
- projectile/AoE/summon references;
- status/damage output;
- acquisition/UI;
- persistence.

### Tune the narrow consumer when possible

Examples from Magic Tweaks research:

- global mana scalar → native mana-cost calculation;
- heavy-only mana behavior → heavy cost paths;
- light chain timing → existing next-cast delay after native animation event;
- recovery → native magic FSM inactive duration;
- projectile speed/range → projectile velocity only for actual projectile spells.

This avoids replacing the entire magic FSM or skill graph.

### Treat status/effect identity separately

A projectile speed change does not prove damage/status ownership.

An on-hit graph can reference a specific `StatusTemplate`; that relationship should be traced exactly.

### Treat VFX overlay as presentation

A `CastingBegun` overlay can produce useful custom visuals without proving or replacing the spell's native VFX/effect identity.

## Why this route

The spell research found that two seemingly similar magic items can route through different graphs and effect owners.

It also corrected an earlier summon comparison: the direct Wolf's Call target was a Frost Wolf summon spec, while a plain wolf spec was only comparison evidence.

That is exactly why spell relationships should be traced by exact serialized/native references rather than names.

## What goes wrong

### Template-name family heuristic treated as native spell mapping

The existing Fire/Ice/Storm/Nature/Dark/Arcane name classifier is useful discovery logic only.

It does **not** prove native VFX or effect identity.

### Projectile tuning applied to non-projectile spells

Drain, self/buff, hitscan, summon, or other spells may have no projectile velocity to scale.

### VFX hook treated as gameplay effect owner

A cast-VFX callback can be presentation-only.

### Custom spell clone treated as complete spell integration

A custom magic ItemTemplate still needs:

- registration;
- SkillReference behavior;
- acquisition;
- UI/icon/localisation;
- cast lifecycle;
- effect output;
- save/uninstall behavior.

### Skill graph rewritten broadly

Can break native animation/cast/cooldown semantics when a smaller item override or runtime stat/cost hook would suffice.

## How to verify

For a spell process:

1. exact magic ItemTemplate;
2. exact action type;
3. exact SkillReference/graph;
4. item-level overrides;
5. cast trigger;
6. animation/FSM timing;
7. effect owner;
8. projectile/AoE/summon identity if applicable;
9. status/damage output;
10. VFX/audio separately;
11. cooldown/cost;
12. acquisition/UI;
13. save/load if durable;
14. disable/uninstall behavior.

## Current proof boundary

Exact native spell traces, several cost/cast/projectile hooks, and a source-inspected VFX overlay route exist.

A generic durable **custom spell registration** process remains blocked by the same broader custom-content concerns: registration, acquisition, Babel/UI, save/uninstall behavior, compatibility, and live end-to-end validation.
