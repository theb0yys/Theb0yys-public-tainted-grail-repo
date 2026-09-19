# UI & HUD Cookbook

UI/HUD covers native HUD decisions, custom overlays, menus, settings surfaces and input/cursor ownership.

## Native HUD

- [05 Force hero HUD bars visible](../05-force-hero-hud-mono/README.md)
- [27 Simple damage numbers](../27-simple-damage-numbers-mono/README.md)

Important evidence distinction:

- the HUD/damage hook can have load or runtime precedent;
- the exact public visual behaviour still needs its own execution proof.

The current simple damage-number public example is **NOT_RUN**. Patch registration or a working damage observer does not prove its visual feed.

## Custom HUD and overlays

A custom HUD owns presentation but should not silently take gameplay, cursor or modal-input authority.

Prefer:

~~~text
read game state
    ↓
build a bounded presentation model
    ↓
draw/update owned UI
    ↓
release owned resources cleanly
~~~

Do not mutate health/mana/stamina merely to keep a display synchronized.

## Menus and action panels

The maintainer corpus has working UI precedents for:

- bounded item-browser/action panels;
- companion command panels;
- native bonfire service routing;
- shared custom-UI scope.

These are stronger foundations for future public menu recipes than raw unscoped IMGUI windows.

## Input and cursor ownership

A modal custom screen needs an explicit owner for:

- cursor visibility and lock state;
- gameplay input suppression;
- close/Esc handling;
- controller navigation where supported;
- teardown/restoration when the screen closes or the plugin unloads.

A screen that merely renders successfully is not enough.

## Settings surfaces

Use ordinary BepInEx config for mod settings unless a stronger native/shared settings route is intentionally required.

FoA Mod Manager / shared UI integration is a systems concern as well as a UI concern; see [Systems](../systems/README.md).
