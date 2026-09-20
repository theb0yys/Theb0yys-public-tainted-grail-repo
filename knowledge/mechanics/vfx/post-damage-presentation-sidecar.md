---
document_type: mechanic
scope: add presentation after native character damage/death
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: REPRESENTATIVE_TAINTED_BLOOD_LINEAGE
  save: NONE
last_verified: 2026-09-20
---

# Post-Damage / Death Presentation Sidecar

For blood or similar visual presentation, keep native damage truth authoritative and attach visuals **after** the native lifecycle reaches the relevant point.

## Living hit

A narrow route observes `HealthElement.OnDamage` after native damage and requires a character target before adding a mod-owned visual.

## Terminal death

`HealthElement.OnDeathEvents` is a better terminal owner for one death visual/pool because it participates in normal damage deaths, direct kill and finisher death paths.

## Rules

- do not modify `Damage`;
- do not skip native damage;
- require the intended target class;
- use bounded global/per-target instance and rate caps;
- own the lifetime/cleanup of custom presentation;
- keep resource/mining/non-character paths out through explicit target guards.

This pattern generalizes beyond blood: presentation should follow the gameplay owner rather than intercepting earlier targeting merely because the needed collider is visible there.
