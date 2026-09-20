# Customize the HUD with Separate System and Visual Proof

**Evidence status: PARTIAL.** Native visibility ownership is well identified, while several custom visual themes had build/deploy/resource proof without complete in-game visual acceptance.

Working lineage: [Native HUD Ownership vs Custom Visual Proof](../../../research/case-studies/ui/hud-owner-and-visual-proof.md).

## Key rule

Do not merge two claims:

1. **the hook/owner is correct**;
2. **the custom visual looks correct in game**.

Both need evidence.

## Native visibility lane

The researched path includes:

- `VHeroHUD.ShowBars`;
- `VHeroHUD.UpdateCanvasGroups()`.

Use the native visibility owner when changing when HUD bars appear.

## Custom visual lane

For themes/layouts/resources:

1. load/validate your custom resources;
2. attach them to the intended HUD owner;
3. verify geometry/bounds/anchors;
4. capture in-game visual evidence;
5. test multiple resolutions/aspect ratios if claimed;
6. restore/destroy owned UI on teardown.

## Do not promote build proof into visual proof

These are separate:

```text
asset embedded
≠ resource loaded
≠ HUD object created
≠ correctly positioned
≠ visually accepted
```

## Verification

Track two receipts:

### System receipt
- native owner found;
- hook/visibility path works;
- native HUD lifecycle preserved.

### Visual receipt
- screenshot/in-game acceptance;
- layout/scale correct;
- no overlap/clipping;
- hide/show transitions correct;
- teardown clean.

## Current proof boundary

System ownership evidence is stronger than the visual-design evidence for some versions. Keep that distinction explicit in every theme/layout guide.
