# Combat VFX Sidecar

This example adds bounded mod-owned particles after FoA's accepted damage/death lifecycle:

~~~text
HealthElement.OnDamage postfix
→ character target
→ reject blocked/parried/DOT
→ per-target 100 ms budget
→ short mod-owned impact

HealthElement.OnDeathEvents postfix
→ character target
→ one terminal effect per target
~~~

Build:

~~~powershell
dotnet build .\CombatVfxSidecar.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example never patches Damage.DetermineTargetHit. Damage, death, corpse, loot and XP remain native.

All mod-owned effect GameObjects have a short lifetime and are also destroyed on plug-in unload.

Guide: ../../../../guides/tasks/rendering/build-a-bounded-combat-vfx-sidecar.md
