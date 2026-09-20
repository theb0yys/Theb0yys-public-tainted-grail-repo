---
document_type: reference
scope: Merlin/native quest objective marker authoring contract
evidence:
  official_source: MERLIN_WORKSHOP_SOURCE
last_verified: 2026-09-20
---

# Quest Objective Marker Contract

Official Merlin Workshop source shows that quest-marker authoring belongs to the **objective**, not merely to an NPC's generic marker asset.

## Objective marker inputs

`QuestTemplateBase.ObjectiveSpecs` contains objective specs.

`ObjectiveSpecBase.GetMarkersData()` builds marker data from fields including:

- target location reference;
- target scene;
- optional related Story flag / visibility condition;
- additional markers.

Individual objective specs also carry their own serialized GUID.

## Important separation

```text
NPC generic map/compass marker
≠
quest objective marker
```

A generic NPC marker asset does not prove that a quest objective points at that NPC, that location, or that scene.

For a marker repair you need the **exact objective's authored marker data** or current runtime objective/marker evidence.
