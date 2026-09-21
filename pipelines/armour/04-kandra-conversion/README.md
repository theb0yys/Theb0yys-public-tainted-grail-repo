# Armour Stage 4 — Kandra Conversion and Package Build

## Objective

Convert the accepted source/target representation into the package structures required by the Kandra path while preserving traceable metadata.

## Procedure

1. Start only from a PASSED deformation candidate.
2. Convert geometry, skinning, bone, submesh/material, and related semantic data into the importer’s canonical Kandra representation.
3. Produce the packed payload and loose/mod-owned package form used by the public-safe pipeline.
4. Generate metadata required for runtime preflight/registration.
5. Validate payload sizes, offsets, indexes, identity relationships, and any importer-owned checksums/receipts.
6. Re-open/re-parse the generated package through the validator rather than trusting the writer.
7. Keep proof fixtures and target production packages as separate identities.

## Output

A validated Kandra package candidate plus conversion/validation receipt.

## Validation gate

PASSED when the package writer and independent validator agree on the intended geometry/metadata contract.

## Failure conditions

- writer and validator disagree;
- metadata refers to different geometry than the payload;
- proof-fixture identity is reused as a production item;
- package validity is inferred only from “file was created.”

## Does not prove

A valid package does not prove runtime Kandra registration or armour equip.

## Next

Proceed to [Kandra runtime registration](../05-kandra-registration/README.md).
