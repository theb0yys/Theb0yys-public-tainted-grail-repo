---
document_type: system
scope: native witnessed-crime reporting and bounty application
runtime: mono
evidence:
  static: CURRENT_BINARY_DECOMPILATION
  runtime: NOT_RUN_FOR_THIS_STATIC_PACKET
last_verified: 2026-09-20
source_artifact_sha256: 749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982
---

# Native Crime Reporting and Bounty Application

The inspected FoA build does **not** expose a rich incident/report/case model before bounty.

Its deferred witnessed-crime path uses one hero-owned `TemporaryBounty` object as a shared batch.

## Native shape

```text
CrimeUtils.TryCommitCrime
→ owner/jurisdiction evaluation
→ CrimeUtils.InformWatchingNPCs
   ├─ no relevant witness
   │    → no noticed legal application for that owner
   │
   ├─ guard witness / InstantReport
   │    → witness ReactToCrime
   │    → CrimeUtils.CommitCrime(ref crime, exactOwner)
   │    → CrimeUtils.AddBounty(exactOwner)
   │
   └─ non-guard witnesses
        → TemporaryBounty.GetOrCreate
        → TemporaryBounty.RegisterCrime
        → witness reactions
        → shared pending batch
        → later ApplyCrimes
        → crime replay with InstantReport / visibility-watcher bypass
        → CrimeUtils.CommitCrime
        → CrimeUtils.AddBounty
```

## Shared-batch semantics

The inspected `TemporaryBounty` contains one shared timer and pending collections. It does not expose native IDs for incident, report, authority case or delivery receipt.

A later crime can therefore share the same pending expiry window as an earlier registration.

## Known flush paths

Static evidence identified:

- timer expiry;
- qualifying guard watcher arrival;
- penalty/payment preparation.

`GuardApplyCrimes` has no incident/report/owner argument; it applies the shared pending batch.

## Ownership rule

Native FoA owns:

- crime entry and owner/jurisdiction evaluation;
- witness reactions;
- deferred pending crime substrate;
- owner-specific `CrimeUtils.AddBounty`;
- native bounty storage/clearing;
- guard intervention and search states.

A mod may add richer semantic records around this path, but should not create a second bounty truth.
