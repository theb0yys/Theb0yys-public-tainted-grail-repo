# Hero Footstep Replacement

This example uses the same FMOD interception shape as a working custom-footstep mod.

Instead of shipping audio files, the example generates a simple beep so the repository remains source-only.

## Owner

The hook observes FMOD one-shot playback and filters to the hero footstep owner before replacing playback.

## Build

~~~powershell
dotnet build .\FootstepBeepExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Pattern

1. intercept the native one-shot call;
2. confirm the debug/source object is the hero footstep owner;
3. select the replacement sound for the reported surface/context;
4. suppress the matching native footstep only when the replacement will play;
5. leave every unrelated FMOD event alone.

For a reusable generic gate, see [Audio replacement gate](../audio-replacement-gate/README.md).
