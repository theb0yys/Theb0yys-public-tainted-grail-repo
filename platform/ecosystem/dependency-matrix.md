# Dependency and Maturity Matrix

| Infrastructure | Typical use | Consumer dependency | Current safe author use | Do not assume |
| --- | --- | --- | --- | --- |
| FoA Mod Manager | settings, controller actions, status, modal UI scope | optional or hard depending on feature | direct public API / ordinary BepInEx config | gameplay ownership |
| Tainted Interface | shared styles/icons/textures/HUD resources | usually optional; hard only when feature requires it | direct public resource/style API | feature input/state ownership |
| Tainted Diagnostic Tool | research IDs/templates/spawners/runtime context | none | install and collect read-only dumps | a dump row is approval |
| Avalon Core | capability/evidence/catalog discovery | hard when directly referenced | `TrustReports`, documented registry/discovery | runtime execution authority |
| Tainted Framework | concrete shared runtime services | **surface-specific** | currently documented promoted surfaces only | every internal capability is public |
| Avalon AI Runtime | shared AI decision/execution host | package references Contracts only | package manifests/goals/actions/blackboard contracts | direct package → FoA calls |
| Avalon Contracts | shared contract/provider readback | provider/consumer-specific | explicit registration + discovery/readback; exact promoted lifecycle lanes only | host owns provider gameplay truth |
| Tainted Grail Extender | extensions and external local SDK | required for TGE route | explicit extension/service/SDK contracts | generic remote/admin command server |

## Hard-dependency rule

Use a hard dependency when your mod **cannot perform its advertised feature correctly without the owner**.

Use a soft/optional dependency when the integration is enhancement-only and your mod has a clear local fallback.

Never hide a hard semantic dependency behind reflection just to make the DLL technically optional.

## Avoid dependency cycles

Bad:

```text
feature → visual framework → feature
feature → AI host implementation → feature
Core → downstream gameplay mod
```

Preferred:

```text
feature → public contract
host → contract-defined provider/package
host → reviewed native adapter
```
