# FoA Mod Manager

Use FoA Mod Manager when your mod needs shared settings UI, controller actions, custom-screen input/cursor handling, or a read-only runtime status entry.

These integrations are intended for ordinary mod authors. Your mod still owns its gameplay state and its own screen contents; the manager owns only the shared management and input responsibilities documented below.

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
