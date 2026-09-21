# Weapon Importer RH3 Independent Review Handoff — 2026-08-16

Status: `RH3_REQUIRED`

This handoff does not perform or claim RH3. It identifies the exact independent-review target created by the current reconciliation task.

## Review target

- Process under review: `documents/frameworks/weapon-importer/weapon-importer-end-to-end-process-2026-08-16.md`
- RH1/RH2 reconciliation: `documents/frameworks/weapon-importer/weapon-importer-research-governance-reconciliation-2026-08-16.md`
- Machine review record: `documents/research/reviews/weapon-importer-research-governance-reconciliation-2026-08-16.review.json`
- Governing evidence standard: `documents/process/research-standard/evidence-hierarchy.md`
- Governing review standard: `documents/process/research-standard/review-hierarchy.md`
- Baseline reviewed by reconciliation: `main@43fb8639e40c78f40eb232c9a09956b643fef395`

## Required RH3 independence

The independent reviewer must not be the generating/evaluating agent for the reconciliation. The reviewer must inspect the underlying sources rather than only this handoff or the reconciliation prose, and must record disagreements rather than silently resolving them.

## Required challenge set

The reviewer must independently test at least these points:

1. Whether the old weapon-importer E0-E8 taxonomy has been correctly demoted to historical/E1 context under the repository-wide E0-E7 standard.
2. Whether package-v1 current-source claims are no broader than the exact `TaintedWeaponPackageImporter.cs` implementation.
3. Whether native registrar claims are limited to source/static behaviour and do not imply current runtime success, persistence, rollback, or compatibility.
4. Whether Evil Greatsword is correctly classified as a migration fixture with retained parallel ownership surfaces rather than proof of one-system/one-truth completion.
5. Whether the user-supplied `TG.Main(3).dll` evidence is bounded to its fingerprint and does not substitute for an admitted installed-game tuple.
6. Whether exact Drake ownership/lifetime remains blocked pending the matching `Awaken.ECS.dll` and later runtime proof.
7. Whether startup ordering, save reconstruction, persistence, missing-package handling, migration, compatibility, and performance remain below action adequacy.
8. Whether the proposed package-v2/lifecycle architecture is correctly treated as planning/design context until human promotion rather than current executable truth.
9. Whether any material claim lacks an underlying source, overstates source independence, or crosses evidence-domain lanes without separate proof.
10. Whether the reconciliation grants any implementation, runtime-mutation, save-write, compatibility, performance, release, or public-current permission that the evidence does not support.

## Allowed RH3 outcomes

- `APPROVED_FOR_HUMAN_PROMOTION`
- `CHANGES_REQUESTED`
- `BLOCKED_EVIDENCE`
- `REJECTED`

RH3 does not itself grant implementation, runtime, save, compatibility, performance, release, or current-authority permission. RH5 remains a separate named-human decision.

## Downstream stop

Steps 3-15 of the user-requested sequence remain `NOT_RUN` until RH3 is completed with the required independence and any requested corrections are resolved.
