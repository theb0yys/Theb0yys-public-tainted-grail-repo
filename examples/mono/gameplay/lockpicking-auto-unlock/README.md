# Lockpicking Auto Unlock

Skip only the ordinary lockpicking interaction after FoA confirms the hero can lockpick the target and the target is not key-opened.

~~~text
LockAction.OnStart
→ HeroCanLockpick
→ WillBeOpenWithKey
→ LockAction.Unlock(false)
→ CommitCrime.Lockpicking(...)
~~~

~~~powershell
dotnet build .\LockpickingAutoUnlock.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

If a required private member is unavailable, the patch returns to vanilla behavior.

Guide: ../../../../guides/tasks/gameplay/auto-unlock-with-native-gates.md
