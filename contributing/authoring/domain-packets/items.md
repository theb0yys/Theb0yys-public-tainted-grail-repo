# Domain Packet — Items

## Subject

Custom item definition, native registration, native item construction, acquisition, distribution, and persistence boundaries.

## Reader questions

1. What is an FoA item definition and who owns it?
2. How does a genuinely new item identity become resolvable?
3. How is a live native `Item` created?
4. How is registration different from acquisition/distribution?
5. Which acquisition routes are separately owned?
6. What does current-session success fail to prove about save/load?

## Archetypes required

- **System hub:** item/template/runtime ownership.
- **Mechanic:** custom item registration/integration.
- **Mechanic:** acquisition.
- **Mechanic:** distribution.
- **Reference:** exact identities/registries/evidence.
- **Case:** first proven custom item and batch-generalisation correction.
- **Learning journey:** first custom content route.

## Canonical native owners

- `ItemTemplate` — reusable item definition.
- `TemplatesLoader` — template-map lifecycle/insertion.
- `TemplatesProvider` — normal lookup.
- `World` — native/MVC item-instance ownership.
- `HeroItems` — hero inventory owner.
- acquisition/distribution owners such as `RestockableStock`, `ShopUI`, container/pickup/reward/crafting owners.

## Required prerequisites

- [Templates and registries](../../../systems/core/templates-registries.md)
- [Native object ownership](../../../systems/core/native-object-ownership.md)
- [Saving and persistence](../../../systems/world/saving-persistence.md)
- [Evidence status](../../../reference/evidence/README.md)

## Public source set

- `docs/reference/ITEMS.md`
- `docs/reference/LOOT_REWARDS_ACQUISITION.md`
- `docs/reference/DISTRIBUTION_MERCHANTS_LOOT.md`
- `docs/reference/IDENTITY_GUIDS_NAMES.md`
- `docs/reference/LIFECYCLE_HOOKS.md`
- existing first-item learning page and public examples.

## Private-evidence conclusions to preserve

The reviewed private evidence supports these bounded conclusions:

- exact template identity and template readiness are prerequisites;
- multiple independent implementations use a native-template clone/registration pattern;
- direct/private loader-map registration is patch-sensitive and not transactional;
- normal live item creation uses native `Item` ownership rather than a parallel mod object;
- registration and acquisition are different capabilities;
- merchant/UI timing is part of the first proven acquisition route;
- direct grant, merchant, container, world pickup, reward, recipe and loot-table routes have different owners;
- template identity persistence depends on GUID resolution, but runtime success does not establish cold-save/missing-mod safety.

This packet does **not** promote every private mechanics-index row into current public authority.

## Claims to teach

- Display name is not identity.
- A cloned object is not registered merely because it exists in Unity.
- Registration success is provider re-resolution of the new identity, not just `AddToMap` returning.
- A registered template is still not obtainable until a gameplay owner acquires/distributes a live `Item`.
- Direct grant is a controlled acquisition proof, not loot-table integration.
- Merchant insertion is one distribution mechanic, not a universal item-registration process.
- Persistence is a separate proof lane.

## Failure/correction history to preserve

- native GUID reuse creates collision/replacement risk;
- too-early registration/lookup races readiness;
- too-late shop mutation can miss the UI's stock snapshot;
- batch-first expansion made failures ambiguous and required returning to a one-item proof;
- asset/icon/model success was initially easy to confuse with item-registration success;
- current-session visibility does not establish restore/uninstall safety.

## Required diagrams

1. **Ownership:** `ItemTemplate → TemplatesLoader/Provider → Item → acquisition owner → UI/gameplay`.
2. **Lifecycle:** templates ready → clone/identity → register → provider resolve → construct live item → acquisition/distribution.
3. **Evidence:** runtime visibility and persistence diverge after acquisition.

## Worked-example candidate

The bounded Green Avalon Apple path may be used as a public teaching example where the existing public text already supplies the necessary public-safe identities and proof boundary.

## Evidence lanes

- source/static: strong for registration and native item ownership;
- runtime: bounded for the first proven merchant route;
- persistence: separate and not universal;
- compatibility: private/reflected registration is patch-sensitive;
- release: not inherited by newly authored examples.

## Public/private exclusions

Do not publish private implementation source or bulk decompiled code. Use independently authored pseudocode/examples and public-safe identities already present in the public corpus.

## Canonical target map

- `systems/items/README.md` — ownership and domain map.
- `mechanics/items/custom-item-integration.md` — current `ITEMS.md` process.
- `mechanics/items/acquisition.md` — current acquisition page.
- `mechanics/items/distribution.md` — current merchant/loot/distribution page.
- `learn/content-authoring/items/README.md` — guided route.
- exact lookup remains under `reference/`.

## Completion checks

- system hub does not duplicate full mechanics;
- mechanic explicitly separates registration from acquisition;
- acquisition/distribution routes remain separate;
- persistence boundary remains visible;
- legacy item pages redirect cleanly;
- no public example inherits private runtime status.
