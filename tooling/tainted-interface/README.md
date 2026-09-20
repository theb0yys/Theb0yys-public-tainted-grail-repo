# Tainted Interface

**Posture: Author-ready for shared UI styles/resources**

**Obtain:** https://www.nexusmods.com/taintedgrailthefallofavalon/mods/190

**Version signal:** current public plugin declaration is `0.3.4`.

**Stability boundary:** semantic style/icon/texture/catalog APIs are the supported surface. Raw embedded-pack paths, arbitrary full-pack access and undocumented menu-registration behavior are not compatibility contracts.

See [distribution/versioning](../ecosystem/distribution-and-versioning.md) and [API stability](../ecosystem/api-stability.md).

Use Tainted Interface when your mod needs:

- shared IMGUI styles;
- render-only styles for passive HUDs;
- semantic texture/icon lookup;
- item icon lookup;
- curated UI pack resources;
- common HUD badge helpers;
- optional custom-UI scope helpers.

The public API is:

`TaintedInterface.TaintedInterfaceApi`

## Prefer semantic IDs

Do not read raw asset-pack folders from feature mods.

Use:

- `GetUiTexture(textureId)`
- `GetIcon(iconId)`
- `GetItemIcon(itemReference)`
- catalog/descriptor APIs when you need discovery.

This lets the shared UI owner change packaging while consumers keep stable semantic references.

See:

- [Semantic assets](semantic-assets.md)
- [UI ownership](ui-ownership.md)
