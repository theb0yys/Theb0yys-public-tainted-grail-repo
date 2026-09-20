# IL2CPP Audio Replacement Gate

BepInEx 6 IL2CPP starter for the replacement-first/suppress-second audio pattern.

The default target is self-owned and uses a generated tone. No commercial audio is included.

Build:

```powershell
dotnet build .\Il2CppAudioReplacementGate.csproj -c Release -p:GameRoot="C:\Path\To\Tainted Grail FoA"
```

When adapting to generated FMOD/FoA interop, verify the exact current target/filter and fail open if replacement playback does not start.
