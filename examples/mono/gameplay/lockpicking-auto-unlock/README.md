# Lockpicking Auto-Unlock

This example skips LockAction.OnStart only when:

~~~text
no LockpickingInteraction already exists
WillBeOpenWithKey == false
HeroCanLockpick == true
~~~

For that case it calls LockAction.Unlock(false), then CommitCrime.Lockpicking(...), and skips only the minigame start.

All other cases continue through vanilla LockAction.OnStart.

## Build

~~~powershell
dotnet build .\LockpickingAutoUnlock.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Auto-unlock without bypassing native lock rules](../../../../guides/tasks/gameplay/auto-unlock-with-native-gates.md)
