---
document_type: investigation
scope: extending native crime reporting without replacing native legal truth
evidence:
  static: CURRENT_BINARY_PLUS_PROJECT_DESIGN
  runtime: PASSIVE_SHADOW_MODEL_ONLY
last_verified: 2026-09-20
---

# Crime Reporting and Attribution Investigation

Native FoA provides crime events, witnesses, deferred reporting and owner-specific bounty, but it does not expose all semantic distinctions a deeper crime simulation may need.

A project-side model can therefore **extend around native truth** instead of replacing it.

## Useful semantic split

```text
objective incident
→ witness knowledge
→ report attempt
→ delivery receipt
→ authority case
→ legal attribution
→ owner-specific native application
→ native-application receipt
```

This model distinguishes things vanilla's shared pending batch does not explicitly represent.

## Important invariants

- One objective act should not become several mod incidents because multiple callbacks fire.
- A report attempt is not a delivered report.
- Unknown-offender cases should not create player-specific bounty.
- Witness belief is not legal attribution.
- Native bounty remains canonical storage.
- Multiple witnesses may strengthen one case but should not duplicate the offence.
- AI packages may execute behaviour but should not rewrite crime/legal truth.

## Current maturity

The accepted Crime & Consequences direction begins as a **session-only passive shadow model**.

Persistence, suppression/replacement of native reporting, and authoritative mod-owned legal application require separate proof.
