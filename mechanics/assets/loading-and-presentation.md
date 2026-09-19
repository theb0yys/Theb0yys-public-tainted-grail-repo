<!-- Canonical Wave 5 mechanic page split from docs/reference/ASSETS.md. -->
# Assets, Addressables, and Presentation — Modding Mechanics

> **Document type: mechanic / capability.** Read [the canonical native-system page](../../systems/assets/README.md) first for ownership, identities, lifecycle, and the current technical proof boundary.

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

## Evidence boundary

This Wave 5 split does not strengthen the underlying technical evidence. Current claim scope is owned by [the native-system page](../../systems/assets/README.md).
