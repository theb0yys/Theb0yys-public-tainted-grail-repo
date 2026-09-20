# Case Packet — Runtime Success while Persistence Remains Unproven

## Reader symptom

> The mod works in the current session, but there is no evidence that the state survives save/load, restart, missing-mod, migration or uninstall scenarios.

## Canonical owners

- runtime system/mechanic owner;
- native save/load owner;
- stable identity/registration timing;
- any mod-owned persistence service;
- post-load apply/readiness owner.

## Evidence-backed case history

The public custom-item path provides a bounded current-session example:

- a custom item identity can be registered;
- a native `Item` can be created;
- a controlled acquisition owner can receive it;
- downstream UI/gameplay can observe it.

That does not establish cold-save restoration.

Reviewed persistence research separately establishes:

- item/template restoration is identity-sensitive;
- a saved template GUID must be resolvable when restoration requires it;
- the current binary did not expose a supported mutable arbitrary mod save-domain registrar;
- a sidecar design remains a candidate transaction model, with capture, native success correlation, staging and post-load apply as separate states;
- runtime/save validation for that generic sidecar contract was explicitly not run.

## What this case proves

- runtime evidence and persistence evidence answer different questions;
- current-session success can be genuine while durable-state claims remain blocked;
- “save requested” is not automatically “durable write succeeded”;
- post-load apply requires an explicit readiness point;
- when persistence is not required, session-only/non-saved state is often safer than accidental save ownership.

## What this case does not prove

- that custom items are unsafe to save in every configuration;
- that the candidate sidecar architecture is implemented or validated;
- missing-mod/uninstall/migration behaviour for all content.

## Canonical dependencies

- [Saving and Persistence](../../../systems/world/saving-persistence.md)
- [Custom Item Integration](../../../mechanics/items/register-custom-template.md)
- [Evidence status](../../../reference/evidence/README.md)

## Public outputs

- case study: `examples/failures-and-corrections/runtime-success-persistence-unproven.md`
- diagnosis: `diagnose/persistence/works-now-not-after-load.md`

## Evidence status

- runtime example: bounded current-session evidence;
- persistence architecture: static/research evidence only for the generic sidecar direction;
- public rewrite: documentation-only;
- cold-save/missing-mod/compatibility/release: not inherited.
