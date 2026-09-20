---
document_type: troubleshooting
scope: Addressables catalogue works in isolated proof but not through FoA ModService
last_verified: 2026-09-20
---

# Addressables Loads Offline but Not Through FoA ModService

Check the catalogue layout before changing the asset itself.

## Known layout issue

A generic `Addressables.RuntimePath` catalogue can pass isolated assumptions yet fail the game's ModService discovery model.

Verify:

- runtime load root uses `ModService.ModDirectoryPath/<ModFolder>/[BuildTarget]`;
- dependency schemas use the same native load root;
- bundles are under `StandaloneWindows64/`;
- catalogue JSON/hash copies also exist at the direct mod root;
- no bundle internal IDs still point at generic RuntimePath.

An offline two-cycle load/release proof is still not game deployment proof.
