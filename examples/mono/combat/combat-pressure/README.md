# Combat Pressure

Minimal Mono example for three existing pressure levers without changing raw damage or health:

~~~text
Difficulty.MaxEnemiesAttacking
Difficulty.AttackActionUnBookProlong
Hero.CharacterStats.StaminaUsageMultiplier
~~~

## Build

~~~powershell
dotnet build .\CombatPressure.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## First changes to try

- set EnemyAttackSlotsMultiplier back to 1.0 and change only recovery;
- restore recovery to 1.0 and change only action stamina;
- change one lever at a time before combining them.

The stamina tweak is runtime-only and is removed on plug-in unload.

Guide: [Tune combat pressure without raw damage multipliers](../../../../guides/tasks/gameplay/tune-combat-pressure.md)
