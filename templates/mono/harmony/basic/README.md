# Mono Harmony Basic

Copy this folder for a Mono/BepInEx 5 mod that will use Harmony.

The included patch targets a method inside the template assembly, so you can prove Harmony wiring before choosing a FoA target.

1. Rename assembly, namespace, plug-in GUID and display name.
2. Build against the local game/BepInEx installation.
3. Confirm the self-test logs `patched`.
4. Remove the self-test target/patch.
5. Add only verified game targets.

Build:

```powershell
dotnet build .\HarmonyBasic.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```
