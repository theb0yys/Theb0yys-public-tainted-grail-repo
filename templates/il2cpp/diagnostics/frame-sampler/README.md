# IL2CPP Frame Sampler

Read-only performance/diagnostics starter using a registered IL2CPP `MonoBehaviour`.

It samples `Time.unscaledDeltaTime` and logs bounded aggregate frame timing. It does not patch gameplay, edit saves, or write external reports.

Build:

```powershell
dotnet build .\Il2CppFrameSampler.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```
