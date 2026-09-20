---
document_type: investigation
scope: evidence-lane separation
last_verified: 2026-09-20
---

# Static/Source Evidence vs Runtime Evidence

Use the evidence lane that fits the claim.

## Static/source inspection can establish

- type/member existence;
- call paths;
- candidate owners;
- fixed/static data contracts;
- source implementation shape;
- negative structural facts in the inspected artifact.

## It cannot by itself establish

- that a patch loads;
- that the target executes in the installed build;
- that the downstream effect is visible/correct;
- save safety;
- compatibility;
- release readiness.

## Runtime evidence can establish

what happened in one recorded environment.

It still does not automatically establish persistence, broad compatibility or release quality.

See [Evidence reference](../reference/evidence/README.md).
