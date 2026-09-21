# Real Unity Sample Receipts

This directory stores importer-owned Unity receipts produced by the production
geometry adapter and canonical `ArmorImporter.Import` entry point.

The T56 Dragon Knight iron/no-cape sample is expected to execute six typed
imports, reproduce 444 orientation-reversed triangles and zero collapsed
triangles, leave deformation blocked, and keep candidate-map application plus
every downstream operation false.

That expectation was executed successfully in Unity 6000.0.64f1. The JSON
receipt contains the exact triangle ordinals and typed pipeline results; the
companion Markdown receipt records the sample, hashes, row totals, and boundary.

The canonical current T56 fixture receipt is
`t56-dragon-knight-iron-no-cape-full-geometry-import.json` because it preserves
the refreshed calibration fields and the importer-owned full-geometry capture
needed by the R2 package/compatibility lane. The older non-full
`t56-dragon-knight-iron-no-cape-import.json` is retained only as historical
evidence and must not be used by downstream fixtures.
