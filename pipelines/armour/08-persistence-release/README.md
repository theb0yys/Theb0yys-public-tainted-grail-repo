# Armour Stage 8 — Persistence, Migration, and Release

## Objective

Establish the durable package behaviour actually being claimed.

## Procedure

1. Use a disposable save with the custom armour acquired and, where relevant, equipped.
2. Save and fully exit.
3. Cold-start with the package enabled.
4. Verify:
   - custom ItemTemplate registration ordering;
   - saved item identity restoration;
   - Kandra package/resource availability;
   - native clothes/equip reconstruction;
   - visual state after load.
5. Test disabled/missing package separately if claiming graceful behaviour.
6. Test schema/identity/package migration separately for upgrades.
7. Verify release contents are redistribution-safe and exclude proprietary/native extracted content.
8. Record exact runtime/build/package versions.


## Release gate

Do not label the pipeline complete until every claimed row has evidence and every unresolved row is explicitly scoped out.
