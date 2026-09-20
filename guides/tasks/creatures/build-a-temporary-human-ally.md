# Build a Temporary Human Ally

**Evidence status: PARTIAL.** One reviewed temporary human-ally class is scoped; this is not a generic NPC recruitment system.

Working lineage: [Human Native-Ally Proof](../../../research/case-studies/companions/native-human-ally-proof.md).

## Goal

Spawn one reviewed non-unique human template and apply the same temporary native ally ownership pattern used by the proven session companion route.

## Process

1. Select one reviewed repetitive/non-unique human `LocationTemplate`.
2. Spawn through the native Location path.
3. Immediately set `MarkedNotSaved=true`.
4. Require the expected `NpcElement`.
5. Apply native summon/ally faction ownership.
6. attach/use `NpcHeroPetAlly`;
7. track the exact owned actor;
8. dismiss/discard through native cleanup.

## Explicitly out of scope

Do not extend this guide into:

- converting existing world NPCs;
- named/story/quest NPCs;
- persistence;
- saved recruitment roster;
- equipment/levelling;
- dialogue mutation;
- custom pathing;
- custom target selection.

## Verification

For the one reviewed human type, prove:

- exact template identity;
- spawn;
- ally state;
- follow/defend;
- not-saved posture;
- cleanup;
- no duplicate/untracked actor.

## Current proof boundary

The case proves the safe scope/model for one temporary human class. Broader "human companion" claims remain blocked until each additional owner/lifecycle is proven.
