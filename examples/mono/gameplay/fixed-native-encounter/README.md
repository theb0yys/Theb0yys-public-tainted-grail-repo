# Fixed Native Wyrdspirit Encounter

A minimal exact-profile encounter example.

~~~text
F8
→ exact Spec_EnemyMonster_T1_Wyrdspirit
→ BaseLocationSpawner.VerifyPosition
→ native SpawnLocation
→ wait for Location initialization
→ require alive hostile non-ally NpcElement
→ native NpcAI.EnterCombatWith(Hero.Current)
→ observe native death
→ cooldown

F9 / unload
→ discard only the exact owned Location
~~~

## Build

~~~powershell
dotnet build .\FixedNativeEncounter.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable test save and open ground.

## Boundary

The historically proven fixed Wyrd Hunt path includes broader leave/return and tested save/load evidence. This compact public example intentionally marks its spawned actor not-saved and teaches the spawn/combat/death ownership seam rather than persistence.

Guide: [Build a fixed native encounter](../../../../guides/tasks/world/build-a-fixed-native-encounter.md)  
Evidence: [Fixed native Wyrdspirit encounter](../../../../research/case-studies/gameplay/wyrdspirit-encounter.md)
