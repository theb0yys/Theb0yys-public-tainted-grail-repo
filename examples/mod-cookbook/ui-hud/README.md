# UI & HUD Cookbook

## Runtime overlay ownership

[Runtime UI overlay](../../proven-paths/03-runtime-ui-overlay-mono/README.md)

A custom overlay should read game state, draw only its owned presentation and release its UI/input state cleanly.

Keep cursor/input ownership explicit for modal screens. Passive HUD overlays should not take modal input ownership.
