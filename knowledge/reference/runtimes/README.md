# Runtime Reference

Exact lookup for FoA runtime lanes and their modding implications.

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

## Version discipline

Record runtime lane alongside game/build evidence. For IL2CPP work, generated/interop surfaces may need regeneration after game updates; native offsets and detour targets require their own revalidation.

Do not assume a result established in Mono transfers to IL2CPP or Merlin without evidence.
