# Contracts, Adapters, and Shared Ownership

A reusable system should centralize a repeated responsibility without becoming the owner of every feature.

## Preferred shape

```text
feature
→ small public contract
→ shared owner
→ runtime/version-specific adapter
→ native FoA owner
```

This isolates compatibility work while keeping feature truth in the feature/provider that owns it.

## Contract responsibilities

A public contract should define only what consumers need:

- capability identity;
- input/output DTOs;
- readiness/availability;
- failure behavior;
- lifecycle/cleanup;
- version semantics.

Avoid leaking private host classes into the contract.

## Adapter responsibilities

An adapter can own:

- Mono vs IL2CPP differences;
- target type/method discovery;
- game-version-specific binding;
- conversion between public DTOs and native types;
- capability/readiness checks.

An adapter should not silently invent gameplay policy that belongs to the feature.

## Discovery vs execution

Discovery metadata is not execution authority.

A registry can report:

```text
capability exists
version=2
ready=true
```

without being the component that executes the gameplay action.

Keep the actual execution owner explicit.

## Versioning

When other mods depend on your public contract, document:

- contract/API version;
- package/plugin version relationship;
- compatibility promises;
- deprecated surfaces;
- minimum supported version where proven.

See [API stability](../../tooling/ecosystem/api-stability.md) and [Distribution/versioning](../../tooling/ecosystem/distribution-and-versioning.md).
