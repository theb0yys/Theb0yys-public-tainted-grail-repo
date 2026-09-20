---
document_type: investigation
scope: owner discovery
last_verified: 2026-09-20
---

# Finding the Native Owner

A common FoA failure mode is searching the object that *looks* relevant instead of the system that owns the behaviour.

## Procedure

1. Define the exact subject and outcome.
2. Record what currently works and what fails.
3. List candidate owners.
4. Inspect call sites, services, elements, views and lifecycle transitions.
5. Prefer read-only observation before mutation.
6. Reject candidates that are only presentation, authoring inputs or transient wrappers.
7. Select the smallest owner whose normal lifecycle reaches the desired downstream behaviour.
8. Only then choose a hook or mutation.

## Mount lesson

Early mount work showed why “I cannot find the capability on this object” is not proof that the game lacks the capability. Ownership had to be reconstructed across mount, hero movement and related components before a narrow velocity seam could be selected.

## Output

An owner investigation should end with:

- exact owner;
- readiness condition;
- entry point;
- downstream consumer;
- cleanup/restoration;
- evidence state;
- remaining unknowns.
