<!-- Canonical Wave 5 location. Migrated from docs/reference/PROPRIETARY_RENDERING_SYSTEMS.md. -->
# FoA Rendering Ownership

Tainted Grail uses specialised rendering ownership in addition to ordinary Unity renderers.

## Character and clothing presentation

Skinned character/clothing presentation uses the game's Kandra path. Treat mesh registration, rig/deformation ownership, clothing stitching and equip teardown as one lifecycle.

See [Armour/Kandra lifecycle](../../docs/reference/ARMOUR_KANDRA_LIFECYCLE.md).

## Rigid presentation

Rigid gameplay presentation can be owned by the game's rigid-rendering path rather than a loose replacement `MeshRenderer`. Preserve the gameplay/equip owner and let the native presentation owner control runtime lifetime.

See [Weapon native lifecycle](../../docs/reference/WEAPONS_NATIVE_LIFECYCLE.md).

## World/static presentation

Large world content can be owned by scene/culling/streaming systems rather than per-object runtime GameObjects.

## Rule

Identify the native presentation owner first. Do not treat “a mesh is visible” as equivalent to correct game integration.
