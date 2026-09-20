# Skip the Lockpicking Minigame Safely

This example automatically opens an ordinary lock **only when the game already says the player is allowed to pick it**.

It does not turn every locked object into an unlocked object.

## Build it

~~~powershell
dotnet build .\AutoUnlock.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Copy the built DLL into its own folder under BepInEx\plugins.

## Try it in game

Use a disposable test save.

Try several different locks:

1. a normal lock you can pick;
2. a lock that opens with a key;
3. a lock you are not allowed to pick;
4. a lock where picking it counts as a crime.

The normal lock should skip the minigame. The other cases should continue using the game's normal rules.

## What to change first

The easiest first change is the on/off config value in Plugin.cs.

Do not start by changing the unlock method itself.

## How it works

When you interact with a lock, Tainted Grail runs LockAction.OnStart.

Before skipping that interaction, this example asks the same lock object:

- HeroCanLockpick — is this a lock the hero may pick?
- WillBeOpenWithKey — is this meant to open with a key?

Only if the lock passes those checks does the example call the lock's own:

~~~text
LockAction.Unlock(false)
~~~

It then calls the game's normal lockpicking-crime route:

~~~text
CommitCrime.Lockpicking(...)
~~~

If the mod cannot find one of the required game methods, it leaves the normal lockpicking behavior alone.

## Next

[Read the full auto-unlock guide](../../../../guides/tasks/gameplay/auto-unlock-with-native-gates.md)
