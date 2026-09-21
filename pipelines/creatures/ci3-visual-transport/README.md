# Creature CI3 — Visual Transport

## Objective

Build pack-owned visual roots that can be loaded, inspected, released, and reloaded through the validated FoA-compatible asset route.

## Procedure

1. Convert/build only the visual content needed by the selected source creature.
2. Use explicit pack-owned asset addresses.
3. Build the catalogue/bundle/dependency closure required by the supported asset-loading route.
4. Load the visual root in the target environment.
5. Record:
   - catalogue/bundle identity;
   - hashes;
   - explicit root/address;
   - dependency closure;
   - missing scripts;
   - shader/renderer state;
   - mesh/skeleton fingerprint.
6. Release the root.
7. Load it again to prove repeatable asset lifetime.

## Validation gate

PASSED when the exact visual root loads and releases repeatedly with the expected structural fingerprint.

## Important boundary

Headless or non-rendering asset load cannot stand in for graphics-dependent Kandra/HDRP visual acceptance.

## Does not prove

Animation mapping, templates, actor construction, movement, combat, death, or persistence.

## Next

Proceed to [CI4A animation mapping](../ci4a-animation-mapping/README.md).
