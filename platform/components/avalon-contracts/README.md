# Avalon Contracts

**Status:** Discovery and readback are supported. Lifecycle operations are available only through explicitly documented APIs.

Use Avalon Contracts when separate mods need a shared model for provider discovery, catalog/state/evidence readback, previews, validation, and supported lifecycle operations.

Do not use it merely because your mod has a config option named “contract”.

## Public model

Providers register explicitly.

Other mods can discover/read host availability, provider descriptors, capability IDs, catalog/state/evidence snapshots, and preview/validation results.

Useful public APIs include:

- `AvalonContractsApi.DiscoverHost()`
- explicit provider registration/unregistration;
- catalog/state/evidence/preview/validation snapshots.

## What Avalon Contracts does not own

Avalon Contracts does not auto-load provider mods and does not become the owner of provider gameplay state.

Provider mods still own their domain state, target/reward semantics and any native gameplay/save integration.

Use lifecycle commands and provider callbacks only where the API explicitly documents them as supported.

See:

- [Providers and consumers](providers-consumers.md)
- [Provider/consumer recipe](../../recipes/contracts-provider.md)
