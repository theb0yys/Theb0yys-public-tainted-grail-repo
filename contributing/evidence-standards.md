# Evidence Standards

Material under `knowledge/` should state runtime/build scope where relevant and distinguish observed facts from inference.

Use `research/investigations/` while identity, ownership, lifecycle, persistence or runtime behaviour is still being established. Keep static/source, runtime and persistence evidence distinct.

Use `research/case-studies/` for lessons tied to concrete mod work and `research/sources/` for official or upstream provenance.

Examples and templates demonstrate source structure or bounded mechanisms; their presence does not itself establish runtime, persistence, compatibility or release proof.

## Internal/private evidence publication

Private or internal evidence may support public documentation when the published result is a bounded, public-safe derived claim. Preserve runtime/build scope and evidence lane, and do not expose private filesystem paths, private logs, saves, credentials, proprietary binaries or bulk decompiled source.

Static/decompiled evidence may establish type/member presence, ownership and code-path relationships, but it does not by itself prove runtime ordering, persistence, compatibility or mutation safety. Link derived public claims to a provenance record under `research/sources/` when the underlying evidence is not itself publicly accessible.
