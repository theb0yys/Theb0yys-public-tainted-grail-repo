# Hybrid Mono + Merlin Starter

Use this starter when one mod combines Merlin-authored content with a separate Mono/BepInEx runtime plug-in.

The two parts stay separate on purpose: Merlin owns authored content inside the official Workshop project, while the runtime project owns code that executes in the game. A small shared contract records the identity that connects them.

## Workflow

1. Start the content side from [the Merlin overlay template](../../merlin/basic/).
2. Choose one stable content/mod identity and record it in `contract/README.md`.
3. Rename the runtime project, namespace and plug-in identity.
4. Build the runtime plug-in only against locally supplied BepInEx/game references.
5. Author/export Merlin content using the official Merlin workflow.
6. Install/test the two outputs together only when the specific integration mechanism is researched and implemented.
7. Validate authoring output, runtime behaviour, persistence and compatibility as separate proof lanes.

The provided runtime plug-in only logs the shared contract ID. It deliberately does not pretend that Merlin content discovery or game integration has been proven.
