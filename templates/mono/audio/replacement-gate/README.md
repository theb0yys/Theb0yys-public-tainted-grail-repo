# Mono Audio Replacement Gate

A copy-ready replacement-audio starter built around one invariant:

> Do not suppress the native/original event unless the replacement actually started.

The default demonstration is self-owned and generates a short tone, so no commercial audio is redistributed.

Build:

```powershell
dotnet build .\AudioReplacementGate.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

When adapting it to FMOD/footsteps/world actions, verify the exact owner and filter, keep unrelated events native, and fail open if replacement playback fails.
