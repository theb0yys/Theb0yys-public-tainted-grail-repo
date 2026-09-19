# 18 — Personal Helper Light

**Category:** environment / lighting  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This is an asset-free example of a mod-owned HDRP point light.

It is derived from the helper-light mechanism used in the maintainer's torch work, but it does **not** copy torch detection, item matching, offsets, balance or feature design.

## What it does

- creates one plug-in-owned point light;
- uses HDRP \`HDAdditionalLightData\`;
- follows the current main camera;
- can be toggled with a key;
- exposes range and lumen intensity in config;
- destroys only the object it created when unloaded.

Default toggle key: \`F7\`.

## Build

~~~powershell
dotnet build .\PersonalHelperLightExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Optional deploy:

~~~powershell
dotnet build .\PersonalHelperLightExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA" -p:DeployOnBuild=true
~~~

## Why this is useful

From here a modder can iterate toward:

- a lantern;
- a spell light;
- a photo-mode light;
- an accessibility light;
- an equipment-gated helper light.

If you make it item/equipment dependent, research the exact item/equip lifecycle instead of scanning the whole scene every frame.

## Evidence warning

The source helper-light path was built/deployed in the maintainer workspace, but its final visual comparison matrix remained incomplete.

The public rewrite above is **NOT_RUN**.
