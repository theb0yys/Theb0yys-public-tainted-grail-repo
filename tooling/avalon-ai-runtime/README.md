# Avalon AI Runtime

**Posture: Capability-gated single AI host**

Use Avalon AI Runtime when your mod needs reusable AI decision logic that should compose with other AI packages.

Do **not** ship another independent AI scheduler/host for the same actors.

## Canonical pipeline

```text
AI package
→ AvalonAI.Contracts
→ Avalon AI Runtime
→ single FoA host
→ reviewed native/game-system executor
```

Canonical architecture rule:

**Rabbit remembers, GOAP reasons, Blaze acts, Avalon owns access/ownership/safety.**

## Third-party package boundary

Packages reference **Avalon AI Contracts only**.

They do not reference:

- FoA internals;
- Rabbit implementation;
- GOAP implementation;
- Blaze implementation;
- the FoA host implementation;
- another package's private runtime.

See:

- [Package authoring](package-authoring.md)
- [Single-host ownership](host-ownership.md)
