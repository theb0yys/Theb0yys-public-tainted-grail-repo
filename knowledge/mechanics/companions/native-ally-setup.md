---
document_type: mechanic
scope: make a plugin-owned temporary actor use native hero-ally ownership
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: CREATURE_AND_HUMAN_PROOF_LINEAGE
  persistence: NONE
last_verified: 2026-09-20
---

# Native Ally Setup for a Temporary Companion

For a plugin-owned one-session actor, the recurring native ally route is:

```text
spawn reviewed non-unique LocationTemplate
→ MarkedNotSaved=true
→ require live NpcElement
→ OverrideFaction(Hero summon faction, Summon context)
→ add NpcHeroPetAlly(Hero.Current)
→ track only in plugin memory
```

If native ally setup fails, discard the owned actor.

## Human proof boundary

A disabled-by-default human proof reused the same native summon-faction + `NpcHeroPetAlly` path on a reviewed non-unique outlaw template.

That was a **proof actor**, not permission to convert existing civilians/named/quest NPCs into companions.

## Do not add custom combat AI first

If a native ally-marked actor still behaves incorrectly, inspect humanoid/creature-specific faction/combat surfaces before inventing a parallel AI/targeting system.
