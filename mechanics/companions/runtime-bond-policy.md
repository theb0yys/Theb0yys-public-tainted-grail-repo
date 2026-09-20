---
document_type: mechanic
scope: project-owned runtime trust/loyalty policy affecting companion command timing only
runtime: mono
evidence:
  source: PROJECT_POLICY
  native_truth: NOT_NATIVE_FOA_STAT
  persistence: RUNTIME_ONLY_IN_CITED_STAGE
last_verified: 2026-09-20
---

# Runtime Companion Bond Policy

A trust/loyalty/bond model can remain **project-owned policy** while still influencing bounded companion behaviour.

The private companion policy allowed only command-timing/threshold modifiers such as:

- catch-up interval;
- catch-up distance threshold;
- defend-prompt cooldown;
- whether automatic native defend assist is allowed outside explicit Defend;
- trust gain/penalty rates.

It explicitly did **not** write:

- native buffs/stats;
- target overrides;
- movement states;
- forced hostility;
- saved actor ownership.

## Useful design rule

The effective bond level was the lower of trust and loyalty, preventing one score from masking a weak other dimension.

This is an example of safe project semantics layered around native actor ownership—not evidence that FoA has native “bond levels”.
