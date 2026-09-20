---
document_type: system
scope: runtime Location/Npc actor creation and ownership
runtime: mono
evidence:
  static: DECOMPILED_AND_MULTI_PROJECT_CORROBORATED
  runtime: BOUNDED_TGE_AND_COMPANION_LINEAGE
last_verified: 2026-09-20
---

# Native Runtime Actor Lifecycle

Use this page when you are creating or cleaning up a runtime NPC or creature and need to know what must exist beyond the spawned GameObject.

A runtime NPC/creature in FoA is more than a spawned GameObject.

A recurring native ownership chain is:

```text
reviewed LocationTemplate
→ placement verification
→ LocationTemplate.SpawnLocation / LocationCreator
→ Location
→ World ownership
→ Location initialization
→ NpcElement
→ native visual/view
→ AI/faction/combat owners
→ death / discard / scene lifecycle
```

## Readiness matters

A useful runtime actor should not be treated as “appeared” merely because a spawn call returned.

Projects that needed stronger ownership waited for:

- non-discarded Location;
- expected template identity;
- complete location/NPC initialization;
- living NpcElement where relevant;
- active/loaded visual where relevant.

## Session-only ownership

For plugin-owned proof/temporary actors, `MarkedNotSaved=true` is a recurring defense-in-depth rule.

It does not by itself prove every save-neutrality edge case, but it clearly distinguishes temporary actor ownership from persistent world ownership.

## Cleanup authority

Delete by the **owned Location reference/handle**, not by searching for a matching native ID globally.

`Location.Discard()` is the recurring owned cleanup operation; serious validation observes downstream model/view destruction rather than assuming a return from `Discard` means cleanup is complete.

## Separate concerns

```text
template identity
≠ placement
≠ actor ownership
≠ faction/ally role
≠ movement
≠ target selection
≠ persistence
```

Each deserves its own evidence.
