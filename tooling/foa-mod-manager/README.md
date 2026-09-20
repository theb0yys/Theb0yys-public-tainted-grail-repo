# FoA Mod Manager

**Posture: Author-ready**

**Obtain:** https://www.nexusmods.com/taintedgrailthefallofavalon/mods/167

**Stability boundary:** the documented `FoAModManagerApi` members are a supported public surface. Do not infer a permanent ABI guarantee for undocumented manager internals. Record the exact package version tested before declaring a minimum dependency.

See [distribution/versioning](../ecosystem/distribution-and-versioning.md) and [API stability](../ecosystem/api-stability.md).

Use FoA Mod Manager when your mod needs one or more of:

- normal BepInEx config shown in a common manager;
- display metadata for config entries;
- controller-triggered mod actions;
- shared cursor/input/world-freeze ownership for a custom screen;
- controller cursor support;
- a read-only runtime status row.

## Public API

The public surface is `FoAModManager.FoAModManagerApi`.

Useful members include:

- `SetCustomUiScope(ownerId, active, freezeWorld)`
- `SetControllerCursorScope(ownerId, active)`
- `RegisterControllerAction(...)`
- `UnregisterControllerAction(...)`
- `RegisterStatusProvider(...)`
- `UnregisterStatusProvider(...)`
- `IsCustomUiScopeActive`
- `IsModUiInputOwned`
- `IsControllerCursorInputReadActive`

## Start with normal BepInEx config

For ordinary settings, use `Config.Bind<T>` first.

The manager discovers normal BepInEx configuration. A mod should not invent a parallel settings file merely to appear in the manager.

See:

- [Settings integration](settings.md)
- [Custom UI scope](custom-ui-scope.md)
- [Controller actions](controller-actions.md)
- [Status providers](status-providers.md)

## Ownership boundary

FoA Mod Manager owns **shared management/UI input concerns**.

It does not own:

- your gameplay feature state;
- your save data;
- your custom screen's internal widgets/commands;
- Tainted Interface visual resources;
- Avalon Core capability truth;
- AI decisions.
