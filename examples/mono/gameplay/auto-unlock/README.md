# Auto-Unlock with Native Gates

This example skips only the normal lockpicking interaction after FoA says the lock is lockpickable.

It preserves:

- key-opened cases through WillBeOpenWithKey;
- native lock eligibility through HeroCanLockpick;
- native unlock through LockAction.Unlock(false);
- lockpicking crime through CommitCrime.Lockpicking(...).

## Build

~~~powershell
dotnet build .\AutoUnlock.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

If any private member cannot be resolved, the patch returns to vanilla behavior.

Guide: [Auto-unlock with native gates](../../../../guides/tasks/gameplay/auto-unlock-with-native-gates.md)
