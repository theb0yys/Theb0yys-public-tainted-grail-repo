# Mono Rendering Examples

Use these examples for rendering changes where your mod temporarily owns presentation state.

The current example shows the full ownership pattern: capture the previous state, create only mod-owned resources, apply the change, restore the original state, and destroy only what your mod created.

- [Skybox ownership](skybox-ownership/README.md)
