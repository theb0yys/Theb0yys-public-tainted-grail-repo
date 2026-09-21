# Weapon Stage 7 — Persistence, Package Lifetime, and Release

## Objective

Prove the weapon survives the durability and packaging conditions being claimed.

## Procedure

1. Use a disposable save with the custom weapon acquired/equipped.
2. Save and fully exit.
3. Cold-start and verify registration occurs before saved item identity restoration.
4. Load and verify:
   - custom ItemTemplate identity resolves;
   - Item/equip state is valid;
   - presentation resources resolve;
   - no duplicate registration occurs.
5. Exercise scene transitions and repeated equip/unequip.
6. Test missing/disabled package behaviour separately if the release claims it.
7. Test migration separately when GUIDs, package identity, source archetype, or presentation schema changes.
8. Verify the release package contains only redistribution-safe source/configuration/assets the author is authorised to publish.


## Check

Use the [weapon validation matrix](../validation/README.md) before claiming the weapon pipeline complete.
