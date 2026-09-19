# 01 — Stamina Drain Example

**Category:** player stats / stamina  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This is a real FoA-target example. It changes two runtime hero stats:

- CharacterStats.SprintCostMultiplier
- CharacterStats.StaminaUsageMultiplier

It applies non-saved StatTweak elements after CharacterStats initialization and reapplies when config changes.

## What to try

Set:

~~~text
[Sprinting]
Multiplier = 0.5

[Actions]
Multiplier = 1
~~~

That asks for half sprint stamina use while leaving the broader action scalar at vanilla.

A value of 1 means no change. The example clamps values to 0 through 1.

## Build

~~~powershell
dotnet build .\StaminaDrainExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Add -p:DeployOnBuild=true to copy the DLL to BepInEx/plugins/TGExample.StaminaDrain.

## Why this is a good pattern

- modifies runtime stats instead of editing saves;
- marks its tweaks not-saved;
- reuses the game's StatTweak system;
- removes its own tweaks when returning to vanilla/unloading;
- changes only the current hero.

## Evidence warning

The owner-side path has target/decompilation/build/deploy evidence, but the inspected records still required in-game sprint/action validation. Treat this example as a real target path that still needs feature testing.

Test with a visible stamina bar before making release claims.
