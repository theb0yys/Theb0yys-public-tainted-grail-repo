# Persistence Architecture

FoA's native save system owns game state. Mods should not replace that ownership casually.

## Public handbook rule

Use the smallest persistence owner that matches the data:

- **game-owned state** — keep it on the game's native lifecycle;
- **mod configuration** — use BepInEx configuration;
- **mod-owned transient runtime state** — rebuild it from current game state when possible.

A runtime hook firing successfully is not a reason to write save data.

## Save-aware mod design

When a feature touches durable game state:

1. use the native game operation that normally owns that state;
2. avoid duplicating the same state in a second owner;
3. make repeated execution idempotent where possible;
4. keep unload/disable behaviour from corrupting native state.

For concrete save lifecycle surfaces, see [Saving and Persistence](../world/saving-persistence.md).
