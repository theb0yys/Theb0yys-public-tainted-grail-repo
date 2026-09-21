# Tainted Interface

**Status:** Ready for shared UI styles and resources.

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

Semantic IDs let the UI package change internally without forcing dependent mods to hardcode asset paths.

See:

- [Semantic assets](semantic-assets.md)
- [UI ownership](ui-ownership.md)
