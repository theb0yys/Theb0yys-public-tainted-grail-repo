# Shared Infrastructure Map

The ecosystem is deliberately layered.

```text
                         MOD AUTHOR
                             |
          +------------------+------------------+
          |                  |                  |
      user/config        shared UI        diagnostics
          |                  |                  |
   FoA Mod Manager     Tainted Interface   Diagnostic Tool
          |                  |                  |
          +----------+-------+------------------+
                     |
             shared contracts/services
                     |
          +----------+-----------+
          |                      |
      Avalon Core          Tainted Framework
  discovery/evidence      runtime-facing services
          |                      |
          +-----------+----------+
                      |
              feature-specific hosts
                /             \
       Avalon AI Runtime    Avalon Contracts
          AI host          contract providers
                      |
              advanced external bridge
                      |
             Tainted Grail Extender
```

## Ownership summary

### FoA Mod Manager
Owns mod settings presentation, controller-action registration, status providers, shared custom-UI cursor/input/world-freeze scope.

### Tainted Interface
Owns shared visual resources, semantic texture/icon IDs, reusable render styles and UI resource packs.

### Avalon Core
Owns shared discovery, evidence/trust/catalog/contract metadata and fail-closed capability discovery. Treat its public baseline as read-only unless a later named capability is explicitly promoted.

### Tainted Framework
Owns reusable runtime-facing framework services that have completed their own promotion gates. Do not assume every internal service is consumer-ready.

### Avalon AI Runtime
Owns the **single AI runtime/host**. Packages describe goals/actions/blackboard requirements through Avalon AI Contracts; packages do not own FoA scheduling or direct host access.

### Avalon Contracts
Owns cross-mod contract/provider discovery, readback and lifecycle contract surfaces. Provider mods remain owners of their gameplay truth.

### Tainted Grail Extender
Owns advanced extension hosting and authenticated local SDK transport for external development clients.

### Tainted Diagnostic Tool
Owns read-only evidence collection. It discovers facts; it does not approve mutations.

## Dependency direction

Feature mods should depend **toward shared owners**, never the reverse.

Shared infrastructure must not take over feature gameplay state merely because a consumer uses it.
