# Change Map Fog Display Without Rewriting Discovery

Change the Character Sheet map presentation while leaving MapMemory.visitedPixels untouched.

Working lineage: [Map Fog: Display Without Rewriting Discovery Memory](../../../research/case-studies/map/map-fog-display-only.md).

## Runnable source

Start from the buildable example: [Map fog display](../../../examples/mono/ui/map-fog-display/README.md). Build it unchanged first, confirm the documented behavior, then make one change at a time.


## Native discovery memory

FoA persists map discovery in:

~~~text
Awaken.TG.Graphics.MapServices.MapMemory.visitedPixels
~~~

Do not write that collection for a display-only mod.

## Disable the map mask

The working No Map Fog implementation patches:

~~~text
Awaken.TG.Main.Heroes.CharacterSheet.Map.MapUI.ToggleFogOfWar()
~~~

with a prefix.

When the feature is enabled:

~~~csharp
MapFogState.TrySetFogEnabled(false);
__result = false;
return false;
~~~

That stops the map UI from re-enabling the fog presentation.

## Skip mask creation

It also patches:

~~~text
Awaken.TG.Graphics.MapServices.FogOfWar.CreateMaskTexture()
~~~

When the display mask is disabled:

~~~csharp
MapFogState.TrySetFogEnabled(false);
__result = null;
return false;
~~~

This is presentation suppression. It does not mark the world as visited.

## Reveal map markers separately

Map markers use:

~~~text
FogOfWar.IsPositionRevealed(...)
~~~

The optional marker-reveal patch returns true from that presentation query when configured.

Keep this separate from native discovery memory and fast-travel eligibility.

## Why the distinction matters

The map can visually show more while all of these remain native:

- visitedPixels;
- location discovery;
- quest discovery;
- fast-travel unlock rules;
- save state.

Do not use this feature to write fake visited pixels.

## Disable/rollback

When the mod is disabled, stop suppressing the three presentation paths and let the native MapUI/FogOfWar code run normally again.

No save migration is required because the mod never changed MapMemory.visitedPixels.
