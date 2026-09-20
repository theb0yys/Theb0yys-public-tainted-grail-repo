# Settings Integration

Use normal BepInEx `Config.Bind<T>` entries.

FoA Mod Manager can present those entries without your mod directly creating manager UI.

## Good config design

- stable section/key names;
- meaningful descriptions;
- acceptable ranges/lists where appropriate;
- `KeyCode` for keyboard bindings;
- safe defaults;
- no expensive side effects from simple reads.

Optional UI metadata may provide display section/name, ordering, choice labels and hidden/internal flags without changing the saved value.

## Rule

The manager is the **presentation/discovery owner** for shared config UI.

Your mod remains the owner of:

- config definition;
- validation;
- feature application;
- rollback/default semantics.
