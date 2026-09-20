---
document_type: mechanic
scope: Addressables content layout for FoA ModService discovery
runtime: unity-authoring
evidence:
  build: OFFLINE_TWO_CYCLE_PROOF
  game_runtime: NOT_RUN_IN_CITED_PROOF
last_verified: 2026-09-20
---

# ModService Addressables Catalogue Layout

A private offline proof corrected an Addressables layout from generic `Addressables.RuntimePath` to FoA's native ModService root.

## Proven offline layout

Runtime load root:

```text
{Awaken.TG.Assets.Modding.ModService.ModDirectoryPath}/<ModFolder>/[BuildTarget]
```

Bundles remain under:

`StandaloneWindows64/`

and native discovery requires catalogue JSON/hash copies at the **direct mod root** as well.

## What the proof established

For the isolated Goblin visual catalogue proof:

- 3 bundle internal IDs used the ModService root;
- 0 still used generic Addressables RuntimePath;
- direct-root catalogue/hash copies matched the platform copies;
- two non-instantiating load/inspect/release cycles produced identical fingerprints.

## What it did not establish

- game deployment;
- actor construction;
- combat/death/corpse behaviour;
- save exclusion;
- world population.

This is an **authoring/layout mechanic**, not a live actor-integration proof.
