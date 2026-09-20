# Research Methods

Use these methods when you know **what you want to change**, but you do not yet understand enough of FoA's implementation to choose a safe hook or API.

You do not need to use every method for every problem. Pick the one that matches what is still unknown.

## [Find the native owner](finding-native-owner.md)

Use this when you can see the behaviour but do not yet know **which game system actually controls it**.

Typical questions:

- Is this owned by the Hero, an Item, a View, a service, or something else?
- Is the visible GameObject really the gameplay owner?
- Which assembly/type should I inspect next?

This is usually the first step when several plausible patch targets exist.

## [Trace a lifecycle](tracing-lifecycles.md)

Use this when the right object exists, but **timing is unclear**.

Typical problems:

- the object is null during plugin startup;
- a hook fires too early;
- UI has already cached its data;
- an object is valid before a scene change but stale afterwards;
- cleanup is happening in the wrong place.

The goal is to find when the object is created, initialized, used, restored, and discarded.

## [Separate static evidence from runtime evidence](static-vs-runtime-evidence.md)

Use this when you need to decide **what source/decompilation proves and what still needs an in-game test**.

For example:

- decompilation can show that a method exists and what it calls;
- source can show an intended access pattern;
- only runtime evidence can prove that your patch fired on the current build in the scenario you care about.

This keeps "I found the code" separate from "I proved the feature works."

## [Verify the exact native identity](name-heuristic-vs-native-identity.md)

Use this when you found something by **name, search result, visual similarity, or guesswork** and need to prove it is the exact object/template/event you think it is.

Examples:

- item/template GUID;
- SkillGraph;
- scene;
- actor template;
- FMOD event;
- status;
- native type or method overload.

Names are useful for discovery. Exact identities are what you should build against.

## [Prove a new mechanic](proving-a-mechanic.md)

Use this when the pieces are mostly understood and you want to turn them into a **repeatable modding technique**.

A useful proof normally connects:

~~~text
exact identity
→ native owner
→ lifecycle point
→ intervention
→ downstream result
→ cleanup
→ verification
~~~

The result should explain not only what worked, but also what it does **not** prove.

## Where subject-specific research goes

If you are investigating one particular system or feature—such as persistence, travel, crime, or a specific bug—keep the working investigation under [Investigations](../investigations/).

Once the important facts are established, move the reusable explanation into the appropriate [Knowledge](../../knowledge/README.md) page and link back to the research for provenance where useful.
