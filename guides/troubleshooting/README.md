# Troubleshooting

Start with the **first thing that failed**, not the final symptom.

For example, if a custom item is missing after loading a save, first check whether its template was registered and available at restore time before changing inventory or UI code.

- [Hook did not fire](hook-did-not-fire.md)
- [Template lookup is too early](template-lookup-too-early.md)
- [Custom item missing on load](missing-registrar-on-load.md)
- [UI opens but action does not fire](ui-opens-but-does-not-work.md)
- [Content registers but is not visible](content-registers-but-not-visible.md)
- [Works now but not after load](works-now-but-not-after-load.md)
- [Runtime shim is not persistence](runtime-shim-not-persistent.md)
- [Addressables works offline but not through ModService](addressables-loads-offline-not-in-game.md)
- [Private reflection broke after update](private-reflection-broke-after-update.md)
- [Presentation-only failure](presentation-only-failure.md)

The goal is to identify the earliest broken step in the chain and fix that step instead of adding workarounds farther downstream.
