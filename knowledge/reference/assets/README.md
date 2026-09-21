# Assets, Addressables, and Presentation

> **Reference page.** Use this when working with models, textures, icons, prefabs, AssetBundles, Addressables or `ARAssetReference`.

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

## How we interact with it

Keep asset transport separate from gameplay registration.

For a first custom item, reuse a known native presentation first when possible. Add custom icons/models only after the custom identity itself works.

For custom external assets:

1. record source/licence/provenance;
2. cook only mod-owned/redistributable assets;
3. build for the correct Unity/player target;
4. validate the exact asset resolves;
5. bind it through the system that actually owns that presentation;
6. validate unload/release.

## Why this route

The working repo contains multiple cases where a bundle/prefab loaded correctly but that fact alone did not prove gameplay integration.

Separating **asset loads** from **definition registration** and **runtime ownership** makes failures diagnosable.

## What goes wrong

Known mistakes:

- assuming a loaded AssetBundle means the item/weapon/NPC is registered;
- binding a custom mesh directly to an unrelated runtime renderer and bypassing native equip ownership;
- using the wrong Unity version/target;
- missing dependencies/materials/shaders;
- stale or invalid Addressables address;
- treating Merlin replacement capability as a general content-addition API;
- failing to release resources or cleaning up the wrong owner.

## How to verify

Verify separately:

- bundle/catalogue existence and hash;
- exact asset load;
- dependency closure/no missing scripts;
- material/shader/renderer state;
- binding from gameplay definition to asset;
- native owner actually presents it;
- clean unload/release.

## Current proof boundary

This repository has strong examples for mod-owned AssetBundle loading and several Addressables/ModService proof routes. Asset success must never be upgraded into a gameplay-registration claim without the corresponding native definition/runtime proof.
