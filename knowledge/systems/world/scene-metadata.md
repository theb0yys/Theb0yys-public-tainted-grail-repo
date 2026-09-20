---
document_type: system
scope: scene metadata useful for read-only context
runtime: mono
evidence:
  static: DECOMPILED
last_verified: 2026-09-20
---

# Scene Metadata

FoA exposes scene/world context through `SceneService` and scene configuration data.

Useful read-only context includes:

- `ActiveSceneRef`;
- `ActiveSceneDisplayName`;
- `IsOpenWorld`;
- `IsAdditiveScene`;
- `IsPrologue`.

`SceneConfigs` / `SceneData` provide corresponding scene metadata including open-world, additive, prologue and Wyrdnight-related flags.

## Use

This metadata can gate passive features such as:

- interior-only overlays;
- diagnostics;
- scene-context rules;
- display labels.

## Do not infer too much

`IsOpenWorld == false` is a useful first-pass **interior** gate. It is not proof that every such scene is a “dungeon” or that a particular entrance/exit owner has been identified.

Read scene context without jumping directly to `SceneService.ChangeTo` or player teleport calls.
