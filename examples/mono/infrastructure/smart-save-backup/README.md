# Smart Save Backup

This example listens for the native EndSave(string) completion callback, then copies the completed slot through CloudService into a mod-owned ZIP under BepInEx/config.

Build:

~~~powershell
dotnet build .\SmartSaveBackup.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Archive flow:

~~~text
native save completes
→ slotId queued
→ BeginLoadSlot
→ EnumerateFilesInSlot
→ TryLoadSlotFile
→ write .zip.tmp
→ EndLoadSlot
→ File.Move to final .zip
~~~

It never creates another FoA save slot or edits native serialization.

Guide: ../../../../guides/tasks/saving/build-sidecar-save-backups.md
