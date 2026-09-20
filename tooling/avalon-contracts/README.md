# Avalon Contracts

**Posture: Read-only/provider discovery with bounded lifecycle contracts**

Use Avalon Contracts when independent mods need a shared contract/provider surface without centralising each provider's gameplay truth.

## Public model

Providers register explicitly.

Consumers can discover:

- host availability;
- provider IDs/descriptors;
- aggregate capability IDs;
- catalog/state/evidence snapshots;
- preview/validation state.

The static API includes `AvalonContractsApi.DiscoverHost()` and explicit provider registration/readback surfaces.

## Ownership boundary

Avalon Contracts does not auto-load providers and does not own provider gameplay truth.

Provider mods still own:

- their source state;
- target selection;
- reward/gameplay semantics;
- persistence where applicable.

See [Providers and consumers](providers-consumers.md).
