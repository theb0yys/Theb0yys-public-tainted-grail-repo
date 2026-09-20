# Managed Assemblies and System Ownership

> **Reference page.** Use this when you know the game system you want to inspect but not which managed assembly owns it.

## What this system is

On the supported Mono branch, FoA's managed code is split across several assemblies.

A critical research finding is:

> **`TG.Main.dll`, not `Assembly-CSharp.dll`, is the main Questline game/runtime host.**

`Assembly-CSharp.dll` exists, but the major FoA gameplay, templates, scenes, MVC, items, Story, UI and several proprietary integration surfaces live elsewhere.

## Who owns it in FoA

The inspected Mono-build owner map is:

| Assembly | Main ownership |
|---|---|
| `TG.Main.dll` | main gameplay/runtime host: MVC, templates, scenes, items, hero/equipment, Story, UI, much AI/gameplay, Leshy integration |
| `Awaken.Utility.dll` | shared archive/file/buffer/math/packed-transform/mipmap primitives |
| `Awaken.ECS.dll` | Drake/MergedDrake, Medusa, ECS bootstrap, flocks/critters and ECS/rendering systems |
| `Awaken.Kandra.dll` | Kandra skinned/deforming renderer, mesh registration, streaming and GPU data |
| `HLOD.dll` | HLOD controller/runtime/streaming/FSM |
| `Awaken.Babel.dll` | Babel localisation runtime and providers |
| `Awaken.PackageUtilities.dll` | shared interfaces/config/lifetime contracts used by proprietary systems |
| `Awaken.Orchestrating.dll` | startup orchestrator entry wrapper |
| `Assembly-CSharp.dll` | project/vendor support surface; Rewired/UI/VFX/support types, not main FoA engine body |

## Important identities, types, and methods

### TG.Main.dll

High-value namespaces/systems include:

- `Awaken.TG.MVC`
- `Awaken.TG.MVC.Domains`
- `Awaken.TG.Main.Templates`
- `Awaken.TG.Main.Heroes.Items`
- `Awaken.TG.Main.Heroes.Combat`
- `Awaken.TG.Main.Locations`
- `Awaken.TG.Main.Stories`
- `Awaken.TG.VisualScripts`
- `Awaken.TG.LeshyRenderer`

Key owners documented elsewhere in the handbook:

- `SceneService`
- `TemplatesLoader`
- `TemplatesProvider`
- `World`
- `Item`
- `ItemTemplate`
- Story runtime
- hero/inventory/shop/location systems

### Awaken.Utility.dll

Notable primitives:

- archive mount/path adjustment;
- typed file readers;
- file writer;
- sequential buffer reader;
- packed transforms/matrices;
- shared mipmap streaming managers.

### Awaken.ECS.dll

Contains:

- Drake authoring/runtime components;
- Drake renderer systems;
- Medusa renderer;
- ECS bootstrap;
- ECS/linked entity components;
- additional high-volume rendering systems.

### Awaken.Kandra.dll

Contains:

- `KandraRenderer`
- `KandraMesh`
- `KandraRendererManager`
- mesh manager/streaming manager
- packed/compressed skinned-mesh data structures

### HLOD.dll

Contains:

- `HLODManager`
- `AddressableHLODController`
- `AddressablesResourcesLoader`
- `HLODLoadManager`
- HLOD tree/FSM logic

### Awaken.Babel.dll

Contains:

- `BabelManager`
- `BabelPersistence`
- streaming/preloaded providers

## Where it exists in the lifecycle

Assembly ownership is a **discovery and compatibility** concern.

The normal research path is:

~~~text
feature/domain question
→ identify native owner
→ identify assembly
→ inspect exact type/method
→ fingerprint version/build
→ map lifecycle
→ design hook/API
~~~

Do not start by scanning every assembly for vaguely matching names.

## How we interact with it

### Reference the minimum local assemblies

A mod project should reference only the local assemblies needed for its exact feature.

### Keep game assemblies local

Do not redistribute FoA/Unity/BepInEx binaries through this public repository.

### Bind private hooks to the owning assembly version

If a patch/reflection path depends on a private member, the assembly identity is part of the compatibility contract.

### Re-check after game updates

A matching type name does not prove identical method semantics.

## Why this route

Assuming "Unity game = Assembly-CSharp.dll" wastes research and can produce completely wrong conclusions for FoA.

The decompilation inventory showed that Questline's proprietary systems are intentionally separated into Awaken/TG assemblies.

Knowing the owner assembly dramatically narrows:

- decompilation;
- dependency references;
- hook search;
- compatibility checks;
- reverse-engineering scope.

## What goes wrong

- looking for FoA gameplay internals only in `Assembly-CSharp.dll`;
- referencing dozens of game DLLs unnecessarily;
- treating support/package DLLs as gameplay owners;
- copying an IL2CPP generated assembly assumption onto Mono or vice versa;
- keeping a reflected target after its assembly changed without revalidation;
- assuming proprietary renderer data belongs to `TG.Main.dll` when the owner is Kandra/ECS/HLOD.

## How to verify

For a native target, record:

- assembly name;
- game build/runtime lane;
- SHA-256 or MVID when the claim is patch-sensitive;
- fully qualified type;
- exact member/signature;
- owner lifecycle;
- public/private status;
- evidence state.

## Exact inspected Mono baseline

The strongest current assembly-owner map is bound to this recorded Mono environment:

| Field | Value |
| --- | --- |
| Steam build | `24270691` |
| Runtime | Mono |
| `TG.Main.dll` SHA-256 | `749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982` |
| `TG.Main.dll` MVID | `68528841-991C-481E-BD94-7F1776FC3579` |
| `TG.Main.dll` size | `9,058,304` bytes |
| BepInEx | `5.4.23.3` |
| Harmony | `2.9.0.0` |
| Unity | `6000.0.41.4645959` |

The recorded managed inventory also fingerprints the owner assemblies used by the map above. These identifiers are compatibility evidence for the inspected Mono build; they are not claims about later builds or IL2CPP interop assemblies.

See [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).

## Current proof boundary

This page reflects a specific inspected Mono build and the current research corpus.

It is a routing map—not a promise that every assembly/type layout is identical in later Mono builds or IL2CPP generated interop.
