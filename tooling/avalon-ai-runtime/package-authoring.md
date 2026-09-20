# Avalon AI Package Authoring

A V2 AI package implements the provider-neutral contract:

`IAvalonAiPackage`

and exposes:

- a package manifest;
- goal policies;
- goal definitions;
- action definitions.

## Manifest responsibilities

The manifest declares stable package identity and compatibility such as:

- package ID/name/version;
- required Runtime API version;
- goal IDs;
- action IDs;
- required action capabilities;
- blackboard namespace/schema;
- declared blackboard keys;
- supported actor roles;
- maximum policy cadence;
- persistent-key declarations;
- procedure requirements where used.

## Author boundary

Your package should describe **what should be considered/done**, not own the host loop or direct FoA execution.

Package code should remain provider-neutral and reference `AvalonAI.Contracts` / V2 contracts only.

## Good package responsibilities

- translate provider-owned observations into package policy;
- declare goals/actions;
- reason over declared blackboard facts;
- return bounded proposals/plans;
- return no proposal when not ready.

## Forbidden shortcut

Do not reach around the host to call game APIs directly from a package just because a type is discoverable.
