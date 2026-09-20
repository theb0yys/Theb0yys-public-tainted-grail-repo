---
document_type: mechanic
scope: player-caused status buildup accumulation
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
last_verified: 2026-09-20
---

# Status Buildup Scaling

The narrow buildup route is:

`CharacterStatuses.BuildupStatus(float buildupStrength, StatusTemplate, StatusSourceInfo)`

It receives raw buildup strength before the native buildup object applies its own gain, threshold, resistance/decay and completion logic.

## Pattern

```text
incoming buildup request
→ confirm SourceCharacter == Hero.Current when player-only
→ identify BuildupStatusType
→ scale input buildupStrength
→ native BuildupStatus logic continues
```

Known buildup types include Bleed, Burn, Frenzy, Confusion, Corruption, Mute, Poison, Slow, Stun, Weak, Drunk, Intoxicated, Full and Petrification.

## Why this seam

Direct non-buildup `AddStatus` calls do not pass through this path, so ordinary instant status application remains separate.

Scaling input buildup preserves the native threshold/decay/conversion owner rather than replacing it.
