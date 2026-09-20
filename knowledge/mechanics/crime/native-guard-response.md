---
document_type: mechanic
scope: native guard response adapters
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_CORROBORATED
  runtime: PROJECT_SPECIFIC
last_verified: 2026-09-20
---

# Native Guard Response Adapters

Crime & Consequences preserves native guard execution rather than spawning replacement actors by default.

Two distinct response shapes matter.

## Formal guard pressure

`CrimeReactionUtils.CallGuardsToHero(CrimeOwnerTemplate)`

can remain the native response adapter after the mod has established a legitimate owner/legal context.

## Already-loaded visible guard pursuit

Existing visible guards can receive a native alert point through the guard's alert stack, e.g. a bounded `NewPoi(..., Hero.Current)` route in the private implementation.

That is a **visible pursuit refresh**, not legal attribution and not omniscient tracking after visual loss.

## Rule

Keep these concerns separate:

```text
legal/authority decision
≠ spawn policy
≠ guard call
≠ already-loaded guard pursuit
≠ search after visual loss
```

The crime model supplies context. Native/Avalon AI movement owners execute behaviour.
