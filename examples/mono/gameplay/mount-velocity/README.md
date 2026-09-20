# Mount Velocity

Minimal Mono/BepInEx 5 example for the proven native-horse velocity seam.

## Owner

The game still computes movement. This example changes only the return values of VMount.RunningVelocity and VMount.TurningVelocity through Harmony postfixes.

## Build

~~~powershell
dotnet build .\MountVelocity.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Copy the resulting DLL into its own BepInEx plug-in folder.

## Expected log

After mounting/moving, each active getter logs once:

~~~text
RunningVelocity postfix active. Multiplier=1.25.
TurningVelocity postfix active. Multiplier=1.25.
~~~

## In-game test

1. test 1.0 / 1.0 as vanilla;
2. test running only;
3. test turning only;
4. test 1.25 / 1.25;
5. dismount/remount;
6. cross a normal transition;
7. save/load and relaunch;
8. restore 1.0 / 1.0.

## Boundary

This demonstrates only native running/turning scalar adjustment. It does not own mount spawning, recall, stamina, armour, animation, camera, persistence or custom mounts.

Guide: [Change mount running and turning speed](../../../../guides/tasks/gameplay/change-mount-speed.md)  
Evidence: [Native mount velocity tuning](../../../../research/case-studies/gameplay/mount-velocity.md)
