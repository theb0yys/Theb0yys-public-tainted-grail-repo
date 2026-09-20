# Crime Semantic Sidecar

A buildable example that keeps FoA's native bounty state authoritative.

The project patches CrimeUtils.AddBounty:

~~~text
prefix
→ read current native bounty
→ optionally scale only the incoming value

native CrimeUtils.AddBounty
→ commits native bounty

postfix
→ read final native bounty
→ append a mod-owned incident row
~~~

The CSV is written under BepInEx/config and does not become a competing bounty store.

## Build

~~~powershell
dotnet build .\CrimeSemanticSidecar.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Guide: [Extend crime semantics without replacing bounty](../../../../guides/tasks/gameplay/extend-crime-semantics-without-replacing-bounty.md)
