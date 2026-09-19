# 20 — Camera Shake Strength

**Category:** camera / comfort  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example scales FoA's native camera-shake call rather than replacing the camera system.

Target:

~~~text
Awaken.TG.Main.Cameras.GameCamera.Shake(...)
~~~

## What it changes

The prefix scales amplitude and frequency, and reduces duration to zero when strength is effectively zero.

Config:

~~~text
[Camera]
ShakeStrength = 0.5
~~~

Values: 0 suppresses the shake, 0.5 is half strength, 1 is vanilla, and values up to 2 are allowed for experimentation.

## Build

~~~powershell
dotnet build .\CameraShakeExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This keeps the game's existing shake event route and changes only parameters at the call boundary.

Do not assume every visual jolt is produced by GameCamera.Shake. Test the exact event you care about.

The public rewrite is **NOT_RUN**.
