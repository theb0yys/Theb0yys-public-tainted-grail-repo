---
document_type: mechanic
scope: observe native proficiency XP source events
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: MULTIPLE_PRIVATE_CAPTURE_ROWS
last_verified: 2026-09-20
---

# Proficiency Event Observation

`ProficiencyEventListener.XPGainEvent` is a useful observation/router surface for native proficiency activity.

Private progression and survival research captured confirmed source categories including examples such as:

- weapon damage;
- blocked damage / shield use;
- sprint;
- fast swim;
- dash;
- jump;
- fall damage;
- sneak activity;
- cooking;
- magic/summon damage.

## Use as observation first

A safe diagnostic path records:

```text
native XP event
→ exact proficiency/source
→ amount/context
→ optional project practice ledger
→ no change to vanilla XP unless separately authorized
```

## Do not infer missing proficiencies

If a player-facing talent group has no confirmed native XP source, keep it as a project branch/overlay concept rather than fabricating a hidden proficiency.
