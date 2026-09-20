# Recipe: Avalon Contracts Provider / Consumer

Use this only when several mods need a shared contract/provider model.

## Provider

The provider owns its domain truth and explicitly registers its provider instance.

```text
provider plugin loads
→ check Avalon Contracts host availability
→ register provider explicitly
→ host exposes provider/catalog/readback
→ provider unregisters on teardown
```

Do not make the host discover providers by scanning assemblies/files.

## Consumer

Start with discovery/readback:

```csharp
AvalonContractHostDiscoverySnapshot host =
    AvalonContractsApi.DiscoverHost();
```

If the host/provider is unavailable, disable only that integration.

Then query only the catalog/state/evidence/preview/validation surfaces required by the feature.

## Mutation/lifecycle rule

Do not infer generic mutation authority from provider presence, a contract ID, a lifecycle DTO, or a command method existing in source.

Use lifecycle commands/callbacks only for their exact promoted provider/operation lane.

## Ownership

```text
Avalon Contracts owns contract coordination/readback
provider owns gameplay truth
consumer owns presentation/use of readback
native game owner keeps native state
```
