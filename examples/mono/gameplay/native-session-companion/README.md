# One-Session Native Companion

A compact exact-Qrko companion lifecycle.

~~~text
F8
→ resolve Spec_Pet_Qrko by exact GUID/name
→ native SpawnLocation
→ MarkedNotSaved=true
→ require NpcElement
→ native summon faction + NpcHeroPetAlly
→ track exactly one owned Location

F9 with a live hero attacker
→ NpcHeroPetAlly.EnterCombat()

F8 again / plug-in unload
→ exact owned Location.Discard()
~~~

## Build

~~~powershell
dotnet build .\NativeSessionCompanion.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable test save.

## Boundary

This is session-only. It does not save a roster, recruit existing world NPCs, replace targeting/pathing, add dialogue/equipment/levelling, or respawn a companion after load.

Guide: [Build a one-session native companion](../../../../guides/tasks/creatures/build-a-one-session-native-companion.md)  
Evidence: [One-session native companion lifecycle](../../../../research/case-studies/companions/native-companion-lifecycle.md)
