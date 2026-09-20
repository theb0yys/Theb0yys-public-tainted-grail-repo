---
document_type: case
scope: arbitrary mod-owned native save domain
runtime: mono
evidence:
  static: CURRENT_BINARY_NEGATIVE_VERDICT
  runtime: NOT_RUN
last_verified: 2026-09-20
---

# Native Save-Domain Boundary

Negative evidence can change architecture.

## Question

Can a BepInEx mod register an entirely new FoA save Domain and rely on native restoration?

## Static result

For the inspected `TG.Main.dll` hash:

`749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982`

the answer was **no supported mutable registrar found**.

The save coordinator uses a fixed native domain sequence. Unknown `<name>.data` entries can be cached and emitted again, but native restoration never discovers arbitrary names and deserializes them as a new mod domain.

## Architectural correction

Do not turn passive archive round-trip into a fake “custom save domain” feature. Move the problem to a separate sidecar contract and prove that lifecycle independently.

This is an example of a blocked route producing useful public knowledge.
