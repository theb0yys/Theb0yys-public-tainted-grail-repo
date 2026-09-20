# Recommended Author Stacks

Choose the **smallest stack that owns your actual problem**.

## 1. Ordinary gameplay mod

Use:

- BepInEx;
- Harmony only when a real native hook is needed;
- FoA Mod Manager optionally for config/controller/status.

Do not add Core/Framework/AI/TGE without a concrete integration need.

## 2. Rich UI / HUD mod

Use:

- FoA Mod Manager for shared cursor/input/controller/world-freeze scope;
- Tainted Interface for semantic styles/icons/textures;
- your feature mod for widgets, commands and gameplay state.

Recipe: [Custom modal UI](../recipes/custom-ui.md).

## 3. Ecosystem-aware mod

Use Avalon Core when you need trust/report readback, capability/adapter discovery, or shared catalog/evidence metadata.

Start read-only. If you need execution, locate the named execution owner rather than assuming Core executes it.

Recipe: [Core discovery](../recipes/core-discovery.md).

## 4. Shared framework-service consumer

Use Tainted Framework **only for the exact promoted service**.

A class or capability ID existing in Framework source is not enough.

Recipe: [Framework service gate](../recipes/framework-service.md).

## 5. AI-enabled feature

Your feature/provider owns domain truth.

Your AI package references Avalon AI Contracts and declares policy/goals/actions.

Avalon AI Runtime + the single FoA host own scheduling and bounded execution.

Recipe: [AI package](../recipes/ai-package.md).

## 6. Contract/provider ecosystem feature

Use Avalon Contracts when multiple mods need shared provider discovery/catalog/state/evidence/lifecycle semantics.

Provider gameplay truth stays in the provider.

Recipe: [Contracts provider/consumer](../recipes/contracts-provider.md).

## 7. External tooling / editor / local automation

Use Tainted Grail Extender + FOA-SDK when the client runs **outside** the BepInEx process.

Keep the loopback authenticated and service-specific.

Recipe: [External SDK tool](../recipes/external-sdk-tool.md).

## 8. Unknown game identity/owner

Use the Diagnostic Tool first.

Then move to `reference/`, `systems/` and `investigate/` before writing mutation code.

Recipe: [Diagnostic evidence → implementation](../recipes/diagnostic-to-implementation.md).
