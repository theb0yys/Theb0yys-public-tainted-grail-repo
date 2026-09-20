# Temporary Human Ally

A session-only human ally example using an exact non-unique repetitive human template.

~~~text
TemplateReference
→ reject unique template
→ SpawnLocation
→ MarkedNotSaved
→ reject unique spawned NPC
→ OverrideFaction(hero summon faction)
→ NpcHeroPetAlly
→ optional EnterCombat() defend handoff
→ exact Location.Discard()
~~~

Default target:

~~~text
Spec_NPC_Special_GalahadSquire_Repetetive
a13a2abd2f5e61d438f322360035ea9a
~~~

## Build

~~~powershell
dotnet build .\TemporaryHumanAlly.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save.

Guide: [Build a temporary human ally](../../../../guides/tasks/creatures/build-a-temporary-human-ally.md)
