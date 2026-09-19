# 25 — Motion Blur Toggle

**Category:** graphics / comfort  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example demonstrates a small graphics-setting override.

Targets:

~~~text
MotionBlurSetting.Enabled
MotionBlurSetting.Intensity
~~~

Safety default:

~~~text
[Graphics]
DisableMotionBlur = false
~~~

When enabled, the example returns false from the native Enabled getter and zero from the native Intensity getter.

## Build

~~~powershell
dotnet build .\MotionBlurExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This is preferable to scanning HDRP volume objects every frame when the game already exposes a setting owner.

The public rewrite is **NOT_RUN**.
