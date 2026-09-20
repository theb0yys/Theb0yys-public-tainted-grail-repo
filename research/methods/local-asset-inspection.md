---
document_type: investigation
scope: local asset, prefab, scene and serialized-object inspection
last_verified: 2026-09-20
---

# Local Asset Inspection

Use local asset inspection when source/static code cannot answer how a FoA object is assembled, referenced, or serialized.

Examples include questions about:

- prefab/component structure;
- scene ownership;
- ScriptableObject relationships;
- serialized references;
- addressable/resource identity;
- visual or audio authoring inputs.

Tools such as AssetRipper may be useful locally, but extracted commercial material remains outside the public repository.

## What this evidence can establish

For the exact inspected build, local asset inspection may establish:

- that an object/resource exists;
- its serialized structure;
- component/type relationships;
- serialized references between objects;
- scene/prefab membership;
- candidate data consumed by a native owner.

## What it cannot establish by itself

Asset inspection does not prove:

- that the resource is loaded in the tested runtime path;
- that a serialized field is semantically authoritative;
- that a component executes;
- that replacing an asset is safe;
- persistence behaviour;
- compatibility with another build.

Treat asset structure as its own evidence lane.

## Procedure

1. Record the exact game build and runtime lane.
2. Define the asset question narrowly.
3. Extract or inspect only the local material required for that question.
4. Record the minimum stable identities needed to reproduce the finding.
5. Map the serialized object/component back to the owning code path where possible.
6. Use runtime observation if actual loading/execution remains unknown.
7. Publish conclusions, not the extracted corpus.

## Public-safe recording

Prefer a note such as:

```text
Build: <identified build>
Observed locally: <prefab/scene/resource identity>
Contains: <relevant component/type relationship>
Static owner candidate: <type/service>
Runtime use: NOT_RUN
Persistence effect: NOT_RUN
```

Avoid copying:

- full serialized object dumps;
- textures, meshes, audio or animation data;
- bulk localization;
- prefab/scene exports;
- proprietary binary or generated asset bundles.

A short identifier or structural statement should be included only when it is necessary to reproduce or understand the technical conclusion.

## Cross-lane rule

Use the smallest combination of evidence required:

```text
code/static inspection
→ who could own/use it

asset inspection
→ what serialized structure exists

runtime observation
→ what actually loads/executes

persistence test
→ what survives save/reload
```

One lane must not silently substitute for another.
