---
document_type: mechanic
scope: ask native ally companion to defend against hero's existing attacker
runtime: mono
evidence:
  static: DECOMPILED_AND_SOURCE_INSPECTED
  runtime: PRIVATE_COMPANION_LINEAGE
last_verified: 2026-09-20
---

# Native Defend Assist

The narrow defend-assist route does **not** select an arbitrary nearby target.

## Native relationship

- `Hero.Current.PossibleAttackers` exposes current attackers.
- `NpcHeroPetAlly.EnterCombat()` enters the native ally combat path.
- Native `NpcAlly` / `NpcHeroSummon` logic remains responsible for selecting from the hero's attacker relationship.

## Pattern

```text
explicit Defend command or bounded automatic assist
→ confirm live hero attacker exists
→ confirm actor is a managed native ally
→ call NpcHeroPetAlly.EnterCombat()
→ native ally target/combat owner continues
```

## Boundary

This does not authorize:

- custom nearby-hostile scans;
- arbitrary attack commands;
- forced faction hostility;
- target overrides;
- wild-actor conversion.

Use the relationship the game already owns.
