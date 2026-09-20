---
document_type: mechanic
scope: post-roll removal/scaling of generated SearchAction rows
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  persistence: SAVE_BACKED_RUNTIME_ROWS
last_verified: 2026-09-20
---

# Post-Roll Container Rules

A post-roll rule operates **after vanilla has generated container rows**.

It can:

- remove matching generated rows;
- scale quantities on matching generated rows;
- preserve protected rows;
- use stable container/item identity tokens to select policy.

## It cannot increase a probability that never rolled

If no treasure row exists, multiplying generated treasure quantity does not create a treasure roll.

That distinction is crucial:

```text
loot-table probability editing
≠ generated-row filtering
≠ generated-row quantity scaling
```

The private implementation uses deterministic row rolls for keep/remove policy so repeated checks do not re-roll the same container instance arbitrarily.
