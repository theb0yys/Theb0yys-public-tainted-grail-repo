# Item Pipeline Validation Matrix

Record each row independently.

| Gate | Required proof |
| --- | --- |
| Source identity | Exact native source resolves on the claimed build |
| Clone integrity | Source unchanged; custom clone has expected topology |
| Custom identity | Stable, unique GUID and template name |
| Registration | Custom GUID resolves through normal provider |
| Runtime Item | World-owned Item references custom template |
| Acquisition | Selected native owner contains the same Item |
| UI/presentation | Downstream owner visibly/structurally sees it |
| Asset lifetime | Custom presentation loads and releases correctly |
| Cleanup | No duplicate/leaked runtime ownership on repeated use |
| Persistence | Cold save/load if claimed |
| Missing package | Explicit disabled/missing behaviour if claimed |
| Migration | Upgrade path if schema/identity changes |
| Compatibility | Claimed runtime/build combinations tested |

A stage is Confirm the specific the lane actually tested.
