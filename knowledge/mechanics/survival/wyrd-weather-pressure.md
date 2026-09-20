---
document_type: mechanic
scope: derive session-only pressure from native weather/night/Wyrdness facts
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: REPRESENTATIVE_PRIVATE_HEARTBEAT_VALIDATION
last_verified: 2026-09-20
---

# Weather / Night / Wyrdness Pressure

A survival overlay can consume **native environmental facts** without owning those systems.

Relevant readback families include:

- game-time/night state;
- weather/precipitation readback;
- scene Wyrdnight capability;
- `Hero.Current.IsSafeFromWyrdness`;
- `HeroWyrdNight.IsHeroInWyrdness`;
- repeller context where required.

## Pattern

```text
native weather/Wyrdness truth
→ project policy computes session pressure
→ session fatigue/effect
→ no weather/Wyrdness mutation
```

The pressure value is mod policy. The underlying weather/Wyrdness state remains game-owned.

Do not describe project fatigue gain as a native environmental damage mechanic.
