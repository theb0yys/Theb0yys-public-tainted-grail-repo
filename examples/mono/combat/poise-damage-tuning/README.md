# Poise Damage Tuning

Temporarily scale Damage.Parameters.PoiseDamage inside NpcGeneralFSM.OnDamageTaken, then restore the original value in the postfix.

~~~powershell
dotnet build .\PoiseDamageTuning.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This does not edit NpcStats.PoiseThreshold and does not replace stamina-driven stagger.

Guide: ../../../../guides/tasks/gameplay/tune-poise-safely.md
