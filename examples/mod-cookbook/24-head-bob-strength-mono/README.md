# 24 — Head Bob Strength

**Category:** camera / comfort  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example scales FoA's native head-bob intensity setting.

Target:

~~~text
HeadBobbingSetting.Intensity
~~~

Config:

~~~text
[Camera]
HeadBobStrength = 0.5
~~~

Values:

- 0 = no supported head bob;
- 0.5 = half;
- 1 = vanilla;
- up to 2 = stronger for experimentation.

## Build

~~~powershell
dotnet build .\HeadBobExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This changes only the value returned by the native setting getter.

The public rewrite is **NOT_RUN**.
