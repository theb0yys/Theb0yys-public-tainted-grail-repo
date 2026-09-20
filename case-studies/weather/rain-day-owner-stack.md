---
document_type: case
scope: Weather → Skybox → Water bounded live integration
evidence:
  runtime: PASS_FOR_RAIN_DAY_GREENSHALLOWS
  visual: PARTIAL
last_verified: 2026-09-20
---

# Rain / Day Owner-Stack Validation

This case demonstrates why an inter-mod stack should validate **owner boundaries**, not only visible output.

The accepted live run showed:

- Tainted Weather publishing `Rain` truth and external consumer requests without directly mutating sky;
- Tainted Skybox selecting/applying its sky and owning Unity skybox mutation;
- Immersive Water accepting the Weather plan and applying `GreenShallows` to water surfaces;
- Weather remaining the semantic source.

A later same-session screenshot supported the active sky/weather presentation.

## Limits

Water was not visible in that accepted screenshot, and the run does not establish all weather families, time buckets, presets, restore paths or performance.

The useful proof is the **bounded ownership handshake**.
