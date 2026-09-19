# Story Graphs, Dialogue, Choices, and Runtime Execution

> **Reference page.** Use this when investigating dialogue, Story Graphs, choices, quests, narrative triggers, or Story UI.

## What this system is

Questline Story Graphs are a custom authoring + compilation + runtime interpreter stack.

High-level architecture:

~~~text
XNode-based StoryGraph authoring
→ StoryGraphParser
→ StoryGraphRuntime
→ GUID-named .story payload
→ story.arch
→ StoryReader
→ Story MVC Model
→ StepSequenceRunner
→ StoryStep execution
→ dialogue / choices / quests / gameplay
~~~

XNode is the authoring substrate. It is not the normal player runtime interpreter.

## Who owns it in FoA

Important owners include:

- `StoryGraph` — authoring graph + GUID identity;
- `StoryGraphParser` — authoring-to-runtime compiler;
- `StoryGraphRuntime` — compiled runtime representation;
- `StoryWriter` / `StoryReader`;
- `Story : Model` — active execution owner;
- `StepSequenceRunner` — runtime interpreter;
- `StoryStep` / condition runtime objects;
- optional Story Views/UI such as dialogue panels.

## Important identities, types, and methods

### Graph identity

`StoryGraph.GUID` is used in the inspected source for:

- compiled payload filename;
- runtime lookup;
- bookmark graph reference;
- Story runtime selection.

### Bookmark identity

A `StoryBookmark` combines:

- Story graph GUID/reference;
- semantic chapter/bookmark name.

Bookmarks are entry references. They are **not proven arbitrary mid-execution save checkpoints**.

### Active Story persistence

The researched `Story` Model is `IsNotSaved = true`.

That means the active interpreter/runner Model is not persisted through ordinary MVC Model saving.

It does **not** mean Story actions cannot mutate other persistent game state.

## Where it exists in the lifecycle

~~~text
Story.StartStory(config)
→ World creates/registers Story Model
→ compiled graph selected by bookmark GUID
→ Story state initializes
→ chapter/bookmark resolved
→ StepSequenceRunner begins
→ steps/conditions execute
→ optional View/UI
→ Story ends/discards
~~~

The runtime works on compiled Story representation, not direct XNode traversal.

## How we interact with it

### Identify the exact Story graph first

Use graph GUID + semantic bookmark/chapter names where established.

### Observe before mutating

Useful source-located hooks include:

- `Story.OfferChoice(...)`;
- `VChoice.Select(Choice)`;
- choice hover/constructor paths that differ by runtime lane.

These can establish offering/selection flow, but a visible choice does not by itself prove downstream quest/state effects.

### Map the complete narrative path

For a choice mutation, map:

~~~text
Story step
→ choice construction/offering
→ UI presentation
→ selection/input
→ callback/handler
→ Story continuation
→ quest/state/consequence mutation
~~~

## Why this route

Story is compiled and interpreted.

Editing an authoring graph assumption, choice UI, or one handler without understanding the compiled/runtime owner can affect the wrong layer.

The system also couples some text to Babel's positional `LightLocString` IDs, making private Story compilation against a modified localisation corpus dangerous.

## What goes wrong

### XNode graph treated as live runtime object

Normal runtime execution uses Questline's compiled representation.

### Choice UI treated as complete Story mutation path

Selection may occur, but the Story continuation/state effect can be elsewhere.

### Bookmark treated as save program counter

Not established.

### Active Story Model `IsNotSaved` misread as "Story has no persistence"

Quest/flags/decisions triggered by Story can be saved through their own owners.

### Private .story binary mutation

The type-byte registry and compatibility/version surface are incomplete; arbitrary injection is not a safe public process.

## How to verify

For a narrative hook/process, prove:

1. exact Story GUID;
2. entry bookmark/chapter;
3. expected compiled graph loads;
4. Story Model starts;
5. target step/choice is reached;
6. UI/input selection fires if applicable;
7. handler/continuation executes;
8. expected quest/state owner changes;
9. save behavior is validated separately if durable;
10. Story/Views clean up.

## Current proof boundary

The authoring→compiled-runtime architecture and Story MVC ownership are well supported by reviewed source research.

Arbitrary runtime node injection, direct `.story` mutation, full current node/type registry, and blanket save-compatibility claims are not proven.
