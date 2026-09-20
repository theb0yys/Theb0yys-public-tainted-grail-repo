# Save Completion Observer and Sidecar Backup

One buildable project for two related save mechanisms.

## Observer

Harmony targets EndSave(string) on the concrete Steam, SteamNoCloud, Debug and GOG CloudService implementations.

The postfix only queues the completed slot ID. Heavy work runs later from Update.

## Backup

When enabled, the example reads the exact slot through:

~~~text
CloudService.Get.BeginLoadSlot(slotId)
→ EnumerateFilesInSlot()
→ TryLoadSlotFile(...)
→ ZIP under BepInEx/config
→ EndLoadSlot(slotId)
~~~

The ZIP is written to .tmp first and renamed only when at least one entry was copied.

It does not create another FoA save slot or modify native serialization.

## Build

~~~powershell
dotnet build .\SaveObserverBackup.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Related guides:

- [Observe native save completion](../../../../guides/tasks/saving/observe-native-save-completion.md)
- [Build sidecar save backups](../../../../guides/tasks/saving/build-sidecar-save-backups.md)
