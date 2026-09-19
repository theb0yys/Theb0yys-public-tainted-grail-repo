# Case Study — Runtime Success while Persistence Is Still Unproven

Document type: **case study**.

## Symptom / risk

A modded feature works correctly in the current session.

It is tempting to conclude:

> “The feature is done.”

That conclusion is too broad when save/load has not been tested.

## Concrete example

The bounded custom-item route can prove:

```text
custom identity registered
→ provider resolves it
→ native Item created
→ acquisition owner receives it
→ UI/gameplay sees it
```

That is real runtime success.

It still does not prove:

```text
save
→ process exit
→ restart
→ custom identity available at restore time
→ saved object restored correctly
→ missing-mod / migration / uninstall behaviour
```

## Why persistence is a different owner problem

Persistence adds new contracts:

- stable durable identity;
- native save timing;
- restore timing;
- registration readiness before restore;
- missing dependency behaviour;
- duplicate/replay handling;
- migration;
- crash/partial-write handling for mod-owned state.

A current-session runtime object does not exercise those contracts.

## Important save-system finding

Current save-system research did not find a supported generic mutable native save-domain registrar for arbitrary mod-owned state.

A sidecar architecture is therefore a **candidate design**, not a proven public implementation.

The candidate itself needs separate states:

```text
capture
→ stage
→ native-save success correlation
→ atomic sidecar commit

load selection
→ stage sidecar
→ native restore
→ post-load readiness
→ apply once
```

Static source/binary evidence can identify candidate save/load milestones.

It cannot replace controlled throwaway-save runtime validation.

## Reusable lesson

When a feature works now:

```text
record runtime success honestly
→ ask whether the feature is intended to be durable
→ if no: make session-only behaviour explicit
→ if yes: open a separate persistence proof lane
```

Do not weaken a valid runtime PASS merely because persistence is unknown.

Also do not strengthen it into a persistence PASS.

## Diagnostic handoff

Use [Works now but not after load](../../diagnose/persistence/works-now-not-after-load.md).

## Canonical system/mechanic

- [Saving and Persistence](../../systems/persistence/README.md)
- [Custom Item Integration](../../mechanics/items/custom-item-integration.md)

## Evidence status

Underlying runtime example: **bounded current-session proof**.

Generic sidecar persistence direction: **static/research evidence; runtime/save validation not completed**.

This public rewrite: **documentation-only**.
