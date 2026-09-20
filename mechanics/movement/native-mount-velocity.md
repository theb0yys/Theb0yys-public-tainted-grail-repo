---
document_type: mechanic
scope: native horse running and turning velocity
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PROVEN_ON_TESTED_STACK
  persistence: SMOKE_ONLY
  compatibility: SMOKE_ONLY
last_verified: 2026-09-20
known_limits:
  - does not prove custom mount ownership
  - does not prove recall, armour, stamina or marker ownership
---

# Native Mount Velocity

For simple native-horse speed tuning, patch the values the native mount controller already consumes.

## Proven seam

- `VMount.RunningVelocity`
- `VMount.TurningVelocity`

The tested implementation used postfixes on those getters and multiplied the returned values.

```text
native VMount calculates velocity
→ getter returns native value
→ mod adjusts returned value
→ native mount controller consumes it
```

## Evidence

A live run proved the `Proof125` profile at **1.25 running / 1.25 turning** with both getter paths active. A later user-confirmed smoke covered mount, movement, transition, save/load and quit/relaunch without Avalon Mounts errors on that stack.

## Do not generalise this into

- mount ownership;
- spawn/recall;
- armour;
- stamina;
- marker/map ownership;
- custom creature mount contracts;
- persistence implementation.

Those remained separate research surfaces.

This is a good example of changing one scalar without replacing the movement controller.
