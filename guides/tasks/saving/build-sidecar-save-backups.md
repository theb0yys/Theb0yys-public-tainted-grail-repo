# Build Sidecar Save Backups

**Evidence status: PARTIAL.** The safer architecture is established, but the cited private validation had not yet proven actual backup archive creation.

Working lineage: [Smart Backups Without More Save Slots](../../../research/case-studies/persistence/smart-backup-boundary.md).  
Observation guide: [Observe Native Save Completion](observe-native-save-completion.md).

## Goal

Add redundancy without changing FoA's native slot count or save-domain model.

## Architecture

```text
observe native save/slot identity
→ read/copy existing native save through supported provider/read surface
→ write mod-owned archive outside native slots
→ retain metadata/hash
→ restore only through an explicit separate tool/process
```

## Why sidecar

The user wants backup redundancy, not a new native save system.

Keeping archives outside native slot rotation preserves:

- native UI;
- native slot policy;
- native serialization ownership.

## Process to prove

1. observe exact completed/native save slot;
2. obtain bytes/file through the reviewed provider/read route;
3. write a timestamped/versioned backup in a mod-owned directory;
4. hash/verify the backup;
5. enforce retention policy;
6. test restore on disposable data separately.

## Do not claim yet

- backup created successfully until bytes/hash are verified;
- restore works until performed on a disposable copy;
- every cloud/provider backend behaves identically.

## Verification

Require:

- exact source slot;
- non-empty backup;
- checksum;
- repeated-save naming/retention;
- failure leaves native save untouched;
- restore procedure tested independently.

## Current proof boundary

Architecture and boundary are established. Actual archive creation/restore still require explicit runtime validation before release claims.
