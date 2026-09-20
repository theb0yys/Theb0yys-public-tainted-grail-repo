# Build a Read-Only Avalon Core Consumer

Use this guide when you want your mod to consume shared framework metadata/capability discovery without granting the framework permission to mutate gameplay.

This is the safest introduction to framework integration because the first public Avalon Core consumer baseline is explicitly **read-only**.

Working lineage: [Discovery Before Mutation](../../../research/case-studies/frameworks/discovery-before-mutation.md).  
Runnable source: [Avalon Core Read-Only Consumer](../../../examples/mono/infrastructure/core-readonly/README.md).

## What you will build

A BepInEx mod that:

```text
declares Avalon Core dependency
→ loads after Core
→ reads trust reports
→ looks up the documented adapter-registry engine
→ reads registry/capability information
→ logs supported metadata
→ fails closed when required contracts are unavailable
→ performs zero gameplay mutation
```

## Prerequisites

1. complete the [first Mono plug-in](../../getting-started/first-mono-plugin.md);
2. install/release-build the required Avalon Core assemblies locally;
3. know where `AvalonCore.dll`, `AvalonCore.Abstractions.dll` and `AvalonCore.Trust.dll` are located.

For broader infrastructure ownership, read [Tooling and Shared Infrastructure](../../../platform/README.md).

## Step 1 — build the provided example first

Point the project at your installed Core directory:

```powershell
dotnet build .\CoreReadOnly.csproj -c Release \
  -p:FoAGameRoot="C:\Games\Tainted Grail FoA" \
  -p:AvalonCoreDir="C:\path\to\AvalonCore"
```

Do not start by copying internal Core types into your own mod. Consume the published assemblies/contracts.

## Step 2 — declare the dependency

Your plug-in should make its dependency/load-order requirement explicit.

The consumer should not quietly run a degraded mutation path when the required framework is absent.

For a read-only optional feature, your policy can instead be:

```text
Core unavailable
→ report capability unavailable
→ leave gameplay untouched
```

## Step 3 — read public discovery state only

The proven baseline includes:

- read-only trust report access;
- declarative descriptor registration;
- adapter/capability queries;
- service-contract compatibility checks;
- registry report logging.

Do not reach into Core private fields because a public contract already exists.

## Step 4 — look up exact capabilities

Treat a capability/engine ID like an API contract, not a fuzzy name.

For the public example, the documented target is the `adapter-registry` engine.

Your code should:

1. query the public registry;
2. require the exact expected identity;
3. verify version/compatibility information where the contract exposes it;
4. treat absence/incompatibility as unavailable;
5. avoid mutation fallback hacks.

## Step 5 — fail closed

A framework consumer should be predictable when:

- Core is missing;
- the required engine is absent;
- a contract version is incompatible;
- trust/discovery state is unhealthy;
- a query throws.

For this read-only guide, "fail closed" means:

> report that the capability cannot be used and do nothing to gameplay.

## Step 6 — keep discovery separate from execution

A successful registry query proves:

- dependency/load order;
- API connectivity;
- contract identity;
- read-only metadata access.

It does **not** prove:

- a gameplay provider works;
- an adapter can safely mutate state;
- a transport is authenticated;
- a framework service is release-ready.

Each execution capability needs its own evidence.

## Verify in game

1. Core loads before your consumer;
2. your consumer loads successfully;
3. trust/discovery information can be read;
4. exact `adapter-registry` lookup succeeds on the supported setup;
5. the expected report/capability information is logged;
6. intentionally missing/incompatible capability produces a clean unavailable result;
7. no gameplay state is mutated;
8. removing the consumer leaves Core/gameplay unchanged.

## Common mistakes

### Treating framework presence as capability availability

A DLL being loaded does not mean every service/capability is promoted.

Query the public contract.

### Reflecting into private framework state

That bypasses the compatibility boundary the framework exists to provide.

### Turning a discovery result into permission to mutate

Discovery and execution are separate capabilities.

### Silently substituting a local implementation

If the shared owner is required, report unavailable instead of inventing an unreviewed replacement.

## Evidence boundary

**Proven:** dependency/load order, read-only trust reports, declarative registration, adapter/capability discovery, service-contract compatibility and fail-closed behaviour with zero mutation.

**Not claimed:** gameplay execution, arbitrary adapters, provider correctness, authenticated external transport or general framework readiness.

## Next steps

After the read-only consumer is stable:

- study the specific platform component you actually need;
- consume only a named promoted runtime surface;
- use a separate guide/evidence lane for execution.

Do not broaden from "I can discover it" to "I can safely execute it" without the corresponding contract and proof.
