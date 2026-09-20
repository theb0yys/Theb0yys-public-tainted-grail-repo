# Create a Separate Backup ZIP After a Save

This example copies a completed Tainted Grail save slot into a ZIP file owned by the mod.

It does not create an extra slot in the game's save menu and it does not change Tainted Grail's save format.

## Build it

~~~powershell
dotnet build .\SmartSaveBackup.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save while testing.

## Try it in game

1. launch the game with the example installed;
2. make a normal save;
3. wait for the save to complete;
4. check the mod's backup directory under BepInEx\config;
5. open the ZIP and confirm it contains files from the completed slot.

## What to change first

Change the backup filename or retention count.

Do not begin by changing how the native slot is read.

## How it works

After the normal save finishes:

~~~text
completed slot ID is queued
→ mod opens that slot through CloudService
→ lists the files in the slot
→ reads each file through CloudService
→ writes a temporary .zip.tmp archive
→ closes the slot
→ renames the completed archive to .zip
~~~

Writing to a temporary archive first means a failed copy does not leave a half-written file looking like a valid backup.

The backup stays outside Tainted Grail's own save slots.

## Next

[Read the save backup guide](../../../../guides/tasks/saving/build-sidecar-save-backups.md)
