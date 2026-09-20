# Container Post-Roll Rules

This example patches SearchAction.OnInitialize after FoA has generated the runtime container rows.

It reads the private _itemsInsideContainer collection and applies one quantity rule only to generated rows whose ItemTemplate is consumable or crafting material. A ConditionalWeakTable ensures one SearchAction instance is not transformed twice.

Build:

~~~powershell
dotnet build .\ContainerPostRollRules.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The example does not replace loot tables, item transfer, or container UI.

Guide: ../../../../guides/tasks/gameplay/change-post-roll-container-contents.md
