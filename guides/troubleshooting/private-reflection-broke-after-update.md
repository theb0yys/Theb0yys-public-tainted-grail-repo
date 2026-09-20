---
document_type: troubleshooting
scope: reflected private field/property no longer resolves after update
last_verified: 2026-09-20
---

# Private Reflection Broke After an Update

Reflection against private members is patch-sensitive.

A merchant filtering implementation, for example, depends on private `Capacity` and `_compressedItems` storage.

## Diagnose

- bind the report to the exact game/assembly version;
- confirm the declaring type still exists;
- confirm the member name/backing-field shape still matches;
- check whether the owner changed representation rather than only renaming the member;
- fail closed if the expected private contract is absent.

Do not silently fall back to a “similar-looking” field. Re-establish the owner and semantics.
