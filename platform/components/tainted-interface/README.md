# Tainted Interface

Use Tainted Interface when your mod needs shared UI styles, textures, icons, HUD helpers, or other common visual resources.

These UI resources are intended for ordinary mod authors. Consume the public semantic APIs instead of depending on the package's internal file layout.

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
