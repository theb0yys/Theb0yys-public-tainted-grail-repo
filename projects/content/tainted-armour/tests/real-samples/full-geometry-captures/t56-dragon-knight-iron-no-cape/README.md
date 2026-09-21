# T56 Dragon Knight Iron/No-Cape Full-Geometry Capture

This capture was written by the Tainted Armour Unity validation command while
feeding the real Dragon Knight iron/no-cape sample through `ArmorImporter.Import`.

The capture boundary is intentionally no-write:

- candidate-map application: false;
- conversion: false;
- downstream writes: false.

`capture.geometry.json` records the Unity/source/mesh metadata and points at the
raw dense geometry blobs under `raw/`. The R2 package builder consumes those raw
blobs and persists them into
`../../r2-canonical/t56-dragon-knight-iron-no-cape/`.
