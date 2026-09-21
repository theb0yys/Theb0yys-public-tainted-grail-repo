# Tainted Framework

**Status:** Use only documented services. It is not a general FoA gameplay SDK.

Tainted Framework provides shared runtime services that have been documented for common use.

An internal service, capability ID, host assembly, or design document is not automatically supported for use by other mods.

## What a mod author should do

1. Identify the exact service you need.
2. Find its public contract or tooling page.
3. Check whether that service is currently documented as available.
4. Depend only on the required public assembly/API.
5. Fail closed if the service is unavailable or incompatible.
6. Validate the feature independently in your mod.

## Currently documented services

### Runtime report

`framework.runtime-report` is a read-only diagnostics API.

The Tainted Diagnostic Tool shows the intended usage pattern.

### Native item registrar

There is a shared registration design for native items, but it is **not a universal register-anything API**. Use it only for content types that are explicitly documented as supported.

## Relationship to Avalon Core

- Avalon Core = discovery, evidence, contracts, and planning metadata.
- Tainted Framework = concrete reusable runtime services.

Do not add both dependencies by default.

See:

- [Runtime report](runtime-report.md)
- [Native item registrar](native-item-registrar.md)
- [Framework service gate recipe](../../recipes/framework-service.md)
