---
document_type: mechanic
scope: positive Wealth reward scaling
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: CONFIG_GATED_PRIVATE_LANE
last_verified: 2026-09-20
---

# Positive Wealth Reward Scaling

The bounded live route targets positive `Wealth` changes observed through `SChangeCurrency.Execute` before vanilla applies the stat change.

## Allowed

- positive Wealth reward amounts only;
- configured multiplier;
- vanilla execution remains responsible for applying the result.

## Preserve

Do not touch through this mechanic:

- Cobweb;
- zero/negative Wealth sinks;
- item rewards;
- bounty penalties;
- inventory;
- crime state;
- unrelated story state.

This separation matters because “currency change” is broader than “positive Wealth reward”.
