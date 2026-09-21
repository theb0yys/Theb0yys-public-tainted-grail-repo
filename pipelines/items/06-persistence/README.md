# Item Stage 6 — Persistence and Compatibility

## Objective

Prove durable behaviour separately from current-session runtime success.

## Required questions

- What identity is serialized for the custom item?
- At what point during load must the custom template be registered?
- What happens when the mod/package is disabled or missing?
- What happens when a GUID/name changes between releases?
- How are collisions with another package detected?
- Is migration required?
- Which runtime/build is being claimed?

## Procedure

1. Use a disposable save.
2. Register and acquire the custom item through the already-PASSED stages.
3. Save with the custom item present.
4. Fully exit.
5. Cold-start and verify template registration occurs before the saved identity is resolved.
6. Load and confirm the same custom identity and relevant item state.
7. Test the documented missing-package/disabled-mod condition separately.
8. Test upgrade/migration separately when identity or schema changes.
9. Record the exact game build, runtime lane, mod version, and result.

## Validation gate

Do not mark persistence PASSED from a same-session reload or from the existence of a serialization field. Use an actual cold save/load observation.

## Current public boundary

The generic custom-item pipeline does not currently claim universal missing-mod, uninstall/orphan, migration, or cross-mod collision safety. Treat those as PARTIAL or NOT_RUN unless concrete evidence exists for the target package.

## Next

Use the [validation matrix](../validation/README.md) before release.
