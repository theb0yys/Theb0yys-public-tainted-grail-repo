# Build a One-Session Native Companion

Use this guide to create a temporary companion for the current session while keeping FoA's native NPC, ally and combat systems in charge.

Working lineage: [One-Session Native Companion Lifecycle](../../../research/case-studies/companions/native-companion-lifecycle.md).  
Canonical actor model: [Creatures and NPCs](../../../knowledge/systems/gameplay/creatures-npcs.md).

## Runnable source

Start from the runnable public example: [One-session native companion](../../../examples/mono/gameplay/native-session-companion/README.md). Build/run it unchanged first, confirm its documented result, then make one change at a time.

## What you will build

```text
explicit player command
→ resolve one reviewed non-unique LocationTemplate
→ native SpawnLocation
→ MarkedNotSaved = true
→ require valid NpcElement
→ apply native hero-summon/ally ownership
→ attach NpcHeroPetAlly
→ track one managed companion
→ use native follow/defend routes
→ dismiss via owned native cleanup
```

This guide is intentionally **session-only**.

## Step 1 — choose one reviewed non-unique actor

Start with one known-safe template.

Do not turn this into "recruit any NPC".

Exclude:

- named/story NPCs;
- quest actors;
- unique templates;
- actors whose component/ally contract is unknown.

## Step 2 — make recruitment explicit

Use an explicit player action/command to create the companion.

That gives you a clean ownership start point and avoids accidental world-wide conversion.

## Step 3 — spawn natively

Resolve the reviewed `LocationTemplate` and use the native spawn path.

Immediately set:

```csharp
location.MarkedNotSaved = true;
```

Persistence is out of scope for this guide.

## Step 4 — validate the NPC element

Require a valid `NpcElement`.

If actor construction does not produce the expected native NPC contract, fail closed and clean up the owned Location.

## Step 5 — apply the native ally route

Use the native hero-summon/ally ownership path and attach `NpcHeroPetAlly`.

Do not build a parallel faction/targeting controller when the native ally system already owns that behaviour.

## Step 6 — keep a bounded roster

The proven pattern tracks at most one active managed roster actor for this simple lane.

Useful invariants:

- managed companion remains `MarkedNotSaved=true`;
- duplicate/excess managed roster actors are discarded;
- untracked session allies from the same owned route can be cleaned;
- recall/recovery reasserts the not-saved posture.

## Step 7 — use native defend behaviour

When the hero has a live attacker, the working route uses:

```text
NpcHeroPetAlly.EnterCombat()
```

That hands combat targeting back to native systems.

Do not invent a second target-selection AI for the first version.

## Step 8 — dismiss through owned cleanup

Dismiss the exact companion your mod owns.

Use native Location/model cleanup rather than destroying only the visible GameObject.

## Verification checklist

1. explicit command creates one companion;
2. reviewed template identity is exact;
3. companion Location is `MarkedNotSaved`;
4. valid `NpcElement` exists;
5. native ally state is active;
6. follow behaviour remains native;
7. defend handoff works against a live attacker;
8. duplicate/excess companion creation is bounded;
9. dismiss removes the owned companion;
10. save/reload does **not** become an undocumented persistence feature.

## Evidence boundary

**Proven:** one-session managed creature companion path using native spawn, ally ownership, `NpcHeroPetAlly`, not-saved posture, defend handoff and bounded cleanup.

**Not claimed:** persistent rosters, saved companion state, arbitrary NPC recruitment, custom pathing, custom target selection, dialogue, equipment or levelling.
