# Make Combat Feel More Pressured

This example shows three small ways to make fights feel more demanding **without increasing enemy damage or health**.

It can:

- let more enemies attack at once;
- shorten the pause before another enemy can attack;
- make the player's actions cost more stamina;
- change how much poise damage the player's attacks deal.

You can use the same project for either the [combat pressure guide](../../../../guides/tasks/gameplay/tune-combat-pressure.md) or the [poise guide](../../../../guides/tasks/gameplay/tune-poise-safely.md).

## Build it

Open PowerShell in this folder and run:

~~~powershell
dotnet build .\CombatPressurePoise.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Replace the example game path with the real folder Steam opens for Tainted Grail.

Copy the built DLL into its own folder under:

~~~text
Tainted Grail FoA\BepInEx\plugins\
~~~

## Try it in game

Start with every multiplier set to 1.0. That gives you the normal game values.

Then change **one setting at a time**.

A simple first test is:

1. leave poise and stamina at 1.0;
2. raise the enemy attack-slot multiplier a little;
3. enter a fight with several enemies;
4. confirm that more enemies are allowed to take attack turns;
5. restore it to 1.0;
6. test the next setting.

For poise, test one known enemy with one known attack before changing several attack types.

## What to change first

Open Plugin.cs and look for the config settings created in Awake().

The important values are the multipliers for:

- enemy attack slots;
- enemy attack recovery;
- player action stamina;
- player poise damage.

Small changes are easier to understand than extreme values.

## How it works

Tainted Grail already has values that control these systems. The mod changes those existing values instead of creating a second combat system.

The game names you will see in the code are:

- Difficulty.MaxEnemiesAttacking — how many enemy attacks may be active;
- Difficulty.AttackActionUnBookProlong — how long an enemy attack slot stays occupied;
- CharacterStats.StaminaUsageMultiplier — the player's action-stamina cost;
- Damage.Parameters.PoiseDamage — the poise contribution of the current hit.

The poise value is changed only while the game handles that hit, then restored immediately afterward.

The stamina change is attached to the player's current stat object and is not written into the save.

## Next

- [Understand the combat-pressure version](../../../../guides/tasks/gameplay/tune-combat-pressure.md)
- [Understand the poise version](../../../../guides/tasks/gameplay/tune-poise-safely.md)
