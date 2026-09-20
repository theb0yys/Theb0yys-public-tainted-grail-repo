# Session-Only Survival

A complete session-only fatigue slice:

~~~text
movement proficiency events
→ fatigue rises
→ stage changes
→ non-saved stamina/sprint StatTweaks

safe RestPopupUI.SkipWeatherTime
→ fatigue falls

IMGUI overlay
→ shows current session state
~~~

Nothing is written into FoA save serialization.

## Build

~~~powershell
dotnet build .\SessionSurvival.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Prototype a session-only survival system](../../../../guides/tasks/gameplay/prototype-a-session-only-survival-system.md)
