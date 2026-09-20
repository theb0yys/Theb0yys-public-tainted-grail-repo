# Change Items After a Container Has Rolled Its Loot

This example waits until Tainted Grail has already generated a container's contents, then changes those generated rows.

That is useful for rules such as:

- increase consumable quantities;
- reduce crafting-material quantities;
- remove a particular category after the normal loot roll.

## Build it

~~~powershell
dotnet build .\ContainerPostRollRules.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save.

## Try it in game

Find a searchable container you have not already emptied.

Open it once and compare the generated quantities with the rule in Plugin.cs.

Close/reopen the same container and confirm the same SearchAction is not transformed repeatedly.

## What to change first

Change the quantity multiplier for one simple category.

Do not begin by replacing the loot table.

## How it works

After Tainted Grail creates the runtime container rows, the example reads the container's generated item list.

It then changes only rows matching the example rule.

A small per-container guard remembers which SearchAction instance was already processed, so an irreversible rule is not applied again and again to the same generated list.

The normal container UI and item-transfer code still run afterward.

## Next

[Read the container post-roll guide](../../../../guides/tasks/gameplay/change-post-roll-container-contents.md)
