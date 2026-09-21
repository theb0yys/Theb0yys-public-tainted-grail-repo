# Avalon AI Runtime

**Status:** Package authoring is supported. Live execution uses one shared host.

Use Avalon AI Runtime when your mod owns domain data that should participate in shared AI decisions.

Do **not** ship another independent scheduler/host for the same actors.

## How it fits together

```text
feature/provider data
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

The V2 package API centers on `IAvalonAiPackage`:

- `Manifest`
- `GoalPolicies`
- `GoalDefinitions`
- `ActionDefinitions`

The host/runtime decides whether those declarations can run for the current actor, world state, and available capabilities.

See:

- [Package authoring](package-authoring.md)
- [Single-host ownership](host-ownership.md)
- [AI package recipe](../../recipes/ai-package.md)
