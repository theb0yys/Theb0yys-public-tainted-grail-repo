# Spawn a Temporary Human Ally

This example spawns one **new, non-unique human NPC** and temporarily makes that new actor an ally.

It does not recruit or rewrite an existing story character.

## Build it

~~~powershell
dotnet build .\TemporaryHumanAlly.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save.

## The example character

By default the project uses this repeatable human template:

~~~text
Spec_NPC_Special_GalahadSquire_Repetetive
a13a2abd2f5e61d438f322360035ea9a
~~~

The long value is the template's stable game ID.

## Try it in game

Use the keys shown by the example's startup log.

The important things to confirm are:

1. one new human appears;
2. the actor behaves as an ally rather than an enemy;
3. the actor can be handed into the game's normal ally combat behavior;
4. dismissing the actor removes the exact one created by the mod;
5. restarting the game does not create a saved permanent companion.

## What to change first

Do **not** start by changing the character template.

First change the spawn distance or one of the simple runtime commands.

Once the lifecycle is clear, swap to another known non-unique human template.

## How it works

The example:

~~~text
loads an exact LocationTemplate
→ rejects unique/story-style templates
→ spawns a new Location
→ marks it not saved
→ gets the spawned NpcElement
→ gives it the hero's summon faction
→ adds NpcHeroPetAlly
→ keeps the exact spawned Location
→ discards that Location when finished
~~~

NpcHeroPetAlly is an existing Tainted Grail ally behavior. The mod uses that rather than inventing a replacement combat AI.

## Next

[Read the temporary-human-ally guide](../../../../guides/tasks/creatures/build-a-temporary-human-ally.md)
