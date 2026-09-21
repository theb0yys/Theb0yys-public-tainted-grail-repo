# FoA Mod Manager

**Status:** Ready for mod authors.

Use FoA Mod Manager when your mod needs one or more of:

- normal BepInEx config shown in a common manager;
- display metadata for config entries;
- controller-triggered mod actions;
- shared cursor/input/world-freeze handling for a custom screen;
- controller cursor support;
- a read-only runtime status row.

## Public API

Use `FoAModManager.FoAModManagerApi`.

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

## What the manager handles

FoA Mod Manager handles shared management/UI input concerns.

It does not own:

- your gameplay feature state;
- your save data;
- your custom screen's internal widgets/commands;
- Tainted Interface visual resources;
- Avalon Core capability data;
- AI decisions.
