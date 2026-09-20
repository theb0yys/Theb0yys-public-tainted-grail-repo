# Tainted Framework

Use Tainted Framework only when your feature needs one of its specifically documented shared runtime services.

It is not a general FoA gameplay SDK. Some internal services exist only for framework implementation or future work, so a service becomes consumer-ready only when its public contract is explicitly documented below.

## What an ordinary author should do

1. Identify the exact service you need.
2. Find its public tooling page/contract.
3. Check its current consumer posture.
4. Depend only on the required public assembly/surface.
5. Fail closed if the surface is unavailable/incompatible.
6. Validate the feature independently in your mod.

## Current practical public lanes

### Runtime report

`framework.runtime-report` is a read-only diagnostics surface.

The Tainted Diagnostic Tool is the reference consumer pattern.

### Native item registrar

There is a shared ownership direction and readiness contract, but it is **not a universal register-anything API**. Use only if/when the exact registrar lane is promoted for your content type.

## Relationship to Avalon Core

- Avalon Core = discovery/evidence/contracts/planning metadata.
- Tainted Framework = concrete reusable runtime service implementation when promoted.

Do not add both dependencies by default.

See:

- [Runtime report](runtime-report.md)
- [Native item registrar](native-item-registrar.md)
- [Framework service gate recipe](../../recipes/framework-service.md)
