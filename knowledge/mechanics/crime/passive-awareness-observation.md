---
document_type: mechanic
scope: passive crime/suspicion awareness observations
runtime: mono
evidence:
  static: DECOMPILED_TARGETS_AND_ACCEPTED_DESIGN
  runtime: BOUNDED_PROJECT_LINEAGE
  persistence: NONE_SESSION_ONLY
last_verified: 2026-09-20
---

# Passive Crime Awareness Observation

FoA exposes read/observe surfaces that can feed richer crime awareness without taking over native crime execution.

Candidate native owners include:

- `TrespassingTracker`;
- `IllegalActionTracker`;
- `NpcCrimeReactions`;
- `CrimeRegion` read-only qualifiers;
- native bark/crime observation surfaces.

## Safe first-slice shape

```text
native observation/event
→ classify exact native bucket
→ create bounded in-memory observation
→ update per-suspect / per-witness memory
→ emit structured event/proof
→ no bounty, region, faction, save or AI movement mutation
```

Examples of bounded buckets include:

- watched sneak;
- suspicious observation;
- trespass warning;
- lockpick warning when native watcher evidence exists;
- pickpocket alert;
- crime witnessed;
- guard-called/reporting context;
- last-known/search facts.

## Fail closed

If witness, suspect, owner, crime type or position is not established by the native event, keep it unknown or drop the observation. Do not manufacture perfect knowledge.
