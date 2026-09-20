# Managed Runtime and Proprietary Assembly Map

## What it is

FoA's proprietary systems are distributed across several managed assemblies rather than one monolithic engine DLL.

## Main assembly map

| Assembly | Major ownership |
| --- | --- |
| `TG.Main.dll` | main gameplay/runtime host, templates, scenes, MVC, items, story, Leshy integration |
| `Awaken.Utility.dll` | archive/file/buffer utilities, packed transforms, mipmap masters |
| `Awaken.ECS.dll` | Drake, Medusa, critters/flocks, ECS rendering and linked-entity support |
| `Awaken.Kandra.dll` | Kandra skinned/deforming renderer |
| `HLOD.dll` | HLOD runtime/controller/streaming |
| `Awaken.Babel.dll` | Babel localisation runtime |
| `Awaken.PackageUtilities.dll` | shared interfaces/config/lifetime support |
| `Awaken.Orchestrating.dll` | startup orchestrator |

## Why this matters

A system name tells you where ownership lives, but a mod may still cross assemblies.

For example:

- weapon gameplay lives largely in `TG.Main.dll`;
- rigid weapon presentation can enter `Awaken.ECS.dll` through Drake;
- character clothing presentation enters `Awaken.Kandra.dll`;
- localisation lookup enters `Awaken.Babel.dll`.

## Mono and IL2CPP

The public system maps here describe the researched Mono managed surfaces. IL2CPP builds require resolving the equivalent generated/interoperability surface for the installed build rather than assuming identical reflection/patch signatures.

## Deeper reference

- [Assembly boundaries and ownership](assembly-boundaries.md)

## Modding relevance

Use this assembly map to find the owning types before selecting a patch or reflection target.

## Related systems

- [Runtime orchestration](../runtime-orchestration/README.md)
- [Runtime lifetime](../runtime-lifecycle/README.md)
