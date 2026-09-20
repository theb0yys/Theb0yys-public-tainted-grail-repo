# Runtime Reference

Use this page when you need to know whether a technique or API applies to Mono, IL2CPP, or both.

The runtime matters because assembly access, interop, hook targets, object lifetime, and available loader APIs can differ even when the gameplay feature looks the same.

## Evidence lanes

Keep these lanes explicit:

| Lane | Publicly demonstrated surface |
| --- | --- |
| Mono | Managed FoA mods using BepInEx/Harmony and direct managed game types |
| IL2CPP managed/interop | FoA IL2CPP packages that depend on generated interop metadata/assemblies |
| IL2CPP native | Public native mods using inline detours and direct IL2CPP memory access without BepInEx/Harmony |
| Merlin | Questline's public Unity authoring toolkit with compatible/simplified game scripts, templates and replaceable Addressables |

A finding in one lane is not automatically an equivalent API in another.

## FoA-specific examples

- A public Mono recipe mod uses managed hero/item state and performs a `KnownItems` backfill after the player loads.
- A public IL2CPP native implementation uses `HeroItems.OnRestore` to capture a valid owner pointer and calls `HeroRecipes.LearnRecipe` for existing/native recipes.
- That same native implementation documents stale/freed pointers while traversing `CraftingTemplate.recipes`, illustrating that IL2CPP-native access adds memory-lifetime hazards absent from ordinary managed references.
- Questline describes Merlin's Workshop as a toolkit for replacing Addressables assets and ships a simplified compatible source surface plus selected templates/assets. Merlin evidence is therefore valuable for names, relationships and authoring surfaces, but should not silently be treated as proof that every shipped-game runtime implementation is identical.

## Exact Mono evidence baseline

One inspected Mono environment is pinned strongly enough for patch-sensitive static reference:

- Steam build `24270691`;
- `TG.Main.dll` SHA-256 `749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982`;
- MVID `68528841-991C-481E-BD94-7F1776FC3579`;
- BepInEx `5.4.23.3`;
- Harmony `2.9.0.0`;
- Unity `6000.0.41.4645959`.

Static/decompiled facts explicitly tied to this baseline should not be silently generalized to other Mono builds or IL2CPP.

See [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).

## Version discipline

Record runtime lane alongside game/build evidence. For IL2CPP work, generated/interop surfaces may need regeneration after game updates; native offsets and detour targets require their own revalidation.

Do not assume a result established in Mono transfers to IL2CPP or Merlin without evidence.
