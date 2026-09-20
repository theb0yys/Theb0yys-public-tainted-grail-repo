# Optional Cross-Mod API

A minimal consumer for the proven public API bridge shape used by Wyrd Decoy and Wyrd Hunt.

## Pattern

~~~text
provider absent
→ feature unavailable
→ consumer otherwise continues

provider present
→ resolve exact public API
→ require IsAvailable
→ call TryReduceThreat
→ only after success commit consumer-side charge
~~~

The example has one session-only demo charge. Press F8 to attempt the call.

## Build

~~~powershell
dotnet build .\OptionalCrossModApi.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The project has no compile-time Wyrd Hunt dependency.

## Boundary

This demonstrates optional public-contract discovery and transactional consumer-side state. It does not reflect into provider internals or provide general plugin-version migration.

Guide: [Build an optional cross-mod API bridge](../../../../guides/tasks/interoperability/build-an-optional-cross-mod-api-bridge.md)  
Evidence: [Fail-closed cross-mod API bridge](../../../../research/case-studies/gameplay/cross-mod-api.md)
