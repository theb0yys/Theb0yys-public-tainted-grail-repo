# Shared Infrastructure Map

The infrastructure is deliberately layered by ownership.

```text
                           FEATURE MOD
                               │
        ┌──────────────────────┼───────────────────────┐
        │                      │                       │
 user/config/input         presentation             research
        │                      │                       │
 FoA Mod Manager        Tainted Interface      Diagnostic Tool
        │                      │                       │
        └──────────────┬───────┴──────────────┬────────┘
                       │                      │
                shared discovery        promoted runtime services
                       │                      │
                  Avalon Core          Tainted Framework
                       │
              ┌────────┴────────┐
              │                 │
        Avalon AI Runtime   Avalon Contracts
         single AI host      provider/contract host
              │
              └────────────┐
                           │
                Tainted Grail Extender
                 local external SDK bridge
```

This is an ownership diagram, not a requirement to install everything.

## Practical references

- [Component reference](component-reference.md) — GUIDs, assemblies and public API entry points.
- [Dependency/maturity matrix](dependency-matrix.md) — what is safe to consume.
- [Recommended author stacks](author-stacks.md) — smallest useful combinations.

## Dependency direction

```text
feature → shared contract/service owner
shared owner → bounded provider/native adapter
```

not:

```text
shared framework → feature implementation
feature A ↔ feature B private internals
AI package → FoA internals
UI framework → gameplay truth
```

The shared layer coordinates common concerns; feature mods retain their feature/gameplay state.
