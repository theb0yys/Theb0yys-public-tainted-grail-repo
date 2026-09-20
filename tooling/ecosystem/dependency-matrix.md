# Dependency and Maturity Matrix

| Infrastructure | Typical consumer | Public posture | Hard dependency? | Runtime mutation authority |
| --- | --- | --- | --- | --- |
| FoA Mod Manager | almost any user-facing mod | **Author-ready** | optional or required by feature | UI/config/controller/status only |
| Tainted Interface | UI/HUD/menu mods | **Author-ready for public API/resources** | usually optional | visual/UI resources; feature gameplay stays consumer-owned |
| Avalon Core | ecosystem-aware mods/tools | **Read-only/discovery baseline** | hard when directly referenced | none unless a named promoted lane says otherwise |
| Tainted Framework | framework consumers | **Capability-gated** | surface-specific | only promoted named services |
| Avalon AI Runtime | AI package providers | **Capability-gated / host-owned** | packages reference Contracts only | host/executor owns action execution |
| Avalon Contracts | contract providers/consumers | **Read-only + bounded lifecycle contracts** | provider-specific | provider-owned callbacks/lifecycle only where promoted |
| Tainted Grail Extender | extensions/external local tools | **Advanced/SDK** | yes for extension/SDK use | service-specific, authenticated, explicit |
| Tainted Diagnostic Tool | researchers/mod authors | **Author-ready read-only tool** | no | none |

## Do not create dependency cycles

Bad:

```text
feature mod → UI framework → feature mod
feature mod → AI host → feature mod implementation
Core → downstream gameplay mod
```

Preferred:

```text
feature mod → shared contract
shared host → contract-defined package/provider
host calls bounded provider/native adapter
```
