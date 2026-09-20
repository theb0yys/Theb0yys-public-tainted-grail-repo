# Merlin Content Overlay Boundary

This example explains the authoring/runtime separation used by the public Merlin starters.

Copyable starter: [Merlin basic overlay](../../templates/merlin/basic/).

## Authoring side

The Merlin/Unity side owns authored content under a mod-owned content root.

Typical responsibility:

```text
Assets/YourMod/
→ authored data/prefabs/scenes supported by Merlin
→ Merlin/Unity build/export lifecycle
```

## Runtime side

If the mod also needs BepInEx runtime logic, keep that in a separate runtime project.

Do not add a project reference from the Merlin Unity project to the BepInEx runtime project merely to share constants.

Instead, define a small stable identity contract between the two lifecycles.

Reference starter: [Mono + Merlin hybrid](../../templates/hybrid/mono-merlin/).

## Why the boundary matters

Merlin authoring success does not prove:

- BepInEx runtime behavior;
- custom native template registration;
- persistence;
- IL2CPP compatibility;
- release packaging.

Likewise, a runtime plugin loading does not prove the authored Merlin content was exported/mounted correctly.

## Verification

Validate separately:

1. authored content root is accepted by the Merlin project;
2. export/build completes;
3. runtime package discovers the intended authored identity when required;
4. runtime integration executes;
5. cleanup/persistence/release claims are tested independently.

Official Merlin source material is indexed under [Official Sources](../../sources/official/README.md).
