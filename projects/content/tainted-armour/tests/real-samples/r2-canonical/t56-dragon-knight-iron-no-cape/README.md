# T56 Dragon Knight Iron/No-Cape R2 Canonical Package Fixture

This is a durable R2 canonical package fixture derived from the importer-owned
Unity full-geometry capture for the canonical current Dragon Knight T56 receipt in
`../../t56-dragon-knight-iron-no-cape-full-geometry-import.json`.

It preserves the real receipt's source hash, mesh name, vertex count, evaluated
submesh index, triangle count, Unity version, and no-write boundary as fixture
inputs for the compatibility reader lane. The package manifest references dense
content-addressed blobs for:

- baked positions: 43,159 `F32 VEC3` elements, 517,908 bytes,
  `sha256:080e2241ec69e0209d4e2be610ae25d5c9ec86654ba8e9afc9dc6b054f8552cc`;
- submesh-3 indices: 76,980 `U32 SCALAR` elements, 307,920 bytes,
  `sha256:cbf3554f7b98eb3a28547e590489bee5f7f7bafd0a4b33bcd688afb8c6b79aec`;
- mesh-to-asset bind matrix: one `F32 MAT4`, 64 bytes,
  `sha256:5998e9d1dd9bf48af6af87a1ef8c7783b818ac8329637ada08cc5fb329042376`.

The package exercises persisted R2 identity, reader, adapter, target-profile
fingerprinting, and compatibility result flow without claiming Kandra
conversion, runtime loading, equip/save mutation, or visual correctness.

`compatibility.receipt.json` is the durable framework-owned output artifact for
the package's current reader -> adapter -> compatibility-stage validation
result. It records the production stage cache key, receipt identity, exact
self-target classification, and the false candidate-map, conversion,
item/equip/save mutation, and downstream-write boundary.

The compatibility fixture also treats this receipt as a no-write consumer input:
it may accept the exact/pass receipt for reporting, but still does not apply
candidate maps, run conversion, mutate item/equip/save state, or write
downstream outputs.
