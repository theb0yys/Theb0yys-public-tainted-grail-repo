# UI, Input, Focus, and Command Routing

> **Reference page.** Use this before copying a working menu, adding an overlay/modal, or diagnosing a panel that opens but does not respond correctly.

## What this system is

A UI path is more than visible GameObjects.

A complete interactive path includes:

~~~text
owner/scope
→ open lifecycle
→ input/control routing
→ cursor/focus ownership
→ event dispatch
→ handler entry
→ command/state mutation
→ close lifecycle
→ cleanup/state restoration
→ validation markers
~~~

## Who owns it in FoA

Ownership depends on the surface:

- native FoA screen/UI owner;
- Unity EventSystem/input modules;
- game controller/input routing;
- native action dispatcher;
- shared mod UI manager where explicitly used;
- plugin-owned overlay/modal for truly mod-owned presentation.

The gameplay system should remain authoritative for gameplay truth unless a proven replacement path says otherwise.

## Important identities, types, and methods

Useful concepts from the research include:

- screen/view owner;
- focus target;
- cursor/input scope;
- EventSystem/input-module route;
- controller route/device identity;
- semantic navigation;
- native action/command dispatcher;
- close/back/`Esc` path;
- subscriptions/listeners;
- restoration lease/state.

## Where it exists in the lifecycle

A correct modal/screen integration generally requires:

~~~text
request/open scope
→ acquire cursor/input/focus ownership
→ populate/bind data
→ route pointer/keyboard/controller events
→ dispatch through intended handler
→ close/back/fault/scene change
→ release ownership and subscriptions
→ restore only mod-owned changes
~~~

## How we interact with it

### Map the full path before copying UI behavior

Do not copy only:

- the button;
- the click handler;
- the visible panel;
- the method called at the end.

Map ownership and routing from entry to command.

### Keep presentation and authority separate

A mod-owned inventory presentation can still read native item truth and dispatch native actions instead of becoming a second inventory authority.

### Hide only what you can restore safely

Build the replacement presentation first.

Then suppress only validated visual/hit-testing/focus surfaces.

Do not disable a GameObject merely because it visually contains the old UI if it also hosts state/logic required by the game.

### Distinguish hover, focus, selection and action

These are separate states, especially across pointer/controller navigation.

## Why this route

The working rules explicitly state:

> if the surface opens but click/selection/dialogue-choice/command/handler markers do not fire, treat the problem as upstream ownership/input routing until proven otherwise.

Editing the downstream handler in that state attacks the wrong layer.

## What goes wrong

### "Menu opens" treated as success

Open lifecycle can succeed while input is still routed elsewhere.

### Copied handler without dispatcher route

Directly calling a handler may bypass validation, ownership, state or cleanup the native path performs before it.

### Hidden native controls still receive input

Visual suppression without hit-testing/focus suppression can create double actions.

### Over-broad GameObject disabling

Can remove model hosts, event owners or data/state behavior you still depend on.

### Cursor/focus leak

A fault/scene transition closes visible UI but leaves input/cursor state modified.

### Controller support inferred from mouse support

Pointer, keyboard and controller navigation can have different ownership/routes.

## How to verify

A full UI path should prove:

1. intended owner opens;
2. correct input source/controller reaches it;
3. focus/cursor state changes as designed;
4. selection/click marker fires;
5. handler entry fires;
6. command/native mutation fires;
7. visible state updates;
8. Back/`Esc` works;
9. close/fault/scene transition releases scope;
10. native state is restored;
11. no hidden duplicate input remains.

## Current proof boundary

This page records the repository's full proven-path rule and repeated UI research conclusions. Exact screen/input APIs vary by surface and must be established for each UI integration.
