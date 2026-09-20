# Play Different Music in Different Situations

This example plays music owned by the mod and switches between two simple music states.

It ships with no music files. Instead, it generates two short test tones in memory so you can build and run the project immediately.

## Build it

~~~powershell
dotnet build .\ContextualMusic.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

The example uses one tone for open-world play and another for interiors.

Move between those situations and listen for the crossfade.

Enter combat and confirm the **mod's music** becomes quieter according to the example's ducking rule.

## What to change first

Replace one generated test tone with your own WAV-loading code.

Keep the lane-switching and cleanup code unchanged until one real track plays correctly.

## How it works

The mod uses FMOD Core, the same audio technology used by the game.

The basic flow is:

~~~text
read current game situation
→ choose one music lane
→ create/play a mod-owned sound
→ fade its volume in
→ fade the old lane out
→ release the old sound when silent
~~~

The example changes only audio channels it created itself.

It does not globally mute game audio and it does not suppress the game's own music. Native-music suppression is a separate feature you should add only when you specifically need it.

## Next

[Read the contextual music guide](../../../../guides/tasks/audio/build-a-contextual-music-mod.md)
