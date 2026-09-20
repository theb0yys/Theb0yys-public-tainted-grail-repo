---
document_type: case
scope: Smart Save Backups native save lifecycle observation
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
last_verified: 2026-09-20
---

# Native Save Completion Observer

Smart Save Backups needed to react after native save-provider activity without inventing a custom save slot or rewriting FoA serialization.

The implementation patches concrete `CloudService.EndSave(string)` methods and receives the exact slot ID.

## Lesson

A useful observation seam can be valuable without being a mutation API.

The same evidence does **not** prove:

- arbitrary save-domain registration;
- custom serialization;
- durable-success semantics for every provider/failure case;
- restoration timing.

That separation directly informed the later sidecar-persistence research.
