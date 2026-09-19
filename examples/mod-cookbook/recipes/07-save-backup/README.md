# Recipe — Safe Save Backup Architecture

**Category:** persistence / filesystem backup  
**Source-path evidence:** LOAD_EVIDENCED  
**Archive-creation feature proof:** NOT_COMPLETED in the inspected records

Start with [14 — Save-slot observer](../../14-save-slot-observer-mono/README.md).

## Recommended architecture

```text
native save completes candidate
        ↓
enqueue slot id
        ↓
wait until your backup worker owns the operation
        ↓
identify the completed slot files
        ↓
copy to a mod-owned backup directory
        ↓
optionally archive
        ↓
apply retention policy
```

## Rules

- never edit the original save in place;
- never write your backups into the native save slot;
- use a plugin-owned folder;
- create a new timestamped backup rather than overwriting the only old one;
- keep a retention cap;
- handle copy/archive failure without affecting the original save;
- avoid racing the game's active writer;
- treat restore as a separate higher-risk feature.

The owner-side backup mod reached build/deploy/load/config/backup-folder creation, but a real snapshot archive triggered from gameplay remained unproved. Do not label a backup feature working until an archive is actually produced and restored/inspected safely.
