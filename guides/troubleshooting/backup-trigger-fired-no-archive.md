# Backup Trigger Fired but No Archive Appeared

Check the pipeline in order:

1. Was a source save slot available?
2. Was `CanAutoSave()` false?
3. If autosave was blocked, was latest-save fallback enabled?
4. Did the provider load API open the source slot?
5. Were domain entries enumerable/readable?
6. Was the plugin-owned output directory writable?
7. Was the request only enqueued from a save-completion patch and never processed on the main-thread backup path?

Do not “fix” this by writing into the native save directory or creating extra visible slots.
