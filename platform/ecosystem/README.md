# Shared Infrastructure Map

This page shows which shared project handles each common job so a mod author can avoid rebuilding the same infrastructure.

```text
                           FEATURE MOD
                               │
        ┌──────────────────────┼───────────────────────┐
        │                      │                       │
 settings/input/UI        presentation             diagnostics
        │                      │                       │
 FoA Mod Manager        Tainted Interface      Diagnostic Tool
        │                      │                       │
        └──────────────┬───────┴──────────────┬────────┘
                       │                      │
                 capability discovery     shared runtime services
                       │                      │
                  Avalon Core          Tainted Framework
                       │
              ┌────────┴────────┐
              │                 │
        Avalon AI Runtime   Avalon Contracts
          AI integration      provider data
              │
              └────────────┐
                           │
                Tainted Grail Extender
                 external/local SDK bridge
```

You do **not** need to install all of these. Start with the smallest dependency set that solves your problem.

## Practical references

- [Component reference](component-reference.md) — GUIDs, assemblies, and public API entry points.
- [Dependency and maturity matrix](dependency-matrix.md) — which integrations are ready to use.
- [Dependency and packaging rules](dependency-and-packaging.md) — hard/soft dependencies and release packaging.
- [Recommended author stacks](author-stacks.md) — useful combinations for common mod types.

## Dependency direction

Prefer this:

```text
feature mod
→ documented shared API
→ native/game integration
```

Avoid this:

```text
feature mod A ↔ private internals of feature mod B
shared UI project → gameplay state ownership
AI package → direct unmanaged access to unrelated FoA systems
```

Shared projects should handle common infrastructure. The feature mod should continue to own its own gameplay rules and state.
