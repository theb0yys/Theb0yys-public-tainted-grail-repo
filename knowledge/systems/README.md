# Tainted Grail Game Systems

Use these pages to understand how the game's own systems work before choosing where to patch or extend them.

- [Core runtime](core/README.md) — models, services, templates, lifecycle, persistence, and other shared foundations.
- [Gameplay](gameplay/README.md) — player, combat, items, actors, progression, crafting, and related gameplay systems.
- [World](world/README.md) — scenes, saving, travel, dialogue, weather, world state, and navigation.
- [Presentation](presentation/README.md) — UI, audio, camera, rendering, VFX, and other presentation systems.
- [Provider/consumer ownership](provider-consumer-ownership.md) — how one system exposes data or behavior that another system consumes.
- [Game systems map](game-systems-map.md) — a broader map of the systems documented in this repository.

A good system page answers practical questions:

- What part of the game actually owns this behavior?
- Which types, services, or models are involved?
- When are they created and ready to use?
- What data moves through them?
- What depends on them?
- What can a mod safely observe or change?
- What evidence supports the explanation?

Being able to access a type or GameObject does not necessarily mean it owns the gameplay state you want to change.
