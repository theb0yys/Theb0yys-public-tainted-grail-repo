# Keep Extra Crime Notes Without Replacing the Bounty System

This example keeps Tainted Grail's normal bounty system in charge and writes a separate mod-owned record describing what happened.

That is useful if you want things such as wanted levels, incident history, or custom crime categories without creating a second bounty value.

## Build it

~~~powershell
dotnet build .\CrimeSemanticSidecar.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save when changing bounty behavior.

## Try it in game

Commit a simple crime that adds bounty.

Check that:

1. the normal game bounty still changes;
2. the example writes its own incident row under BepInEx\config;
3. removing the mod does not remove or replace the game's bounty value.

## What to change first

Leave the bounty multiplier at 1.0.

Start by changing only what the sidecar record stores or how the incident is labelled.

Once that is clear, try a small bounty multiplier.

## How it works

The example observes the game's existing:

~~~text
CrimeUtils.AddBounty
~~~

Before the game adds the bounty, the mod can read the current amount.

After the game adds it, the mod reads the final amount and writes its own record.

The important split is:

~~~text
Tainted Grail bounty
= real legal/gameplay state

mod incident file
= extra history or interpretation
~~~

The mod's CSV file is not used as a replacement bounty store.

## Next

[Read the crime extension guide](../../../../guides/tasks/gameplay/extend-crime-semantics-without-replacing-bounty.md)
