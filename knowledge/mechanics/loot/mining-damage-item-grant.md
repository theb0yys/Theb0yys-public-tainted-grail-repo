---
document_type: mechanic
scope: grant an existing item when hero pickaxe damage hits a mining candidate
runtime: mono
evidence:
  static: MULTI_PROJECT_SOURCE_AND_DECOMPILE_INSPECTED
  runtime: PROJECT_SPECIFIC_VALIDATION_PENDING
last_verified: 2026-09-20
---

# Existing-Item Grant from Mining Damage

Some mining/resource interactions flow through the native damage pipeline as `AliveLocation` targets.

A bounded source route can observe `HealthElement.TakeDamage(Damage)`, require hero-dealt Pickaxe/mining context, then use the ordinary existing-item grant path:

```text
hero damage
→ classify Pickaxe + mining candidate
→ bounded roll
→ resolve existing ItemTemplate
→ World.Add(new Item(template, quantity))
→ Hero.Current.HeroItems.Add(item)
```

## Boundary

Do not turn generic hero damage into loot.

Require exact mining/source context and keep ordinary combat targets out.

This route grants directly to inventory; it is different from adding a search/container row.
