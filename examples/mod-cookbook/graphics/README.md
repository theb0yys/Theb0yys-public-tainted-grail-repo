# Graphics Cookbook

## Runtime visual ownership

[Skybox runtime ownership](../../proven-paths/05-skybox-runtime-ownership-mono/README.md)

Use this pattern when a visual mod needs to capture the current owner, apply an owned runtime replacement, and restore the original state cleanly.

Graphics recipes should preserve the game's real rendering owner instead of forcing unrelated global state.
