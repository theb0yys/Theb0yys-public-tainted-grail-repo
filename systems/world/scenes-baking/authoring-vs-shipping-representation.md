# Authoring Scene vs Shipping Representation

Use this page when a Unity scene object found in authoring/source does not exist in the same shape at runtime.

Canonical overview: [Scenes Baking](README.md).

## Build-time transformation is significant

Questline's scene build can:

- split static/dynamic content;
- flatten hierarchies;
- generate static scenes;
- hand content to Leshy/HLOD/Medusa/Drake;
- rewrite Addressables ownership;
- save generated outputs.

Therefore:

```text
authoring GameObject path
≠
guaranteed shipping runtime path
```

## Runtime investigation rule

When starting from an authoring object:

1. identify whether it is static/dynamic;
2. identify which build processor owns it;
3. determine the resulting shipping representation;
4. find the runtime owner of that representation;
5. validate the mapping in the target build.

## Stable evidence

Useful stable links can include:

- template/native identity;
- scene identity;
- build-generated stable object identity where proven;
- mesh/material/resource identity;
- owning renderer/service.

A raw hierarchy string alone is fragile when scene baking rewrites structure.

## Modding consequence

Patch/runtime code should target the shipping owner.

Editor/Merlin tooling should target the authoring representation only when that is the intended surface.

Keep those lanes separate in documentation and validation.
