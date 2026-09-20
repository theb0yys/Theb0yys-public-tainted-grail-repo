# HDRP Fog Control

A minimal reflection-only demonstration of the proven active-HDRP-owner direction.

Press F8 once to discover currently loaded HDRP Fog profile objects plus LocalVolumetricFog behaviours, capture the state this example will change, and disable those fog owners. Press F8 again to restore the captured values.

## Why one-shot discovery

The production history found repeated Resources.FindObjectsOfTypeAll<Component>() scans could become a performance problem. This example performs the broad discovery only when you explicitly press F8 and then keeps the captured targets.

## Build

~~~powershell
dotnet build .\HdrpFogControl.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Cleanup

Unload always restores every captured active/parameter/Behaviour state before dropping references.

## Boundary

This demonstrates owner discovery, bounded mutation and restoration. It does not change weather truth, time, skybox, HLOD, vegetation or camera culling.

Guide: [Control world fog through the active HDRP owner](../../../../guides/tasks/rendering/control-world-fog-through-the-active-hdrp-owner.md)  
Evidence: [HDRP / FoA fog control](../../../../research/case-studies/rendering/fog-control.md)
