---
document_type: investigation
scope: distinguishing project classification from native identity
last_verified: 2026-09-20
---

# Name Heuristic vs Native Identity

A project can classify objects by name fragments for convenience. That does not make the classification native truth.

## Example

A spell VFX implementation maps `ItemTemplate` names to elemental families, then selects mod-owned VFX prefabs.

That can be perfectly useful implementation logic while remaining **identity-risk evidence**.

## To promote a native identity claim

Prefer, in order:

1. exact stable GUID/reference;
2. exact native type/attachment/relationship;
3. serialized/runtime relationship;
4. observed native owner lookup;
5. name/display heuristic only as a candidate lead.

Keep display/localized names separate from durable IDs.
