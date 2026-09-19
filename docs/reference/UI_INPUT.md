# UI, Cursor, Focus, and Input Ownership

> **Reference/process page.** A custom screen is not complete when it renders. A reusable UI path must own open → input/focus → dispatch → close → restoration.

## What this system is

FoA custom UI sits on top of several independent systems:

- Unity Canvas/IMGUI/EventSystem;
- FoA player/camera input;
- Rewired controller input;
- cursor lock/visibility;
- world-time/freeze policy;
- native modal/UI state;
- custom screen selection/focus;
- action dispatch;
- teardown/restoration.

A visible panel can still be unusable or leave the game stuck after close.

## Who owns it in FoA

Important proven/researched owners include:

- FoA/Unity cursor state;
- FoA player input;
- Rewired;
- Unity EventSystem/input modules;
- native `ForceCursorVisibility` element;
- shared mod UI-scope services exposed by Tainted Interface / FoA Mod Manager in the working ecosystem;
- the custom screen owner itself for its objects, focus, subscriptions and command dispatch.

## Important identities, types, and methods

Bounded working/research surfaces include:

- Tainted Interface `BeginCustomUiScope` soft-reflection route;
- FoA Mod Manager `SetCustomUiScope(ownerId, active, freezeWorld)` fallback;
- FoA Mod Manager `SetControllerCursorScope` older compatibility route;
- native `ForceCursorVisibility`;
- `Cursor.lockState` / `Cursor.visible`;
- Unity `BaseInputModule`;
- `Input.ResetInputAxes()`;
- FoA `GameUI.UpdateMousePosition`;
- `PlayerInput.ProcessLateUpdate`;
- Rewired player axis/button reads;
- Unity `Button.onClick`;
- real `Input.mousePosition`;
- bounded manual `Input.GetMouseButtonDown(0)` hit-test fallback.

## Where it exists in the lifecycle

A complete modal screen path is:

~~~text
open request
→ acquire UI/input/cursor scope
→ capture previous state
→ make cursor usable
→ block gameplay movement/look/input
→ allow UI/EventSystem/controller reads
→ establish initial focus/selection
→ dispatch actions
→ maintain scope while visible
→ close/forced-close
→ unsubscribe/destroy owned UI
→ release scope
→ restore cursor/input/world state
~~~

Every exit path matters: normal close, Escape/Back, scene transition, dependency failure, plugin disable and destroy.

## How we interact with it

### Use one shared scope owner

Acquire once on open and release once on close/destroy.

Do not independently fight cursor/input state every frame if a shared owner already exists.

### Capture and restore prior state

Never assume the previous cursor lock/visibility or world time was a default value.

### Block gameplay while preserving UI reads

A modal screen usually needs to suppress camera/player movement while allowing Rewired/EventSystem input needed by the custom menu.

### Route all selection to one handler

Mouse, controller, keyboard and manual fallback should converge on the same command/action path.

### Keep the gameplay source of truth outside the UI

The screen projects native/mod state; it should not invent a second inventory/quest/companion model.

## Why this route

The UI research includes several failed approaches:

### Cursor visibility only

Failed conceptually because FoA still consumes gameplay camera/player/Rewired input.

**Lesson:** cursor state and gameplay input ownership are separate.

### Reasserting cursor every frame through the wrong scope

Produced a cursor that flashed/recentered.

**Lesson:** use a coherent shared scope instead of competing cursor owners.

### Manager-first/incorrect scope ordering in the companion menu

The menu could open but command selection remained broken.

**Correction:** copy the complete proven path, including the scope order and cursor loop, not merely the presence of a custom scope call.

### Mod-owned virtual cursor

Tried as a workaround for centered native pointer behavior, but later work returned to the proven real-pointer path.

**Lesson:** do not build a second input abstraction to compensate for missing ownership if the correct owner path can be recovered.

### UI "attached" marker without native interactability

An action element existed, but the native interaction scanner could not start it.

**Lesson:** visible metadata/registration is not full entry-path ownership.

## What goes wrong

- cursor flashes/recenters;
- camera/player still moves behind the menu;
- Rewired reads are suppressed for the UI itself;
- controller can move focus but cannot activate;
- duplicate mouse/controller dispatch fires twice;
- close restores the wrong cursor/timeScale;
- native input module remains disabled;
- scene transition leaves custom screen/subscriptions alive;
- a custom screen duplicates native inventory/gameplay state;
- screen-open logs are treated as full UI proof.

## How to verify

A complete UI proof should test:

1. open entry point;
2. one and only one scope owner;
3. cursor visible/unlocked and stable;
4. mouse movement/click;
5. keyboard focus/navigation;
6. controller navigation/activation;
7. gameplay camera/movement blocked when required;
8. EventSystem/Rewired UI reads still work;
9. action reaches the intended handler exactly once;
10. Back/Escape/close;
11. scene/load forced close;
12. plugin disable/destroy;
13. cursor/input/world state restored;
14. subscriptions and objects removed.

## Current proof boundary

The working repository has strong source/proven-path evidence for the **ownership model** and multiple live failures that established the rules.

Individual shared scope APIs and companion-menu implementations have their own version/runtime evidence. A fresh UI implementation still requires its own live mouse/keyboard/controller/open-close-restoration proof before being called release-ready.
