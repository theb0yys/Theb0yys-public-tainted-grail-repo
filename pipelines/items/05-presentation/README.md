# Item Stage 5 — Assets and Presentation

## Objective

Layer icons, models, localisation, and item-specific presentation onto an already working registered identity without confusing asset success with item-system success.

## Rule

Keep presentation as a separate evidence lane.

A bundle or icon can load while registration is broken. A registered item can exist while its icon/model is missing. Diagnose those as different failures.

## Procedure

1. Start from the PASSED registered/acquired item from Stage 4.
2. Add one presentation dimension at a time:
   - display/localisation;
   - icon;
   - world/pickup presentation;
   - optional equipped presentation where the item family requires it.
3. Verify each asset through the repository's supported asset-loading route.
4. Record the exact asset identity/address and lifetime owner.
5. Confirm the custom ItemTemplate points to the intended presentation without mutating the native source.
6. Re-run acquisition and UI presentation after each change.

## Validation gate

PASSED when the same registered custom identity reaches the expected presentation owner and the asset is loaded/released through the correct lifetime.

## Does not prove

Presentation success does not prove gameplay effects, persistence, missing-package behaviour, or compatibility with other item families.

## Next

Proceed to [persistence and compatibility](../06-persistence/README.md) only if the item is intended to survive save/load or ship as a durable package.
