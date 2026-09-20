# Story Graph Compiled Runtime Boundary

Use this page when reasoning about custom Story content or runtime Story hooks.

Canonical overview: [Story Graphs](README.md).

## Authoring graph is not runtime payload

The important boundary is:

```text
XNode StoryGraph authoring
→ Questline parser/compiler
→ StoryGraphRuntime
→ .story binary
→ story archive
→ StoryReader
→ runtime Story / StepSequenceRunner
```

A mod cannot assume that creating an XNode graph at runtime is equivalent to producing valid shipping Story content.

## Compiled identity matters

Runtime compatibility can depend on:

- graph GUID;
- chapter/bookmark identity;
- compiled node/step type IDs;
- binary record layout;
- localisation/Babel identities;
- archive placement/mounting.

Unknown type-ID or payload mappings should fail closed.

## Runtime observation vs authoring

Runtime hooks such as offered/selected choices can be useful without solving the compiler/package pipeline.

Keep these claims separate:

- observe native Story;
- trigger existing Story entry/bookmark;
- author new Story graph;
- compile compatible `.story`;
- mount/package custom Story;
- persist custom Story consequences.

They require different evidence.

For safe choice observation see [Observe a dialogue choice](../../../how-to/quests-dialogue/observe-a-choice-safely.md).
