---
document_type: framework
scope: TGE owned encounter service
runtime: mono
evidence:
  managed: PASSED
  installed_game_native_lifecycle: PASSED_BOUNDED
  human_visual_acceptance: NOT_RUN
  save_compatibility: NOT_PROVEN
last_verified: 2026-09-20
---

# TGE Owned Encounter Service

TGE's encounter service is an example of a **project-owned execution service layered over native actor ownership**.

## Safety model

- exact build fingerprint gate;
- allowlisted template key/name/GUID combinations;
- preview and spawn are separate;
- preview produces unpredictable plan ID + fingerprint;
- execution rejects stale world/player/placement changes;
- at most one spawn per update;
- explicit owned-actor handles;
- native IDs are observation only, never delete authority;
- exact owned `Location` is discarded for cleanup;
- `MarkedNotSaved` is defense in depth;
- lifecycle status is asynchronous: pending → appeared → removing → removed/failed.

## Native path

```text
SDK authenticated adapter
→ encounter service
→ reviewed LocationTemplate
→ native placement verification
→ LocationCreator / World.Add path
→ tracked Location + NpcElement + visual
→ later exact owned Location.Discard
→ model/view destruction observation
```

## Live evidence

A bounded rerun on the pinned Mono build observed:

- one Wyrdspirit initialized/alive/visually active then exact model/view destruction;
- a saved **composition definition** expanded to two Wyrdspirits, both appeared and were removed;
- distinct handles/native-ID digests retained through cleanup.

This does **not** establish human visual acceptance, arbitrary NPC safety, save persistence or cross-build compatibility.

Saved encounter compositions are inert data (name + supported template/count entries), not executable scripts.
