# Build a Fixed Native Encounter

Use this guide to create a bounded encounter from one exact native enemy profile while leaving FoA in charge of the NPC, combat, damage and death.

Working lineage: [Fixed Native Wyrdspirit Encounter](../../../research/case-studies/gameplay/wyrdspirit-encounter.md).

## Runnable source

Start from the runnable public example: [Fixed native Wyrdspirit encounter](../../../examples/mono/gameplay/fixed-native-encounter/README.md). Build/run it unchanged first, confirm its documented result, then make one change at a time.

## What you will build

```text
plugin-owned trigger/threat state
→ exact approved native profile
→ native spawn request
→ native hostile/combat lifecycle
→ plugin tracks encounter
→ native death resolves encounter
→ cooldown
```

The first proven target was:

```text
Spec_EnemyMonster_T1_Wyrdspirit
```

## Step 1 — choose one exact profile

Use an exact template/profile identity.

Do not use "anything with Wyrdspirit in the name" or family matching.

One profile proof does not automatically prove another profile.

## Step 2 — keep trigger state separate from actor state

Your mod can own:

- threat/scent/condition;
- cooldown;
- encounter-active bookkeeping;
- profile selection.

FoA should own:

- NPC template;
- actor creation;
- AI;
- movement;
- combat;
- damage;
- death.

## Step 3 — request one native spawn

When your trigger becomes eligible:

1. confirm there is no active owned encounter;
2. resolve the exact approved native profile;
3. use the reviewed native spawn path;
4. record the exact spawned actor/Location identity.

Do not create a second AI stack around the spawned actor.

## Step 4 — observe native combat/death

Let the native hostile/combat path handle the fight.

The encounter controller should observe enough native state to know whether:

- actor is alive;
- actor was killed;
- actor was removed/invalidated;
- encounter should resolve.

Do not turn encounter bookkeeping into damage ownership.

## Step 5 — apply cooldown after resolution

Once native death/removal provides the accepted completion evidence:

```text
active encounter
→ resolved
→ clear owned encounter state
→ begin cooldown
→ later allow next request
```

Keep repeated spawn prevention explicit.

## Verification checklist

The proven first path exercised:

1. exact Wyrdspirit profile selected;
2. actor spawned;
3. fight/kill worked;
4. leaving and returning worked;
5. save/load worked for the tested path;
6. cooldown/encounter bookkeeping did not require replacing combat.

For any new profile, rerun its own spawn/combat/lifecycle tests.

## Evidence boundary

**Proven:** the fixed `Spec_EnemyMonster_T1_Wyrdspirit` native encounter path, including fight/kill, leave/return and tested save/load behaviour.

**Not claimed:** arbitrary enemy profiles, random population systems, custom AI or persistence semantics for every encounter design.
