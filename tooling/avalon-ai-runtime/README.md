# Avalon AI Runtime

**Posture: Package-authoring contracts are public; live execution stays single-host/capability-gated**

**Obtain Host:** https://www.nexusmods.com/taintedgrailthefallofavalon/mods/220

**Version signal:** current public Host version is `0.8.2`.

**Stability boundary:** third-party packages bind to the explicitly versioned `AvalonAI.Contracts.V2` contract, not the FoA Host implementation. Host/runtime internals and direct package-to-FoA execution remain gated.

See [distribution/versioning](../ecosystem/distribution-and-versioning.md) and [API stability](../ecosystem/api-stability.md).

Use Avalon AI Runtime when your mod owns domain truth that should participate in shared AI decisions.

Do **not** ship another independent scheduler/host for the same actors.

## Canonical pipeline

```text
feature/provider truth
→ AvalonAI.Contracts package
→ Avalon AI Runtime
→ single FoA host
→ reviewed native/game-system executor
```

Architecture rule:

**Rabbit remembers, GOAP reasons, Blaze acts, Avalon controls access, ownership and safety.**

## Package dependency boundary

Third-party packages reference **Avalon AI Contracts only**.

They should not reference the FoA host implementation, FoA internals, Rabbit implementation, GOAP implementation, Blaze implementation, or another package's private runtime.

The V2 package surface centers on `IAvalonAiPackage`:

- `Manifest`
- `GoalPolicies`
- `GoalDefinitions`
- `ActionDefinitions`

The host/runtime decides whether those declarations can run in the current actor/world/capability context.

See:

- [Package authoring](package-authoring.md)
- [Single-host ownership](host-ownership.md)
- [AI package recipe](../recipes/ai-package.md)
