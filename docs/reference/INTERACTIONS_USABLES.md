# Interactions, Prompts, Pickups, Containers, and Illegal Actions

> **Reference page.** Use this when modifying click/hold interactions, theft prompts, pickups, containers, readables, harvesting, or interaction authorization.

## What this system is

FoA interaction behavior is layered.

A visible prompt is only one part of the route:

~~~text
interaction owner/action
→ UI representation
→ input/control scheme
→ tap/hold timing
→ authorization
→ StartInteraction / action
→ crime/inventory/world mutation
→ feedback/cleanup
~~~

The Hold-to-Steal research is a strong example of why the **full path** matters.

## Who owns it in FoA

Important owners include:

- `HeroInteraction`;
- `Pickable`;
- `Regrowable`;
- `PickItemAction`;
- `ContainerUI`;
- `PContainerUI`;
- `Prompt`;
- `HeroInteractionHoldUI`;
- `HeroIllegalInteractionUI`;
- `VReadablePopupUI`;
- native Crime/Inventory owners downstream.

## Important identities, types, and methods

Researched targets include:

- `HeroInteraction.StartInteraction`
- `Pickable.StartInteraction`
- `Regrowable.StartInteraction`
- `PickItemAction.OnStart`
- `PickItemAction.DefaultActionName`
- `ContainerUI.TakeItemFromContainer`
- `ContainerUI.TakeAllItems`
- `Prompt.Tap`
- `Prompt.Hold`
- `HeroInteractionHoldUI.Handle(UIEvent)`
- `HeroIllegalInteractionUI`
- `VReadablePopupUI.OnSteal`

Native illegal hold time is exposed through:

~~~text
HeroIllegalInteractionUI.HoldTime
= 0.37f * HeroStats.TheftHoldTimeModifier
~~~

## Where it exists in the lifecycle

### Illegal world interaction

~~~text
IHeroAction / illegal action identified
→ HeroIllegalInteractionUI shown
→ HeroInteractionHoldUI.Handle receives UI events
→ hold timer completes
→ HeroInteraction.StartInteraction
→ concrete Pickable/Regrowable/PickItemAction path
→ Crime commit
→ inventory/world mutation
~~~

### Container theft

~~~text
PContainerUI builds Prompt.Hold
→ native KeyBindings + ControlSchemeFlag
→ hold completes
→ ContainerUI.TakeItemFromContainer / TakeAllItems
→ StolenItemElement / crime
→ Hero inventory transfer
~~~

### Readable theft

~~~text
PickItemAction
→ readable popup
→ VReadablePopupUI.OnSteal
→ crime commit
→ item transferred
~~~

## How we interact with it

### Reuse native Prompt.Hold when the input semantics already exist

`Prompt` carries native KeyBindings and control-scheme handling.

Wrapping the native callback preserves keyboard/controller behavior better than adding a separate keyboard-only polling loop.

### Authorize the actual completed native hold path

Do not infer authorization from visible label alone.

Use the lifecycle point that proves the native hold completed.

### Guard at the mutation/action owner

Presentation can explain the required action, but the final safety gate should live close to the actual interaction/mutation.

### Preserve native crime/inventory behavior

A hold-to-steal feature should change the **authorization/input requirement**, not reimplement stolen-item creation, crime commit, or inventory transfer.

## Why this route

The Hold-to-Steal failure sequence proves the point.

### Attempt 1 — Change visible prompt

Version 0.4.0 showed **Hold to Steal**, but the final theft guard still blocked the action.

**Lesson:** presentation change did not establish authorization.

### Attempt 2 — Wrap converted Prompt action

0.4.1 authorized the converted prompt callback, but direct world-item theft still failed.

**Lesson:** some illegal world interactions used a deeper/direct illegal-interaction hold path rather than only the converted `Prompt.Hold`.

### Attempt 3 — Authorize `HeroIllegalInteractionUI` / `HeroInteractionHoldUI.Handle`

0.4.2 moved authorization to the actual native illegal hold completion path before `HeroInteraction.StartInteraction`.

**Lesson:** lifecycle ownership matters more than visual prompt shape.

### Attempt 4 — Broaden by visible steal label

0.4.3 covered illegal actions beyond the initially enumerated concrete types.

**Lesson:** once the owner path is correct, classification can broaden carefully.

### Attempt 5 — Container Take All

0.4.4 found the illegal Take All prompt displayed pickup-all text, not steal text.

The robust key was the **callback target** (`ContainerUI.TakeAllItems`), not the visible label.

**Lesson:** display text is presentation, not machine identity.

This is exactly the repository's full-proven-path principle in practice.

## What goes wrong

### Prompt label used as authorization identity

Localized/presentation strings are not reliable action identity.

### Tap/hold UI patched without control-scheme ownership

Can work on keyboard while breaking controller.

### Downstream theft code rewritten

Duplicates native Crime/Inventory ownership and creates compatibility/save risk.

### Guard placed before the native hold can ever authorize

Creates a UI that visually asks the user to hold but still rejects the action.

### Container Take All assumed identical to single-item steal

Different callback/label path.

## How to verify

For an interaction modification:

1. identify concrete action owner;
2. identify illegal/legal classification;
3. identify prompt/UI owner;
4. test keyboard;
5. test controller;
6. prove hold/tap completion marker;
7. prove authorization marker;
8. prove action/interaction entry;
9. prove native crime/inventory mutation;
10. test container single/take-all/readable variants if claimed;
11. verify blocked path gives feedback;
12. verify cleanup/no stuck input scope.

## Current proof boundary

The theft/interaction routing above is strongly source/decompile-backed and includes a useful sequence of runtime corrections.

It should not be generalized to every FoA interaction type without checking that type's concrete owner and UI/action path.
