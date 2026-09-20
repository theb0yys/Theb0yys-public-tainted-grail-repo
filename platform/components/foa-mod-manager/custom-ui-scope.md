# Shared Custom UI Scope

Use:

`FoAModManagerApi.SetCustomUiScope(ownerId, active, freezeWorld)`

when opening a mod-owned interactive screen.

## Modal screen

```text
open
→ SetCustomUiScope(id, true, true)
→ create/show your screen
→ handle your own widgets/commands
→ destroy/close your screen
→ SetCustomUiScope(id, false, true)
```

The shared scope can own:

- gameplay input suppression;
- cursor unlock/visibility;
- controller cursor;
- optional world freeze;
- restoration after the final owner releases.

For a world-running overlay, request `freezeWorld:false`.

## Controller cursor reads

If your mod patches Rewired/input locally, allow the manager's internal cursor-read window through when:

`FoAModManagerApi.IsControllerCursorInputReadActive == true`

## Do not duplicate restoration

Do not keep a second “previous cursor state” system in every consumer. Let the shared scope own global restoration; your screen owns only its own view/event lifecycle.
