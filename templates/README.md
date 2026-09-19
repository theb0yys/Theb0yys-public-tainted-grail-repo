# Templates

These projects are deliberately small and use **local assembly references**. They do not redistribute game or BepInEx binaries.

- `mono-basic` — BepInEx 5 / Unity Mono starter using `BaseUnityPlugin`.
- `il2cpp-basic` — BepInEx 6 / Unity IL2CPP starter using `BasePlugin`.

Pass your game path at build time:

```powershell
dotnet build <project.csproj> -p:GameRoot="D:\Games\Tainted Grail FoA"
```

If your local installation layout differs, update the project locally or pass additional MSBuild properties. Do not commit machine-specific absolute paths.
