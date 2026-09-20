# Observe Native Save Completion

Use FoA's concrete cloud-service completion methods as a post-save notification surface. Do not invent a new native save domain.

Working lineage: [Native Save Completion Observer](../../../research/case-studies/persistence/native-save-completion-observer.md).

## Runnable source

Start with the [Save completion observer example](../../../examples/mono/infrastructure/save-completion-observer/README.md). Build it unchanged first, then change one setting or mechanism at a time.


## Concrete targets

The working implementation resolves `EndSave(string)` on these concrete types:

~~~text
Awaken.TG.Main.Saving.Cloud.Services.SteamCloudService
Awaken.TG.Main.Saving.Cloud.Services.SteamNoCloudService
Awaken.TG.Main.Saving.Cloud.Services.DebugCloudService
Awaken.TG.Main.Saving.Cloud.Services.GogCloudService
~~~

Each target is patched with a Harmony postfix:

~~~csharp
private static void Postfix(string slotId)
{
    Plugin.NotifySaveSlotWritten(slotId);
}
~~~

Use `TargetMethods()` so only concrete implementations that actually exist in the current build are patched.

## Keep the hook lightweight

The postfix should not zip files or do heavy disk work directly.

The working pattern is:

~~~text
EndSave(slotId)
→ enqueue slotId
→ main plug-in Update()
→ drain queue
→ perform bounded post-save work
~~~

A `ConcurrentQueue<string>` is sufficient for that handoff.

## Slot identity

The useful storage identity is the save slot's `SaveFileName`.

The Smart Save Backups implementation keys pending save work by:

~~~text
SaveSlot.SaveFileName
~~~

and correlates that with the `slotId` received by `EndSave(string)`.

`SaveSlot.ID` can be used for same-process lookup/correlation, but do not substitute it blindly for the storage key.

## Requesting a normal FoA save

If your feature needs a fresh autosave first, use the normal guards:

~~~text
CloudService.IsInitialized
+ World.HasAny<Hero>()
+ LoadSave.Get.CanAutoSave()
→ SaveSlot.GetAutoSave(... allowCreate:false)
→ LoadSave.Get.Save(saveSlot, ...)
~~~

Then wait for the matching `EndSave(saveSlot.SaveFileName)` notification before doing post-save work.

## Scope

This process gives you a concrete **save-completion notification point** and exact slot ID. It does not require or imply custom native serialization.
