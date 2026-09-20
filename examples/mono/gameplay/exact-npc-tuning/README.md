# Exact NPC Tuning

A deliberately narrow example based on the validated plain Tier-1 Drowner lane.

## Exact target

~~~text
name: Spec_EnemyZombie_T1_Drowner
GUID: bb613531c5d3bf5499ea3b8103a4024e
~~~

The example performs one throttled World.All<NpcElement>() pass per second, rejects every non-matching actor early, and applies only temporary native aggression/sight/melee state to an alive hostile non-ally exact Drowner.

## Build

~~~powershell
dotnet build .\ExactNpcTuning.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Cleanup

Every temporary HyperAggressiveToHero/StatTweak object owned by this example is discarded when the actor disappears, the feature is disabled, or the plug-in unloads.

## Boundary

This does not replace AI, movement, pursuit, Fireball, spawning, loot, factions, saves or persistence.

Guide: [Tune one native enemy profile](../../../../guides/tasks/creatures/tune-one-native-enemy-profile.md)  
Evidence: [Exact-target NPC tuning](../../../../research/case-studies/gameplay/npc-tuning.md)
