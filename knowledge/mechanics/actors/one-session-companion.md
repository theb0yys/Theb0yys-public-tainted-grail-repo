---
document_type: mechanic
scope: one-session native creature companion
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PROVEN_BY_REPRESENTATIVE_IMPLEMENTATIONS
  persistence: EXPLICITLY_NOT_SAVED
last_verified: 2026-09-20
---

# One-Session Native Companion

The reusable one-session companion pattern keeps FoA's native actor and ally systems in charge while explicitly refusing persistence.

## Path

```text
reviewed non-unique LocationTemplate
→ native SpawnLocation
→ immediately MarkedNotSaved = true
→ validate Location / NpcElement
→ apply native ally/summon ownership
→ attach NpcHeroPetAlly where appropriate
→ track only the managed actor(s) the mod owns
→ use native follow/defend/lifecycle entry points
→ dismiss/failure: discard owned Location and clear mod state
```

## Death and teardown

A mature implementation does not treat “dead” as merely zero health. It waits for the expected native location/NPC death/corpse transition before accepting terminal death state.

Dismissal or invalid-state cleanup should:

- keep the actor non-persistent;
- remove mod-owned runtime surfaces;
- discard the owned Location;
- clear local references/state.

## Boundary

This pattern is deliberately **one-session**. It does not authorize:

- saved companion ownership;
- reload restoration;
- auto-respawn;
- re-adoption after reload;
- arbitrary actor conversion.

See [Creatures/NPCs](../../systems/gameplay/creatures-npcs.md).
