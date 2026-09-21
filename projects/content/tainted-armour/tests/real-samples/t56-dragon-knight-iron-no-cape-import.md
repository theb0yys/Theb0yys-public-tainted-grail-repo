# T56 Dragon Knight Iron/No-Cape Import Receipt

## Executed path

- Unity: `6000.0.64f1`
- Source asset: `Assets/DragonKnightArmorA2KT20/SK_Dragon_knight_UE5_no_Cape.fbx`
- Source SHA-256 before and after:
  `b20cbf3a1c5d9a8e82bd84b31a52a8f5db2bd3befaac29f731c9ed9702bd1b08`
- Renderer and mesh: `UE5_no_cape`
- Mesh: 43,159 vertices, 88 bones, and 43,159 bone weights
- Evaluated submesh: index 3, 25,660 triangles
- Pose/clip: `fall` / `Anim_Hero_TPP_Base_Knockdown_Air_Loop`
- Sample time: `0.28333336114883423`
- Production entry point: `ArmorImporter.Import`
- Executed import count: 6

The importer-owned Unity command created transient baseline and posed renderer
states, used `UnityGeometrySnapshotAdapter` for baked geometry and topology, and
fed the resulting typed requests into the production importer. Temporary Unity
staging was removed after the receipt was copied into this framework-owned path.

## Results

| Target | Bone | Vertices | Triangles | Reversed | Collapsed | Indeterminate | Status |
| --- | --- | ---: | ---: | ---: | ---: | ---: | --- |
| female | `neck_02` | 252 | 617 | 3 | 0 | 0 | `DeformationBlocked` |
| female | `spine_04` | 8,255 | 15,245 | 208 | 0 | 0 | `DeformationBlocked` |
| female | `spine_05` | 370 | 879 | 11 | 0 | 0 | `DeformationBlocked` |
| male | `neck_02` | 252 | 617 | 3 | 0 | 0 | `DeformationBlocked` |
| male | `spine_04` | 8,255 | 15,245 | 208 | 0 | 0 | `DeformationBlocked` |
| male | `spine_05` | 370 | 879 | 11 | 0 | 0 | `DeformationBlocked` |
| **Total** |  |  |  | **444** | **0** | **0** |  |

All six control calibrations passed. Exact row counts and triangle ordinals
match the T56 evidence. The source fingerprint was stable.

The 2026-08-11 full-geometry refresh is recorded in
`t56-dragon-knight-iron-no-cape-full-geometry-import.json`. That refreshed
receipt keeps `controlsPassed=true`, `controlFailureCount=0`, the
out-of-plane rigid-rotation control as a diagnostic warning, and
`metricCalibrationFailed=false`. The offline Unity command does not fabricate
an A2K-T34 live observer request, so `visualRuntimeRequested=false` in that
receipt; live A2K-T34 observations remain owned by the diagnostic transport.
The non-full JSON remains the original 2026-08-09 Unity receipt. It is retired
from downstream fixture use and should not be used as the current
calibration-field source.

## Enforced boundary

The receipt marker is
`TAINTED_ARMOUR_REAL_SAMPLE_IMPORT_EXECUTED_DEFORMATION_BLOCKED_DOWNSTREAM_FALSE`.
Candidate-map application and every recorded downstream operation are false,
including conversion, payload and sidecar generation, runtime loader and
registration changes, item registration, equip, inventory, save and native-game
writes, asset-bundle and Addressables builds, and release readiness.

Production assembly fingerprints used by the original non-full Unity run:

- `Tainted.Armour.dll`:
  `3fd34988797a0da2277207f25870ac4ab3163d381179484ca90f42a864beafde`
- `Tainted.Armour.Unity.dll`:
  `3bd4f9ac9543b7fa7e75d074c0ef88e60fa09f4e462d2e800b73088ca5f3530c`

Production assembly fingerprints used by the refreshed full-geometry Unity
run:

- `Tainted.Armour.dll`:
  `e977bf5c14122a73b818d622e63daeff22ea6fbbe6f3dbcef01b1e050156dc3e`
- `Tainted.Armour.Unity.dll`:
  `06270192fe476f7b6e0b802b15af228e8ac69967fbfb553207ce96a2648ff605`

Current machine-readable fixture evidence:
`t56-dragon-knight-iron-no-cape-full-geometry-import.json`.

Historical machine-readable evidence retained for comparison only:
`t56-dragon-knight-iron-no-cape-import.json`.
