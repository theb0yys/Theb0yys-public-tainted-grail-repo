# Avalon Contracts

Use Avalon Contracts when separate mods need a common way to publish and discover provider data, capabilities, state, evidence, or validation results.

Provider/consumer discovery is available for normal use. Lifecycle execution is more restricted: use only the specific lifecycle routes documented as available for the integration you are building.

## Public model

Providers register explicitly.

Consumers can discover/read host availability, provider descriptors, capability IDs, catalog/state/evidence snapshots, and preview/validation results.

Useful public surfaces include:

- `AvalonContractsApi.DiscoverHost()`
- explicit provider registration/unregistration;
- catalog/state/evidence/preview/validation snapshots.

## Ownership boundary

Avalon Contracts does not auto-load provider mods and does not become the owner of provider gameplay truth.

Provider mods still own their domain state, target/reward semantics and any native gameplay/save integration.

Lifecycle command routes and provider callbacks must be treated by their **exact promoted lane**, not as generic mutation authority.

See:

- [Providers and consumers](providers-consumers.md)
- [Provider/consumer recipe](../../recipes/contracts-provider.md)
