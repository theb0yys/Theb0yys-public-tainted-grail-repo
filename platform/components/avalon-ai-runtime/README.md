# Avalon AI Runtime

Use Avalon AI Runtime when your mod contributes AI goals, actions, or domain information that should participate in the shared AI system.

Package-authoring contracts are public. Live AI execution remains controlled by the single shared host and its capability checks, so feature mods should not ship a competing scheduler for the same actors.

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
- [AI package recipe](../../recipes/ai-package.md)
