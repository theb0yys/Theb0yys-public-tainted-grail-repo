# Modal UI Opens but Buttons Do Not Fire

Use this when a custom Unity UI screen renders correctly, the cursor may even be visible, but button handlers never run.

Working lesson: [Companion Dialogue Input Ownership](../../../research/case-studies/ui/companion-dialogue-input.md).  
Canonical owner model: [UI, Cursor, Focus, and Input Ownership](../../../knowledge/systems/presentation/ui-input.md).

## Symptom

```text
screen visible
+ button visible
+ cursor visible
but
Button.onClick never reaches handler
```

Do not immediately rewrite the command/gameplay code.

## Common cause

A control strategy copied from an IMGUI/debug surface can disable the Unity UI dispatch path.

Unity `Button` requires an active EventSystem/input-module route. If a modal implementation disables `BaseInputModule` or blocks the Rewired/UI reads used by the screen, the button can render but never receive activation.

## Correct ownership model

A modal Unity UI screen should:

- acquire gameplay input/cursor ownership;
- suppress world movement/look as required;
- keep EventSystem/input modules active;
- allow UI-specific Rewired/input reads;
- keep a usable cursor;
- route mouse/controller/keyboard selection into one action handler;
- restore prior input/cursor state on every close path.

## Diagnostic sequence

1. prove the screen opened;
2. prove EventSystem exists;
3. prove required input module remains enabled;
4. prove pointer/focus moves;
5. prove Unity `Button.onClick` receives activation;
6. only then inspect the command handler.

## Common anti-pattern

```text
modal opens
→ disable all input modules
→ gameplay stops
→ UI stops too
```

Blocking gameplay input and disabling UI dispatch are not the same operation.

## Verify the repair

Test:

- mouse click;
- keyboard focus/activation;
- controller focus/activation;
- gameplay movement blocked behind the modal;
- handler fires exactly once;
- Back/Escape restores input;
- scene/plugin teardown restores state.

## Evidence boundary

This case is a failure/correction lesson from moving a companion dialogue surface from IMGUI-style assumptions to Unity UI. It establishes the input-ownership model; each new screen still needs its own live input/restoration proof.
