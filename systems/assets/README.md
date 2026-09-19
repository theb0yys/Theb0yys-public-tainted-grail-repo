<!-- Canonical Wave 5 native-system page split from docs/reference/ASSETS.md. -->
# Assets, Addressables, and Presentation

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/assets/loading-and-presentation.md).

## What this system is

Assets are presentation/data payloads. They are not automatically gameplay registrations.

Important layers include:

- ordinary Unity assets;
- AssetBundles built by a mod;
- Unity Addressables;
- FoA `ARAssetReference` and ModService-compatible routes;
- UI icon references;
- runtime-loaded prefab/model/material resources.

## Who owns it in FoA

Ownership depends on the route:

- Unity/AssetBundle owns raw asset loading;
- Addressables owns keyed/addressed loading;
- FoA ModService/asset wrappers own game-specific asset lookup/release where used;
- a gameplay template may reference an asset, but the asset does not register the template;
- native equip/renderer systems own presentation lifecycle for weapons/armour where proven.

Merlin Workshop is useful here as the official replacement-oriented toolkit and as first-party source evidence for addresses, GUIDs and existing relationships. It is not a general new-content registrar.

## Important identities, types, and methods

Common concepts:

- `AssetBundle.LoadFromFile(...)`
- Addressables address/key
- `ARAssetReference`
- `ShareableSpriteReference` / sprite references
- prefab asset path
- material/mesh references
- ModService-compatible catalogue/bundle layout

## Where it exists in the lifecycle

A typical asset path is:

~~~text
package/bundle available
→ bundle/catalogue loads
→ exact asset address resolves
→ gameplay definition references it
→ native presentation owner instantiates/uses it
→ resource is released/cleaned up
~~~

Each arrow is a separate proof.

## Current proof boundary

This repository has strong examples for mod-owned AssetBundle loading and several Addressables/ModService proof routes. Asset success must never be upgraded into a gameplay-registration claim without the corresponding native definition/runtime proof.
