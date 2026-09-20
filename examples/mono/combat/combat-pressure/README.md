# Combat Pressure

Minimal Mono example using three existing FoA pressure levers:

~~~text
Difficulty.MaxEnemiesAttacking
Difficulty.AttackActionUnBookProlong
CharacterStats.StaminaUsageMultiplier
~~~

Build:

~~~powershell
dotnet build .\CombatPressure.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Start all multipliers at 1.0, then change one at a time.

- AttackSlotsMultiplier changes the native simultaneous attack-slot count.
- AttackRecoveryMultiplier changes the native release delay; lower values turn slots over faster.
- ActionStaminaMultiplier uses one non-saved runtime StatTweak on the current hero stat instance.

Damage, health, target selection and CombatDirector ownership remain native.

Guide: ../../../../guides/tasks/gameplay/tune-combat-pressure.md
