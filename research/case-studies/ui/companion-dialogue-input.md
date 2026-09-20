# Companion Dialogue Input Ownership

The companion UI work exposed a common failure: a surface can render correctly while its input path is broken.

## Initial mismatch

The custom dialogue-style surface moved from IMGUI to Unity UI, but Unity UI has a different dispatch requirement. A debug-panel strategy that disables `BaseInputModule` is not transferable to a Unity `Button` surface.

## Correct model

- native/world prompt remains the entry point;
- plugin-owned Unity UI host renders choices;
- gameplay input is suppressed while modal UI is open;
- cursor ownership is acquired;
- EventSystem/input modules remain active for the Unity UI surface;
- Rewired/UI reads required by buttons are allowed through;
- close/Goodbye/command completion restore input/cursor state.

## Lesson

When a UI opens but the handler never fires, compare the entire control path before editing command code.
