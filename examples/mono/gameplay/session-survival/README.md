# Build a Simple Session-Only Fatigue System

This example adds a small fatigue mechanic without changing Tainted Grail's save format.

Movement raises fatigue. Rest lowers it. Higher fatigue can temporarily make stamina use more expensive.

Everything resets when the session ends.

## Build it

~~~powershell
dotnet build .\SessionSurvival.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try it in game

1. load a save;
2. move around enough to generate movement/proficiency activity;
3. watch the fatigue display rise;
4. confirm the stamina/sprint effect changes at the configured stage;
5. rest safely;
6. confirm fatigue falls;
7. restart and confirm the session-only state is reset.

## What to change first

Change one fatigue threshold or one stamina multiplier.

Do not add saving/persistence until the session version behaves the way you want.

## How it works

The mod reuses events Tainted Grail already produces.

Movement activity is observed through the game's proficiency event.

Rest is observed through:

~~~text
RestPopupUI.SkipWeatherTime
~~~

Temporary gameplay effects are attached to the player's current stamina-related stats.

Those effects are marked not saved.

The overlay only displays the mod's current fatigue value; it is not where the fatigue state is stored.

## Next

[Read the session survival guide](../../../../guides/tasks/gameplay/prototype-a-session-only-survival-system.md)
