---
document_type: mechanic
scope: session-only survival/fatigue pressure model built from native observations
runtime: mono
evidence:
  runtime: MULTI_STAGE_PRIVATE_VALIDATION
  persistence: INTENTIONALLY_NONE
last_verified: 2026-09-20
---

# Session-Only Fatigue Model

Tainted Survival is useful because it builds a richer survival model **without pretending FoA has a native fatigue save domain**.

## Observation sources

The project progressively used read-only or narrow native observations such as:

- rest via `RestPopupUI.SkipWeatherTime`;
- movement proficiency sources;
- player input pressure;
- hero-involved damage;
- status additions;
- weather/night readback;
- strict Wyrdness exposure;
- consumed food/item-use events.

These feed a **plugin-owned session fatigue score** and optional non-saved effects.

## State boundary

```text
native event/readback
→ project session model
→ optional project HUD/effect
→ no FoA save write
```

Safe rest can grant a session buffer; unsafe rest, movement or environmental pressure can adjust session fatigue. The model is deliberately reconstructed per session rather than hidden inside native save state.

## Do not call the project model native survival truth

FoA does not expose one universal native “fatigue” value through this evidence. The fatigue score is a mod-owned interpretation built from native facts.
