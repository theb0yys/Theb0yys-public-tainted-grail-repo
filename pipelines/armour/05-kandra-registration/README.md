# Armour Stage 5 — Kandra Runtime Registration

## Objective

Prove that the generated Kandra candidate can enter the game’s Kandra runtime ownership path.

## Native ownership

Public research identifies KandraRenderer, KandraRendererManager, KandraMesh, KandraRig, and the associated streaming/skinning/material managers as runtime owners. Normal registration is tied to KandraRenderer lifecycle and manager-side finalisation.

## Procedure

1. Run package/metadata preflight before invoking live registration.
2. Create only the minimum approved registration candidate.
3. Invoke the guarded host/runtime registration path.
4. Confirm the runtime manager recognises the renderer/resource identity.
5. Record manager/registration receipts rather than treating absence of an exception as success.
6. Perform the same-mesh decode/A-B validation where the current evidence lane requires it.
7. Release/cleanup the proof candidate through the expected runtime lifetime.

## Validation gate

PASSED when the intended candidate is registered and recognised by the Kandra runtime owner and the registration receipt corresponds to the exact package/geometry under test.

## Critical boundary

A registered Kandra proof renderer is not yet a custom armour item.

## Next

Proceed to [item identity and native clothes/equip](../06-item-native-clothes/README.md).
