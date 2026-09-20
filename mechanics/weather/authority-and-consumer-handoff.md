---
document_type: mechanic
scope: weather truth to sky/water consumer handoff
runtime: mono
evidence:
  runtime: BOUNDED_LIVE_INTEGRATION_PASS
last_verified: 2026-09-20
---

# Weather Authority and Consumer Handoff

A modular environment stack works best when one service owns semantic weather truth and specialist consumers own their presentation domains.

## Path

```text
weather authority selects state
→ publishes sky request + water plan
→ sky consumer resolves contextual sky asset and mutates Unity skybox
→ water consumer resolves preset and mutates water surfaces
→ each consumer reports its own result
```

## Why this boundary matters

Weather should not silently become:

- skybox renderer owner;
- water-surface owner;
- terrain owner.

Likewise, Skybox/Water should not invent weather truth independently when consuming a Weather-owned plan.

## Proof boundary

The private live pass proves one integrated `Rain / Day / GreenShallows` slice and same-session sky/weather visual evidence. Water visual quality and broader matrix coverage remain separate.
