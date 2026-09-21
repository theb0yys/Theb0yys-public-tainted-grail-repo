# Item Stage 1 — Source and Baseline Selection


## Objective

Choose one native ItemTemplate whose existing component and attachment contract is close to the custom item being introduced.

## Prerequisites

- the target game/runtime build is identified;
- the template provider has a known lookup path;
- the source template GUID is known and can be resolved;
- the source is suitable for redistribution-safe documentation.

## Procedure

1. Select one native item family only.
2. Resolve the exact source GUID through the normal template provider.
3. Record the source template name, GUID, classification, component shape, attachment groups, important inherited behaviour, presentation references, and acquisition assumptions.
4. Do not mutate the source template.
5. Record which source characteristics will remain inherited in the first proof and which fields will be changed later.

The proven Apple lineage used a native cooking apple as the baseline and deliberately preserved native behaviour while proving a separate custom identity.

## Output

A source-profile record containing:

- source template name and GUID;
- game/runtime build scope;
- item family;
- component/attachment topology;
- fields intentionally inherited;
- fields approved for change;
- known downstream owners.

## Check

Confirm that the source GUID resolves to the expected ItemTemplate and the recorded component/profile matches the intended native family.

## Before continuing

This stage does not create a custom item, register anything, create a runtime Item, place it in an inventory/shop, display an icon, or prove persistence.

## Next

Proceed to [custom identity and template construction](../02-template-identity/README.md).
