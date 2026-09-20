---
document_type: framework
scope: shared UI responsibility split
last_verified: 2026-09-20
---

# Shared UI Responsibility Split

The private ecosystem converged on four separate owners.

| Responsibility | Project owner |
| --- | --- |
| stable UI surface/asset/capability contracts | Avalon Core |
| input/cursor/controller/world-freeze scope | FoA Mod Manager |
| shared visual assets/styles/rendering helpers | Tainted Interface |
| feature state, commands, save/config and feature validation | each feature mod |

This separation prevents a settings manager from becoming every feature screen, or a visual framework from owning gameplay state.

## Surface types

Useful conceptual categories include:

- passive HUD — no cursor/freeze;
- interactive overlay — cursor/input ownership, usually no freeze;
- modal panel — cursor/input ownership, freeze normally true;
- full custom screen — full shell/back/cleanup contract;
- native-anchored screen — only after native target evidence.

A feature should request only the global ownership it actually needs.
