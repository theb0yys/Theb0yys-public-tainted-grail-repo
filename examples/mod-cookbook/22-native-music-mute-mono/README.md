# 22 — Native Music Mute

**Category:** audio / native music  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example demonstrates FoA's native music-start interception seam.

Targets:

~~~text
AudioCore.PlayExplorationMusic
AudioCore.PlayAlertMusic
AudioCore.PlayCombatMusic
~~~

The example does not ship or play replacement music.

Safety default: MuteNativeMusic=false.

When enabled, the prefix skips new native music starts. It does not stop a track that was already playing before the condition became active.

## Build

~~~powershell
dotnet build .\NativeMusicMuteExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

A full replacement system additionally needs a context router, owned playback, unique/cinematic allowlists, already-active native emitter handling, dialogue/combat behavior, and asset/licence validation.

The public rewrite is **NOT_RUN**.
