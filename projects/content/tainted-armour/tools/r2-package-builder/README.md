# Tainted Armour R2 Package Builder

`Tainted.Armour.R2PackageBuilder` is the framework-owned production boundary
that turns an importer-owned full-geometry capture into a durable R2 canonical
package.

Required inputs:

- `--capture`: directory containing `capture.geometry.json` plus raw position
  and submesh-index blobs;
- `--output`: R2 package directory to receive `canonical.asset.json`,
  `target-profile.json`, and content-addressed blobs;
- `--source-receipt`: the import receipt that produced the full-geometry
  capture.

The builder validates that candidate-map application, conversion, and
downstream writes stayed false in the capture receipt before it writes package
artifacts. It does not run Kandra conversion, item/equip/save mutation, runtime
registration, or native game writes.
