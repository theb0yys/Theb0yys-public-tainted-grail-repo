# IL2CPP Result Postfix

Starter for preserving original execution and adjusting only the final returned result.

The initial target is self-owned; replace it only after validating the generated interop target for the current game build.

Build:

```powershell
dotnet build .\Il2CppResultPostfix.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```
