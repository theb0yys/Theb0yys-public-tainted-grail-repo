---
document_type: case
scope: custom weapon equip and Drake presentation
runtime: mono
evidence:
  runtime: LOG_LEVEL_PARTIAL
last_verified: 2026-09-20
---

# Evil Greatsword: Equip and Presentation Boundary

This case is useful because it separates “weapon registered/equipped” from “all visual/lifecycle behaviour is proven”.

## Observed path

For the tested Tainted Weapons 0.3.6 runtime receipt:

```text
custom identity accepted
→ native equip redirect observed
→ framework Drake prototype built
→ registered mesh key served
→ registered material key served
```

The prototype contained the expected Drake presentation chain and no ordinary Unity renderer fallback.

## What remained unproven in that receipt

- 100-cycle equip/unequip;
- two simultaneous instances;
- native hide/show fixture;
- equipped scene-transition restore;
- full UI isolation across every relevant screen;
- screenshot/video visual acceptance.

## Lesson

A partial runtime success can still be extremely useful when its boundary is explicit. Do not promote unrun lifecycle fixtures by implication.
