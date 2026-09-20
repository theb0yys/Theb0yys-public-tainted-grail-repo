# Investigating a .arch File

Use this when you encounter a FoA `.arch` file and need to determine what it contains.

Canonical architecture: [Serialization and .arch Packaging](README.md).

## Do not begin with the extension

The safe sequence is:

```text
archive path
→ owning system
→ mount/alias convention
→ inner payload reader
→ payload records/layout
```

The `.arch` extension establishes the outer Unity Archive container only.

## Investigation procedure

1. Find which managed/editor system references the archive path.
2. Identify its runtime mount call.
3. Find the resource aliases/virtual paths requested after mount.
4. Trace the exact reader/parser.
5. Record expected headers/counts/record structures.
6. Compare build-side writer/processor when available.
7. Validate one known payload before generalizing the schema.

## Examples of separate inner formats

Known consumers include Story, Babel, MergedDrake, Medusa and HLOD-related data.

Their archive transport can be similar while their payload formats are unrelated.

## Writer reconstruction rule

Do not write a replacement archive from reader guesses alone when:

- section ordering is unresolved;
- alignment/packing is unresolved;
- version fields are unknown;
- resource aliases are unknown;
- build-time preprocessing is part of the contract.

A parser that can read some records is not proof that a compatible writer is known.

For mount mechanics see [Archive IO](../archive-io/README.md).
