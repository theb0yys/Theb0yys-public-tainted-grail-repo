# Observe Native Save Completion

**Evidence status: PARTIAL.** A concrete post-save observation seam exists, but it does not prove durable success semantics for every provider or custom restoration behaviour.

Working lineage: [Native Save Completion Observer](../../../research/case-studies/persistence/native-save-completion-observer.md).

## Goal

React after native save-provider activity without adding a new native save slot/domain or rewriting FoA serialization.

## Known observation seam

The researched implementation patches concrete:

```text
CloudService.EndSave(string slotId)
```

and receives the exact slot ID.

## Process

```text
native save flow
→ CloudService.EndSave(slotId)
→ mod observes slot identity
→ enqueue bounded post-save sidecar/backup action
```

Keep observation and mutation separate.

## Do not infer

This hook does **not** prove:

- arbitrary save-domain registration;
- custom native serialization;
- that every provider failure path reached durable storage;
- restoration timing;
- sidecar correctness.

## Verification

For your build/provider:

- hook target resolves;
- expected slot ID received;
- no duplicate observation;
- failed/cancelled save semantics are understood;
- post-save work is bounded;
- observer does not block/corrupt native save flow.

## Current proof boundary

The observation seam is established. Durable-provider semantics and any sidecar restore contract remain separate work.
