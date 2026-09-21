# Importer Pipelines

Custom content is not one generic "import" operation in Tainted Grail. Armour, creatures, and weapons cross different native systems, file formats, runtime owners, and validation boundaries.

Use the pipeline that matches what you are importing:

- [Armour importer](armour.md) — Unity/skinned-mesh intake, deformation validation, Kandra package generation, and guarded registration work.
- [Creature importer](creatures.md) — source intake, native baseline selection, visual transport, animation mapping, template construction, runtime lifecycle, and live proof.
- [Weapon importer](weapons.md) — package admission, native item-template registration, Drake presentation, preview, acquisition, persistence, and compatibility boundaries.

## The common rule

Treat these as staged pipelines:

~~~text
source intake
→ exact native baseline
→ authored/cooked transport
→ structural validation
→ runtime registration/integration
→ presentation
→ lifecycle/cleanup
→ persistence
→ compatibility
→ release proof
~~~

A successful early stage does not prove the later stages. A bundle loading does not prove registration. Registration does not prove presentation. A visible object does not prove persistence or release readiness.

Keep proprietary game binaries and third-party source assets out of the public repository. Publish your own source, manifests, schemas, tooling, and redistributable outputs only.
