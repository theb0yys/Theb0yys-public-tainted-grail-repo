# Mono Harmony Action Guard

Starter for native actions that should proceed normally unless one narrow mod-owned condition blocks them.

The included target is self-owned. When adapting it, preserve a valid blocked result and do not reimplement the whole native action system.

Build:

```powershell
dotnet build .\HarmonyActionGuard.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```
