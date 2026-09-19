# 14 — Observe Completed Save-Slot Writes

**Category:** persistence / save lifecycle observation  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example does **not** edit save data.

It resolves the same four known FoA cloud-service implementations used by the owner-side backup work and observes `EndSave(string)` after the native method returns.

That gives a mod a post-save notification candidate without replacing serialization.

## Build

```powershell
dotnet build .\SaveSlotObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

## Important limit

An `EndSave` observation is not automatically proof of durable filesystem flush, every possible backend, or safe backup timing. The owner-side backup plugin loaded and created its config/folder, but actual archive creation remained unproved in the inspected evidence.
