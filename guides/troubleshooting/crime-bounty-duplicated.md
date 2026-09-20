---
document_type: troubleshooting
scope: duplicate or unexpectedly multiplied crime legal application
last_verified: 2026-09-20
---

# Crime Appears to Apply More Than Once

Do not deduplicate crime by “same time + same location” unless that identity model has been deliberately designed.

## Native risks to understand

- `TemporaryBounty` is a shared pending batch, not one native incident object.
- Crime may be evaluated for more than one crime owner/jurisdiction.
- Direct/instant and deferred paths are different.
- A whole pending batch can flush from a guard-watch or timer condition.

## Diagnose

1. Capture the objective native crime entry once.
2. Record owner/jurisdiction evaluations.
3. Record witness/deferred registrations.
4. Record each `CrimeUtils.CommitCrime` / `AddBounty` owner path.
5. Distinguish repeated callbacks from genuinely separate owner applications.
6. If a mod adds its own incident model, give the incident an explicit identity and dedupe on that identity—not on loose timing guesses.

Do not “fix” duplication by bypassing native bounty storage globally.
