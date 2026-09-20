# Presentation Systems

Use these pages when you are changing what the player sees, hears, or controls: UI, camera, audio, rendering, fog, VFX, culling, or related presentation systems.

FoA does not use one rendering or presentation path for every object. A weapon, character body, world prop, UI screen, and audio event may all be owned by different systems.

- [UI and input](ui-input.md)
- [Native Hero HUD](native-hero-hud.md)
- [Hero camera and body](hero-camera-body-ownership.md)
- [HDRP world fog](hdrp-fog-ownership.md)
- [Distance culling](distance-culling.md)
- [Audio and FMOD](audio-fmod-integration.md)
- [Music](audio-music.md)
- [Native music and ambience](audio-ownership.md)
- [Proprietary rendering overview](proprietary-rendering-systems.md)
- [Drake / MergedDrake](drake/README.md)
- [Kandra](kandra/README.md)
- [Medusa](medusa/README.md)
- [Leshy](leshy/README.md)
- [HLOD](hlod/README.md)
- [Shared mipmap streaming](mipmap-streaming/README.md)
- [Critter VAT / ECS](critter-vat/README.md)

Before replacing or suppressing presentation, identify which system creates it and which system is responsible for cleanup.
