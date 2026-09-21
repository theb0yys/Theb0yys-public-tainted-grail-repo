# Item Stage 2 — Custom Identity and Template Construction

## Objective

Create a separate mod-owned ItemTemplate identity while preserving the reviewed native contract needed by the first proof.

## Inputs

- PASSED source profile from Stage 1;
- a stable mod-owned GUID;
- a stable template name;
- bounded presentation/economy changes for the first proof.

## Procedure

1. Clone the reviewed source template GameObject rather than editing the native source.
2. Obtain the cloned ItemTemplate component.
3. Assign a new stable custom GUID and a new template name.
4. Confirm the source GUID and source object remain unchanged.
5. Preserve the native component and attachment topology unless a separately evidenced change requires otherwise.
6. Change only the minimum fields required by the proof.
7. Run shape/profile checks before attempting registration.

A separate identity is mandatory. Reusing a native GUID turns an import into a collision or replacement problem.

## Output

An unregistered custom ItemTemplate candidate with a unique identity and a recorded source lineage.

## Validation gate

PASSED when:

- custom GUID differs from the native source;
- source remains unchanged;
- clone contains the expected ItemTemplate;
- required component/attachment shape is present;
- no duplicate custom identity is already owned by another package.

## Does not prove

A valid clone is not registered and cannot be assumed to resolve through the game's template provider.

## Next

Proceed to [native template registration](../03-registration/README.md).
