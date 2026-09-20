# Modifier-Gated Illegal Pickup

This example teaches the theft-guard pattern used by the working Hold to Steal path.

The important rule is to guard the **native theft action** rather than reimplement inventory transfer.

## Covered interaction families

The working implementation lineage covers:

- direct loose-world theft;
- container item transfer;
- container take-all;
- readable-item theft.

## Build

~~~powershell
dotnet build .\IllegalPickupGuard.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Pattern

1. let the game decide that the action is illegal;
2. check the mod's additional input requirement;
3. allow the native action when authorised;
4. block only the guarded theft action otherwise.

Legal pickup remains native.
