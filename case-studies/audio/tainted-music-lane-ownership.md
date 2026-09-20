---
document_type: case
scope: Tainted Music contextual lane and native-audio coexistence
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: PARTIAL
last_verified: 2026-09-20
---

# Tainted Music: Own Your Lane, Not All Audio

Tainted Music deliberately separates:

- plugin-owned background music;
- native exploration/alert/combat music;
- native ambience zones;
- dialogue/SFX/UI audio.

That made it possible to suppress only the overlapping owner while keeping other game audio intact.

## Lessons

- route using read-only game context;
- classify unsupported place categories through configurable policy, not invented native taxonomy;
- duck only plugin-owned channels for dialogue/combat;
- make suppression exceptions explicit;
- restore native ambience ownership when scoped suppression ends;
- fail open to native audio when private suppression targets disappear.

The private matrix still contained audible-validation gaps, so the public mechanic keeps that state visible.
