---
document_type: investigation
scope: full-body first-person feasibility
runtime: mono
evidence:
  static: DECOMPILED_PLUS_READ_ONLY_DUMPS
  mutation: BLOCKED
last_verified: 2026-09-20
---

# Body-Aware First Person

Current evidence supports **read-only investigation**, not enabling the TPP body inside FPP.

## Why

The game uses different FPP/TPP body hierarchies and broad native perspective transitions.

Weapon/body presentation also participates in:

- Kandra renderer ownership;
- equipment reload;
- animator layers;
- native weapon visibility events;
- dialogue hand visibility;
- weapon hit/trail behaviour.

## Approved investigation

A safe probe may log:

- current `Hero.TppActive`;
- active virtual camera;
- FPP/TPP parent/body instance paths;
- `HeroBodyData` transform paths;
- Kandra renderer state/bounds;
- main/off-hand item names;
- `CharacterHandBase` / `CharacterWeapon` presence.

## Blocked without further proof

- forcing perspective;
- enabling TPP renderers in FPP;
- reparenting/scaling body parts;
- mutating Kandra components;
- pushing camera states;
- changing `GameCamera`;
- changing `Hero.WeaponsVisible`;
- altering equipment/animation/combat paths.

The correct next step is comparative live probe evidence, not a “show legs” patch.
