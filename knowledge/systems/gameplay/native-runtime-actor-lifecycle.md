---
document_type: system
scope: runtime Location/Npc actor creation and ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_MULTI_PROJECT_CORROBORATED
  runtime: BOUNDED_TGE_AND_COMPANION_LINEAGE
last_verified: 2026-09-20
---

# Runtime Actor Lifecycle

Use this page as the compact lifecycle map for a **native runtime NPC/creature**.

A FoA actor is more than a GameObject.

## Native chain

~~~text
reviewed LocationTemplate
→ placement verification
→ LocationTemplate.SpawnLocation / LocationCreator
→ Location
→ World registration
→ Location initialization
→ NpcElement
→ native View/visual
→ AI/faction/combat
→ death/discard/scene lifecycle
~~~

## When is the actor actually ready?

Do not treat "SpawnLocation returned" as the readiness boundary.

Stronger checks used by project implementations include:

- Location exists and is not discarded;
- expected template identity matches;
- Location/NPC initialization completed;
- expected `NpcElement` is alive where required;
- visual/View is active where the feature needs presentation.

Choose the readiness check that matches your feature.

## Temporary actors

For plugin-owned proof or temporary actors, `MarkedNotSaved = true` is a recurring guard against accidental persistence.

It does not replace cleanup.

## Cleanup

Delete by the exact `Location` reference your mod owns.

Use `Location.Discard()` rather than performing a global search by template/actor ID.

Then observe downstream cleanup rather than assuming the call returning means every View/resource is gone.

## Keep these concerns separate

~~~text
template identity
≠ placement
≠ actor ownership
≠ faction/ally role
≠ movement
≠ target selection
≠ persistence
~~~

A successful result in one category does not automatically prove the others.
