---
document_type: system
scope: native HeroStorage model, UI and persistence ownership
runtime: mono
evidence:
  static: CURRENT_BINARY_DECOMPILATION_E5
last_verified: 2026-09-20
source_artifact_sha256: 749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982
---

# Native Hero Storage

Hero Storage owns both a compressed inactive representation and a live item representation.

## Storage lifecycle

```text
inactive / compressed
  _stashedItems : ItemSpawningDataRuntime[]
        |
        | RequestItems → CreateAllItems → CreateItem
        v
active / live
  HeroStorage.Items / OwnedItems : Item models
        |
        | ReleaseItems → StashAllItems → StashItem
        v
inactive / compressed
```

`HeroStorageUI.OnInitialize()` calls `Storage.RequestItems()`.

`HeroStorageUI.OnDiscard()` closes the storage and releases/materialises state back into the compressed form.

## Native persistence

`HeroStorage.Serialize` writes `_stashedItems` through native save serialization.

`Deserialize` reconstructs that compressed list.

A mod browsing or transferring stash items should therefore respect the storage's own materialisation/reference-count lifecycle rather than treating it as an always-live list.

## Put / Take

Native storage has Put and Take tabs.

The deposit side reads `HeroItems.StashableInventory`, not every hero item.

The take side reads `HeroStorage.Items`.

Single-item transfer is delegated to `ItemUtils.MoveTo`; the storage UI coordinates the transfer but does not replace inventory accounting.
