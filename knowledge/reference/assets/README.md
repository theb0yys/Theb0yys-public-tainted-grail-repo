# Assets, Addressables, and Presentation

Use this page when your mod works with **models, textures, icons, prefabs, AssetBundles, Addressables, or FoA asset references**.

The key rule is:

> Successfully loading an asset does not mean the game has registered or started using it.

Asset loading, gameplay definition registration, presentation binding, and cleanup are separate steps.

## The main asset paths

You may encounter:

- normal Unity assets;
- mod-built AssetBundles;
- Unity Addressables;
- FoA `ARAssetReference`;
- sprite/icon references;
- ModService-compatible catalogues/bundles;
- runtime-loaded prefabs, materials, meshes, and textures.

## Who loads what?

Different systems own different parts:

- Unity/AssetBundle APIs load raw bundle assets;
- Addressables resolve addressed/keyed content;
- FoA wrappers/ModService-compatible routes can add game-specific lookup and release behavior;
- a gameplay template can point to an asset, but the asset does not register the template;
- weapon/armour/native renderer systems own presentation lifetime when those routes are used.

Merlin Workshop is valuable for official replacement-oriented authoring and first-party source evidence. Do not treat that replacement capability as proof of a universal new-content registration API.

## Think of the full path

A typical custom presentation flow is:

~~~text
package/bundle exists
→ catalogue/bundle loads
→ exact asset resolves
→ gameplay definition references it
→ native presentation system uses it
→ resources are released correctly
~~~

Each step needs its own proof.

## Start simple

For a first custom item or gameplay proof, reuse a known native presentation when possible.

First prove:

- custom identity;
- registration;
- runtime use.

Then add a custom icon/model/material once you know the gameplay object itself works.

That makes asset failures much easier to diagnose.

## Adding custom assets

A safe workflow is:

1. record the source/licence of the asset;
2. include only content you are allowed to redistribute;
3. build/cook for the correct Unity/player target;
4. prove the exact address/path resolves;
5. bind it through the native system that owns that presentation;
6. verify cleanup/release.

## Common mistakes

- assuming "the AssetBundle loaded" means the item/weapon/NPC is registered;
- attaching a mesh directly to the wrong runtime object and bypassing native equip/render ownership;
- building with the wrong Unity version or player target;
- missing shader/material/dependency content;
- using a stale Addressables key;
- treating Merlin replacement support as generic content addition;
- leaking handles, pooled objects, or renderer resources;
- destroying a visible GameObject while leaving its logical/native owner alive.

## How to verify

Test these separately:

1. bundle/catalogue exists and is the expected build;
2. exact asset resolves;
3. dependencies/scripts/materials/shaders are present;
4. gameplay definition points to the asset;
5. the correct native owner actually presents it;
6. unload/disable releases the resources you own.

If the visual loads but gameplay still does not see the content, the problem is probably not the asset loader.

## Evidence limits

This repository has strong examples for mod-owned AssetBundle loading and several Addressables/ModService-compatible routes.

Those examples prove specific loading/binding mechanisms. They do not turn successful asset loading into proof of gameplay registration, persistence, or cross-build compatibility.
