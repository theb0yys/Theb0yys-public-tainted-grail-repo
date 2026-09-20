# Story Graphs

## What it is

FoA's Story Graph system is a custom authoring, compilation and runtime execution stack built on top of XNode.

XNode supplies the graph/node/port authoring substrate. Questline compiles those editor graphs into a separate runtime format and runs its own story interpreter.

## Lifecycle

~~~text
XNode StoryGraph / StoryNode
→ StoryGraphParser
→ StoryGraphRuntime
→ GUID-named .story payload
→ story.arch
→ StoryReader
→ Story MVC Model
→ StepSequenceRunner
→ StoryStep / StepResult
→ dialogue / choices / quests / events
~~~

## Main types

- `StoryGraph`
- `StoryNode`
- `NodeElement`
- `StoryGraphParser`
- `StoryGraphRuntime`
- `StoryWriter`
- `StoryReader`
- `Story`
- `StepSequenceRunner`
- `StoryStep`
- `StepResult`
- `StoryBookmark`

## Runtime model

The active `Story` object is an MVC Model. The runner walks compiled chapters/steps and can pause on a `StepResult` for asynchronous work.

A `StoryBookmark` is an external entry identity: graph plus chapter/bookmark name. It is not the same thing as a saved instruction pointer into an arbitrary running step.

## Storage

Questline's compiled `.story` payload is separate from the outer Unity Archive container.

~~~text
compiled story payloads
→ StreamingAssets/Stroy/story.arch
→ mounted archive
→ StoryReader
~~~

## Localisation coupling

Story text participates in the game's localisation pipeline. Compiled text identity therefore intersects with Babel's corpus/index rules.

## Modding relevance

Use this map for:

- dialogue and choice hooks;
- quest/story execution;
- story event observation;
- understanding why an XNode asset is not the runtime interpreter.

Gameplay state written by story steps and the live Story runner are different ownership concerns.

## Related systems

- [Babel](../babel/README.md)
- [Serialization and archives](../../core/serialization-archives-implementation/README.md)
- [Runtime lifetime](../../core/runtime-lifecycle/README.md)
