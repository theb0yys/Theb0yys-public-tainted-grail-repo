# 07 — Hero Footstep Beep Replacement

**Category:** audio / replacement  
**Source-path evidence:** RUNTIME_EVIDENCED  
**This rewritten public example:** NOT_RUN

This is intentionally silly but real: when enabled, hero footstep FMOD events are intercepted and replaced by a tiny generated beep.

No sound assets are included.

## Why this example exists

It teaches the exact safe replacement rule:

1. filter to the event owner you intend to replace;
2. successfully start your replacement;
3. only then suppress the native call;
4. if replacement fails, let the native call continue.

The filter is VHeroFootsteps on the exact FMODManager.PlayOneShot overload used by the owner-side replacement path.

## Build

~~~powershell
dotnet build .\FootstepBeepExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example defaults to disabled. Set General.Enabled=true in its generated config.

## Next iteration

Replace the generated tone with your own licensed clips, then use the FMOD parameters to map FoA surfaces rather than replacing every hero step with one sound.
