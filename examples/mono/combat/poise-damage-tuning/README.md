# Poise Damage Tuning

This example temporarily scales the incoming Damage.Parameters.PoiseDamage while NpcGeneralFSM.OnDamageTaken runs, then restores the original value in the postfix.

It does not edit NpcStats.PoiseThreshold and it does not replace the native stagger system.

## Build

~~~powershell
dotnet build .\PoiseDamageTuning.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Start with one enemy and one known attack. Change PlayerPoiseDamageMultiplier, verify the poise-break timing changes, and verify ordinary damage/stamina stagger remain native.

Guide: [Tune poise without rewriting stagger](../../../../guides/tasks/gameplay/tune-poise-safely.md)
