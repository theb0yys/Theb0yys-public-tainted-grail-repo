# Item Importation Pipeline

This pipeline reconstructs the proven custom-item path as a sequence of independently validated stages.


## Stage map

1. [Source and baseline selection](01-source-baseline/README.md)
2. [Custom identity and template construction](02-template-identity/README.md)
3. [Native template registration](03-registration/README.md)
4. [Runtime Item construction and acquisition](04-runtime-acquisition/README.md)
5. [Assets and presentation](05-presentation/README.md)
6. [Persistence and compatibility](06-persistence/README.md)
7. [Validation matrix](validation/README.md)
8. [Known failure modes](failures/README.md)

## Canonical technical background

- [Items system](../../knowledge/systems/gameplay/items.md)
- [Templates and registries](../../knowledge/systems/core/templates-registries.md)
- [Identity reference](../../knowledge/reference/identities/identity-guids-names.md)
- [Assets reference](../../knowledge/reference/assets/README.md)

## Reproduction target

The first reproducible target should be deliberately narrow: one reviewed native item prototype, one new mod-owned identity, one registration path, and one acquisition owner. Do not begin by importing a batch of unrelated item families.
