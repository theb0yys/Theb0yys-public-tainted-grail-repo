---
document_type: mechanic
scope: equipped custom weapon presentation
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: LOG_LEVEL_PROVEN
  persistence: NOT_APPLICABLE_TO_VISUAL_ROUTE
last_verified: 2026-09-20
---

# Equipped Custom Weapon Presentation

FoA weapon presentation is not “attach a MeshRenderer to the hand”.

The proven direction preserves the native equip lifecycle and Drake ownership:

```text
registered custom ItemTemplate
→ native ItemEquip selects equipped visual
→ capture/reuse native CharacterHandBase prototype contract
→ build or clone CharacterHandBase presentation
→ DrakeLodGroup + DrakeMeshRenderer remain the renderer owners
→ registered mesh/material keys are served
→ native hand lifecycle owns placement/hide/show/teardown
```

## Runtime evidence

A private live receipt for Tainted Weapons 0.3.6 observed:

- framework registration accepted for the tested Evil Greatsword;
- native equip redirect;
- a framework Drake prototype with one `CharacterHandBase`, one `DrakeLodGroup`, one `DrakeMeshRenderer` and no ordinary Unity renderers;
- registered mesh and material keys served through the Drake loading manager;
- no global Material coercion / `InvalidKeyException` in the checked log.

That receipt did **not** complete the repeated equip/unequip loop, two-instance, hide/show or equipped scene-transition fixtures.

## Design rule

Keep global generic Addressables mesh/material hooks out of this route. The private implementation explicitly disabled them after they broke unrelated UI asset loads on Mono.

If the item exists and works but is invisible, use [Content registers but is not visible](../../diagnose/content-registers-but-not-visible.md).
