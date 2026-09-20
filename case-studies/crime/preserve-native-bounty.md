---
document_type: case
scope: Crime & Consequences reporting/attribution redesign
evidence:
  static: CURRENT_BINARY_PLUS_PROJECT_DESIGN
last_verified: 2026-09-20
---

# Preserve Native Bounty, Extend Semantic Truth

The crime research found a mismatch:

Vanilla has real crime/witness/bounty execution, but its deferred `TemporaryBounty` substrate is a shared batch rather than a complete report/case/attribution simulation.

The project response was **not** to replace bounty.

Instead:

- preserve native crime entry and owner evaluation;
- preserve native witness reactions and pending reporting;
- preserve `CrimeUtils.AddBounty` as legal storage;
- add explicit project-side incident/witness/report/case/attribution semantics around that native path;
- keep the first phase passive/session-only.

## Lesson

A proprietary game system can be both **authoritative** and **semantically incomplete for your mod's goals**. Extend its missing semantic layer without duplicating its authoritative state.
