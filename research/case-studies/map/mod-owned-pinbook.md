# Sidecar Pinbook Instead of Native Map Injection

Multi-Pin Map Notes needed durable personal markers but native map-marker creation/persistence had not been researched.

The project chose:

- native player coordinates as the position source;
- plugin-owned passive HUD/manager UI;
- a small TSV file under BepInEx config;
- no FoA save write;
- no native map/compass marker mutation.

## Lesson

If the feature does not need to participate in the game's own save or map systems, a clearly scoped mod-owned sidecar can be safer and easier to remove.
