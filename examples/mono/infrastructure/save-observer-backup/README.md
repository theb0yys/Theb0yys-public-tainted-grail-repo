# Run Code After Tainted Grail Finishes Saving

This project demonstrates two related ideas:

1. detect when the game finishes writing a save slot;
2. optionally copy that completed slot into a separate ZIP owned by the mod.

If you only need a save-completion notification, use the observer part and leave the backup feature disabled.

## Build it

~~~powershell
dotnet build .\SaveObserverBackup.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Try the observer

Load a disposable save and make the game save normally.

Watch the BepInEx log.

The example should receive the completed slot ID **after** the game calls its normal save-completion method.

The Harmony patch itself only puts that slot ID into a queue. The main plug-in processes it later, so file work is not performed inside the save callback.

## How the observer works

The game has several CloudService implementations depending on platform/configuration.

The example looks for EndSave(string) on the supported concrete implementations and patches whichever ones exist.

The string passed to that method identifies the completed slot.

## Optional backup

When the backup part is enabled, the queued slot is read through Tainted Grail's own CloudService API:

~~~text
BeginLoadSlot
→ EnumerateFilesInSlot
→ TryLoadSlotFile
→ write mod-owned ZIP
→ EndLoadSlot
~~~

The mod does not add another Tainted Grail save slot.

## What to change first

If you are learning the callback, leave backup creation disabled and change only the log message produced when a slot completes.

Once that works, move on to the dedicated backup example.

## Next

- [Read the save-completion guide](../../../../guides/tasks/saving/observe-native-save-completion.md)
- [Open the dedicated backup example](../smart-save-backup/README.md)
