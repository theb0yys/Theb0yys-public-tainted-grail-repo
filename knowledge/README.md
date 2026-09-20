# Knowledge

Use this section when you need to understand **how FoA works** or need an exact technical fact before writing code.

Knowledge is split into three kinds of information:

## [Systems](systems/README.md)

Start here when you need to understand the game's own architecture.

System pages explain things such as:

- which native object or service actually owns a behavior;
- when that owner is created and ready;
- which other systems consume its state;
- how it is cleaned up;
- which parts are known from source, decompilation, or runtime evidence.

Examples include player/Hero state, combat, items, actors, scenes, saving, UI, audio, and FoA's specialized rendering systems.

## [Mechanics](mechanics/README.md)

Use these when you already know the change you want to make and need an established way to do it.

A mechanic connects the native owner and lifecycle to a practical intervention: for example changing a stat, registering an item, guarding an interaction, adding a runtime effect, or integrating configuration.

Each mechanic has its own evidence and validation limits. Do not assume one working mechanic generalizes to every similar-looking system.

## [Reference](reference/README.md)

Use Reference when you need an exact lookup rather than an explanation.

This includes:

- types and members;
- hooks and lifecycle points;
- services;
- template access;
- GUIDs and identities;
- assemblies;
- runtime differences;
- versions and compatibility;
- performance-sensitive APIs.

## If you are trying to do a task

Use [Guides](../guides/README.md) for step-by-step workflows.

If the behavior is still uncertain, contradictory, or not yet proven well enough to document as established knowledge, use [Research](../research/README.md).
