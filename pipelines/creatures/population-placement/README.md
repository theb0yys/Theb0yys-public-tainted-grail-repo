# Creature Stage — Placement and Population

## Objective

Separate a safe one-session actor proof from world distribution, encounter, respawn, and population ownership.

## Procedure

1. Decide the intended consumer:
   - fixed controlled spawn;
   - encounter;
   - summon/companion;
   - ambient population;
   - quest/event-owned actor.
2. Identify the native owner for that distribution mode.
3. Define placement, clearance, duplicate prevention, respawn, density, and despawn rules.
4. Keep provider-owned creature identity separate from consumer-specific distribution logic.
5. Validate one placement mode before broadening.
6. Do not convert a session-only SpawnLocation proof into ambient population by adding random spawn calls.

## Validation gate

PASSED only for the specific distribution owner and lifecycle actually exercised.

## Does not prove

Population success does not automatically prove save persistence, migration, companion behaviour, or another placement owner.

## Next

Proceed to [persistence and cleanup](../persistence-cleanup/README.md) for any durable actor/population claim.
