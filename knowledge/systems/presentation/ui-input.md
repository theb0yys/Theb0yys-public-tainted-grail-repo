# UI, Cursor, Focus, and Input

Use this page when you are building a **custom screen, menu, HUD interaction, controller UI, or any mod UI that temporarily takes control away from normal gameplay**.

The main rule is:

> A UI is not finished when it renders. It must also own input while open and restore the previous game state when it closes.

## What a complete modal UI needs to handle

A custom screen can touch several independent systems:

- Unity Canvas / IMGUI / EventSystem;
- FoA player and camera input;
- Rewired controller input;
- cursor lock/visibility;
- world pause/freeze policy;
- native modal/UI state;
- selection/focus;
- action dispatch;
- teardown/restoration.

A panel can look correct and still be unusable or leave the game stuck after closing.

## Useful proven/researched pieces

Examples include:

- Tainted Interface `BeginCustomUiScope`;
- FoA Mod Manager `SetCustomUiScope(ownerId, active, freezeWorld)`;
- older `SetControllerCursorScope` compatibility route;
- native `ForceCursorVisibility`;
- `Cursor.lockState` / `Cursor.visible`;
- Unity `BaseInputModule`;
- `Input.ResetInputAxes()`;
- FoA `GameUI.UpdateMousePosition`;
- `PlayerInput.ProcessLateUpdate`;
- Rewired axis/button reads;
- Unity `Button.onClick`;
- `Input.mousePosition`;
- bounded manual mouse hit-test fallback.

These are ingredients. A new screen still needs one coherent lifecycle.

## Recommended screen lifecycle

~~~text
open request
→ acquire UI/input/cursor ownership
→ capture previous state
→ show/unlock cursor
→ block gameplay movement/look as needed
→ keep UI/EventSystem/controller input working
→ set initial focus
→ dispatch commands
→ maintain scope while visible
→ close / forced close
→ unsubscribe/destroy owned UI
→ release scope
→ restore previous cursor/input/world state
~~~

Every exit path matters:

- normal Close button;
- Escape/Back;
- scene transition;
- dependency failure;
- plugin disable;
- plugin destruction.

## Use one scope owner

If FoA Mod Manager or another reviewed shared UI service already handles cursor/freeze/input ownership, acquire that once and release it once.

Do not have several mods/components reassert cursor state independently every frame.

Competing owners are how you get flickering/recentering cursors and broken restoration.

## Capture before you change

Before opening, record the state you intend to restore.

Do not assume:

- cursor was locked;
- cursor was hidden;
- time scale was 1;
- a specific input module was enabled;
- gameplay was unpaused.

Restore the actual prior state where the chosen shared owner does not already handle it.

## Block gameplay without blocking the menu

A modal screen often needs:

~~~text
gameplay look/movement blocked
+
UI mouse/keyboard/controller still active
~~~

Those are separate input concerns.

If you simply disable a broad input system, you may also disable the UI itself.

## Route input to one action handler

Mouse click, keyboard activation, controller submit, and any fallback should converge on the same command path.

Avoid separate code paths that can fire the same action twice.

## Keep gameplay state outside the UI

A screen may own:

- layout;
- selection;
- filters;
- view models;
- temporary UI-only preferences.

It should not become a second source of truth for inventory, quests, companions, or other native gameplay state.

## Lessons from failed approaches

### Cursor visibility only

Making the cursor visible does not stop FoA from consuming camera/player/Rewired input.

### Reasserting cursor through competing owners

Produced unstable/recentering cursor behavior.

### Partial custom-scope integration

A menu can open while command selection still fails if focus/input ownership is incomplete.

### Virtual cursor workaround

A mod-owned virtual cursor can mask the real ownership problem instead of fixing it.

### "Attached" UI metadata treated as interactability

An element can exist in the hierarchy while the native scanner/input path still cannot activate it.

## What to verify

Test:

1. open entry point;
2. only one scope owner;
3. cursor visible/unlocked/stable;
4. mouse movement/click;
5. keyboard navigation/activation;
6. controller navigation/activation;
7. gameplay camera/movement blocked when required;
8. UI input still works;
9. each action dispatches exactly once;
10. Back/Escape/close;
11. scene/load forced close;
12. plugin disable/destroy;
13. prior cursor/input/world state restored;
14. subscriptions/GameObjects removed.

## Evidence limits

The repository has strong source and failure-driven evidence for this ownership model.

Specific shared UI APIs have their own runtime/version requirements, and each new screen still needs a live mouse/keyboard/controller/open-close test before being called complete.
