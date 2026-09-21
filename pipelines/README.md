# Reproducible Content Pipelines

This area owns end-to-end reconstruction paths for the four largest custom-content projects in this repository.

A pipeline is not a replacement for Knowledge, Research, Examples, or Platform documentation. It is the ordered execution surface that tells a mod author what to do, which underlying game/system contract applies, what information is required before continuing, and what the completed stage covers.

## Pipelines

- [Items](items/README.md)
- [Weapons](weapons/README.md)
- [Armour](armour/README.md)
- [Creatures](creatures/README.md)

## Pipeline rule

Every stage must record:

1. prerequisites;
2. inputs and identities;
3. exact operation;
4. produced artifact or runtime state;
5. validation gate;
6. failure conditions;
7. evidence scope;
8. the next stage.

Treat each stage as a separate operation. Asset loading, registration, acquisition, rendering, gameplay ownership, and persistence are checked independently.


## Public boundary

These pipelines document process and public/source-safe technical information. Do not add proprietary game binaries, extracted commercial assets, bulk decompiled source, private diagnostics, secrets, or private working-repository code that is not authorised for publication.
