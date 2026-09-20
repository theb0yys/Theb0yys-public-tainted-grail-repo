# Build Sidecar Save Backups

A working backup implementation can copy FoA's existing save-slot contents into a mod-owned ZIP without changing FoA's slot count or serialization format.

Working lineage: [Smart Backups Without More Save Slots](../../../research/case-studies/persistence/smart-backup-boundary.md).  

## Runnable source

Start with the [Smart save backup example](../../../examples/mono/infrastructure/smart-save-backup/README.md). Build it unchanged first, then change one setting or mechanism at a time.


## Storage location

Keep backups under a plug-in-owned directory, for example:

~~~csharp
_backupRoot = Path.Combine(Paths.ConfigPath, PluginGuid);
Directory.CreateDirectory(_backupRoot);
~~~

Do not write additional files into FoA's native save-slot archive.

## Fresh-save flow

The working implementation uses:

~~~text
snapshot requested
→ verify CloudService/world/hero readiness
→ LoadSave.Get.CanAutoSave()
→ SaveSlot.GetAutoSave(allowCreate:false)
→ pendingReasons[saveSlot.SaveFileName] = reason
→ LoadSave.Get.Save(saveSlot, ...)
→ concrete CloudService.EndSave(saveSlot.SaveFileName)
→ enqueue completed slot ID
→ Update drains queue
→ build backup from that exact slot
~~~

If a fresh autosave is not allowed, the same implementation can fall back to:

~~~text
SaveSlot.LastSaveSlotOfCurrentHero
or
SaveSlot.LastSaveSlot
~~~

depending on the feature policy.

## Read the existing slot through CloudService

To copy one completed slot:

~~~csharp
CloudService.Get.BeginLoadSlot(slotId);
try
{
    foreach (string entryName in CloudService.Get.EnumerateFilesInSlot())
    {
        if (!CloudService.Get.TryLoadSlotFile(entryName, out byte[] data))
            continue;

        // write data to the mod-owned archive
    }
}
finally
{
    CloudService.Get.EndLoadSlot(slotId);
}
~~~

This uses the same provider abstraction FoA already uses instead of assuming a Steam-only file path.

## Write a temporary archive first

The working implementation creates:

~~~text
<final-backup>.zip.tmp
~~~

then writes each readable provider entry into a ZIP entry and only after success performs:

~~~csharp
File.Move(tempPath, backupPath);
~~~

If no slot entries could be read, delete the temporary file and report failure.

Do not leave a zero-entry archive looking like a valid backup.

## Backup contents

The implementation writes each provider entry into the ZIP with a stable entry name and separately writes metadata such as:

- creation time;
- snapshot reason;
- source slot ID;
- hero name;
- hero level;
- area;
- entry count.

Keep this metadata outside the native save data.

## Retention

Enumerate only files inside your own backup directory.

Sort backups by creation/last-write time and delete only entries beyond the configured retention count. Before deleting, verify the candidate path remains inside the owned backup root.

Do not recursively delete unknown files/directories.

## Useful snapshot triggers

The working mod queues snapshots from several independent triggers:

- before dialogue choices;
- before quest turn-in;
- timed interval;
- entering configured key areas;
- explicit save completion.

Those are trigger policies. The backup mechanism itself remains the same.

## Restore policy

Restoration should be a separate explicit operation. Do not make normal backup creation silently overwrite FoA's active save slot.
