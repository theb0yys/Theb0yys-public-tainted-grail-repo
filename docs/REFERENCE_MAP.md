# Reference Map

This starter intentionally contains only a small public surface.

## Primary FoA authoring reference

### Merlin Workshop

https://github.com/theb0yys/merlin-workshop

Merlin Workshop is the Tainted Grail: The Fall of Avalon modding toolkit used as the source for the authoring-pipeline documents in this repository.

Current pinned source snapshot for the pipeline notes:

`073bdab3e09d6adad5003339fc49b021738d71e6` — 2026-02-06.

The public documents paraphrase structure and workflow. They do not copy game assets or bulk game/toolkit implementation source.

## Runtime references

### BepInEx Tainted Grail loader work

https://github.com/theb0yys/BepInEx-Tainted-Grail

Contains BepInEx-based loader/runtime work and historical FoA runtime receipts.

### BepInEx

https://github.com/BepInEx/BepInEx

Primary loader/framework upstream.

### HarmonyX

https://github.com/BepInEx/HarmonyX

Harmony patching implementation used by BepInEx ecosystems.

## Evidence rule

Each pipeline document states its evidence level.

- **STATIC_CONFIRMED** means the authoring path and contracts are directly present in the inspected Merlin Workshop source.
- **RUNTIME_PASSED** may be used only when that exact result was actually executed and recorded.
- **NOT_RUN** means no runtime claim is being made.

Source inspection is not runtime proof, and a runtime result on one build is not proof for a later build.
