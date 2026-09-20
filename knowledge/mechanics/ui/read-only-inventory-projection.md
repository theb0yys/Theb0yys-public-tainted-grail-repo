---
document_type: mechanic
scope: custom read-only inventory projection over HeroItems
runtime: mono
evidence:
  static: SOURCE_REVIEW_PASSED
  build: OUTSTANDING_IN_CITED_MATRIX
  runtime: OUTSTANDING_IN_CITED_MATRIX
  persistence: NOT_APPLICABLE
last_verified: 2026-09-20
---

# Read-Only Inventory Projection

The Tainted Interface Inventory Suite's first slice deliberately stops before gameplay mutation.

## Source-reviewed boundary

The custom host:

- projects from loaded `TG.Main`, `Hero.Current`, `HeroItems` and native `Item` members;
- excludes native items with `HiddenOnUI`;
- searches/sorts/groups the projection only;
- owns no equip/use/quick-slot/drop/transfer action;
- calls no save/persistence API;
- fails closed rather than creating a duplicate global EventSystem;
- acquires/releases the shared custom-UI scope;
- keeps search focus stable during projection rebuild.

## Evidence state

Those checks are **repository-source evidence**.

The cited runtime matrix still had all open/close, focus, device switching, cursor restoration, gameplay-input restoration, world-time restoration, conflicts, scene teardown and large-inventory tests OUTSTANDING.

Do not present this source slice as a proven replacement inventory.
