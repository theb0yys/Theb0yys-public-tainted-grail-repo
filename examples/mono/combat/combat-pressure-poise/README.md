# Combat Pressure and Poise

One buildable Mono/BepInEx 5 reference project for two related combat mechanisms:

- enemy group/cadence pressure through Difficulty.MaxEnemiesAttacking and Difficulty.AttackActionUnBookProlong;
- hero action-stamina pressure through a non-saved StatTweak on CharacterStats.StaminaUsageMultiplier;
- hero-originated poise contribution through a temporary prefix/postfix change to Damage.Parameters.PoiseDamage while NpcGeneralFSM.OnDamageTaken runs.

## Build

~~~powershell
dotnet build .\CombatPressurePoise.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Start with every multiplier at 1.0. Change one setting at a time.

The poise patch restores the original per-hit value immediately after native NPC damage handling. The stamina tweak is tied to the current CharacterStats instance and is not saved.

Related guides:

- [Tune combat pressure](../../../../guides/tasks/gameplay/tune-combat-pressure.md)
- [Tune poise safely](../../../../guides/tasks/gameplay/tune-poise-safely.md)
