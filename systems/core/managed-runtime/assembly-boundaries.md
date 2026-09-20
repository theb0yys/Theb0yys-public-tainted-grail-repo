# Assembly Boundaries and Ownership

Use this page when you know a FoA subsystem but need to find the managed assembly that owns the relevant type.

Canonical map: [Managed Runtime and Proprietary Assembly Map](README.md).

## Ownership is often cross-assembly

A feature may cross several assemblies while retaining distinct owners.

Example:

```text
weapon definition / equip / combat
  TG.Main.dll
        ↓
rigid presentation
  Awaken.ECS.dll / Drake
        ↓
shared texture demand
  Awaken.Utility.dll
```

Do not select a patch target merely because a nearby type exists in the first assembly you searched.

## Search order

For a new system question:

1. identify the gameplay or presentation responsibility;
2. start in the assembly that normally owns that responsibility;
3. follow explicit type references/calls into supporting assemblies;
4. record each boundary;
5. keep runtime ownership separate from helper/infrastructure ownership.

## Common mistakes

- treating `Awaken.Utility.dll` helpers as the gameplay owner;
- treating a renderer assembly as the item/combat owner;
- assuming a type name implies the whole subsystem lives in one DLL;
- copying a Mono assembly target directly into IL2CPP without resolving the generated interop surface.

## Version scope

For static claims, record the inspected assembly hash/build where practical.

For IL2CPP, the equivalent runtime type may be exposed through generated interop assemblies and may not preserve reflection/patch behavior exactly.

See [Runtime compatibility](../../../tooling/ecosystem/runtime-compatibility.md).
