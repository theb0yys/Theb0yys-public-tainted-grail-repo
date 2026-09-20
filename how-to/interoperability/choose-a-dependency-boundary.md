# Choose a Cross-Mod Dependency Boundary

Use this when one mod needs functionality owned by another mod or shared infrastructure component.

Canonical dependency rules: [Shared Infrastructure Dependency and Packaging](../../tooling/ecosystem/dependency-and-packaging.md).

## 1. Decide whether the dependency is semantic

Ask:

> Can the advertised feature work correctly when the provider is absent?

If **no**, use a real hard dependency.

If **yes**, the integration may be optional/enhancement-only.

Do not use reflection merely to make a semantic hard dependency look optional.

## 2. Prefer a public contract

Use, in order:

1. versioned public contract;
2. supported public API;
3. promoted capability contract;
4. optional reflection bridge only for documented enhancement surfaces.

Do not reflect into private fields or patch another mod's internals as an integration API.

See [API stability](../../tooling/ecosystem/api-stability.md).

## 3. Hard dependency pattern

When the feature requires the provider:

```csharp
[BepInDependency(
    "provider.plugin.guid",
    BepInDependency.DependencyFlags.HardDependency)]
```

Keep referenced shared DLLs `Private=false` so your release does not vendor duplicate infrastructure.

## 4. Optional dependency pattern

For enhancement-only integration:

```text
base feature works alone
→ provider absent: no-op/fallback
→ provider present + compatible: enable bridge
```

Keep the optional API behind one bridge rather than spreading reflection checks throughout feature code.

Reference pattern: [Optional infrastructure bridge](../../tooling/recipes/optional-infrastructure.md).

## 5. Make resource use transactional

If the consumer spends an item/currency/cooldown to invoke another mod:

```text
verify local resource
→ request provider action
→ provider reports success
→ consume local resource
→ start cooldown
```

Do not consume first and hope the provider succeeds.

Working lineage: [Fail-Closed Cross-Mod API Bridge](../../case-studies/gameplay/cross-mod-api.md).

## 6. Teardown

If you register a provider, status callback, action, UI scope or subscription, unregister/release it on the matching lifecycle boundary.

## Validation

Record:

- provider GUID/package version;
- exact API/contract version or member;
- hard vs optional;
- provider-absent behavior;
- incompatible-version behavior;
- success/failure result;
- duplicate-call behavior;
- cleanup/unregister result;
- Mono/IL2CPP scope claimed.

A successful assembly load is not cross-mod feature proof.
