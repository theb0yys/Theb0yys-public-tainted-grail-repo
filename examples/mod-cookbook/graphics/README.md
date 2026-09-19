# Graphics Cookbook

## Fog and atmosphere

- [HDRP / FoA fog control](FOG_CONTROL.md) — control the existing active Volume/fog owners and restore them cleanly.

## Generic visual ownership

- [Skybox runtime ownership](../../proven-paths/05-skybox-runtime-ownership-mono/README.md) — capture, replace and restore an owned runtime visual state.

The common rule is to identify the game's actual rendering owner instead of forcing unrelated global settings.
