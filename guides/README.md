# Guides

Use this section when you want to **do something**, not just look up a type or method.

The guides are organised around the jobs a modder actually has to work through: getting a mod to load, learning the FoA runtime, making a specific change, diagnosing a failure, and preparing a mod for release.

## Start with the route that matches what you are doing

### [Getting started](getting-started/README.md)

Start here if you have not yet built and loaded a FoA mod.

This route covers the basic setup needed to get from a clean machine to a working first plugin or first piece of authored content, including identifying whether your game uses Mono or IL2CPP and checking that your own code actually loads.

### [Learning paths](learning-paths/README.md)

Use these when you want to understand the modding workflow in a sensible order rather than jumping between unrelated reference pages.

There are paths for first mods, everyday runtime work, debugging, reusable systems, and more advanced topics.

### [Task guides](tasks/README.md)

Use these when you already have a specific goal, such as:

- changing gameplay behaviour;
- adding or working with items, weapons, armour, or creatures;
- building UI;
- changing audio or rendering;
- working with saving, quests, dialogue, or world systems;
- validating compatibility or interoperability.

Task guides focus on getting that job done and link to the exact technical reference when you need it.

### [Troubleshooting](troubleshooting/README.md)

Start here when something is broken and you need to find **which stage failed first**.

These guides cover problems such as hooks not firing, templates being accessed too early, content disappearing after load, UI opening without working input, Addressables failures, and private API breakage after game updates.

### [Shipping](shipping/README.md)

Use this when the mod works and you need to package, validate, version, publish, or maintain it.

This section covers repository hygiene, CI, public-safety checks, versioning, release evidence, packaging, and the difference between a successful build and an actually validated in-game release.

## When a guide sends you elsewhere

You do not need to learn the whole repository before following a guide.

When a guide needs an exact FoA fact—such as a native type, hook, service, lifecycle, template, or version-specific detail—it links to [Knowledge](../knowledge/README.md).

When the answer is still being worked out rather than established, the investigation belongs in [Research](../research/README.md).
