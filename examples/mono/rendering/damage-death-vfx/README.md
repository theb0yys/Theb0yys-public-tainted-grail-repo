# Damage and Death VFX Sidecar

A minimal mod-owned particle sidecar attached to accepted native character damage/death lifecycle methods.

## Hook boundary

~~~text
HealthElement.OnDamage
→ character target only
→ small mod-owned particle burst

HealthElement.OnDeathEvents
→ character target only
→ larger terminal particle burst
~~~

The example does not patch target resolution.

## Build

~~~powershell
dotnet build .\DamageDeathVfx.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Ownership

Every particle GameObject is created by this plug-in and destroyed after 1.5 seconds. Damage, target selection, death, corpse, loot and rewards remain native.

## Boundary

The particles are intentionally simple and source-generated; this is a lifecycle/ownership example, not a polished blood-effects package.

Guide: [Add damage and death VFX sidecars](../../../../guides/tasks/rendering/add-damage-and-death-vfx-sidecars.md)  
Evidence: [Damage and death VFX sidecars](../../../../research/case-studies/rendering/damage-death-vfx.md)
