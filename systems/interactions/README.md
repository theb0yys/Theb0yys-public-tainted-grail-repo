<!-- Canonical Wave 5 native-system page split from docs/reference/INTERACTIONS_USABLES.md. -->
# Interactions, Prompts, Pickups, Containers, and Illegal Actions

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/interactions/interaction-modification.md).

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

## Current proof boundary

The theft/interaction routing above is strongly source/decompile-backed and includes a useful sequence of runtime corrections.

It should not be generalized to every FoA interaction type without checking that type's concrete owner and UI/action path.
