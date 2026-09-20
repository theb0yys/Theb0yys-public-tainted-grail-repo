---
document_type: mechanic
scope: suppress active ManualAudioZone ambience in configured scenes
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: PARTIAL
last_verified: 2026-09-20
---

# Scoped Ambient-Zone Suppression

A repetitive ambient loop is not the same owner as native music.

The researched asylum route:

```text
configured scary-place scene
+ plugin scary-place music active
→ enumerate ManualAudioZone
→ select only currently active zones
→ unregister only _ambientsToRegister as AudioType.Ambient
→ later re-register still-active zones when suppression ends
```

This leaves native music, alert/combat music, snapshots, dialogue, UI and SFX alone.

## Fail closed

If `AudioCore`, `ManualAudioZone` or the zone-active private field cannot be resolved, leave native ambience untouched.

The private validation still required a live audible asylum test for the cited version.
