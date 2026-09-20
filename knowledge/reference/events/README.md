# Events

Use this page when you need to react to FoA state changes through the game's event system instead of polling or patching a method unnecessarily.

It points to the event architecture and the established listener/cleanup patterns used by Models, Elements, Hero state, and global events.

## Public event access patterns

A public Mono mod demonstrates subscription through the global event system:

~~~csharp
World.EventSystem.ListenTo(
    EventSelector.AnySource,
    HealthElement.Events.OnDamageDealt,
    callback)
~~~

It removes the previous listener with:

~~~csharp
World.EventSystem.RemoveListener(listener)
~~~

That mod installs the listener after `Hero.OnFullyInitialized` because the event system/HUD are not ready when the plugin first loads.

Questline public source also demonstrates model-scoped listening:

~~~csharp
Hero.ListenTo(
    Hero.Events.HeroSprintingStateChanged,
    callback,
    owner)
~~~

## Core Model lifecycle events

Preserved developer documentation records these `Model.Events` surfaces:

- `BeforeFullyInitialized`
- `AfterFullyInitialized`
- `AfterChanged`
- `BeforeDiscarded`
- `BeingDiscarded`
- `AfterDiscarded`
- `AfterElementsCollectionModified`

It also documents:

~~~csharp
target.ListenTo(eventDefinition, callback, owner);
target.ListenToLimited(eventDefinition, callback, owner, limit);
target.Trigger(eventDefinition, value);
~~~

and an any-source listener form through the global event system.

Listener ownership matters because Model discard removes listeners owned by the Model and later clears remaining listeners associated with the discarded source/its Elements.

## Useful publicly visible events

| Event | Public use |
| --- | --- |
| `HealthElement.Events.OnDamageDealt` | listen for completed damage outcomes without patching every damage caller |
| `Hero.Events.HeroSprintingStateChanged` | react to hero sprint-state changes |
| `Hero.Events.HideWeapons` / `Hero.Events.ShowWeapons` | publicly triggered by hero input code to change weapon visibility |
| `ItemsUI.Events.ItemsCollectionChanged` | notify inventory UI after batch item movement |

## Event triggering

Questline public source shows `World.EventSystem.Trigger(source, event, payload)` and model-level `Target.Trigger(...)` usage.

Treat event names as ownership/lifecycle clues, not as guarantees that a particular event is safe to synthesize from mods. Verify downstream expectations before triggering native events yourself.

## Cleanup

Keep listener handles and remove owned listeners during replacement/unload where applicable. Public mod code explicitly removes its previous `IEventListener` before installing another.

## Evidence boundary

The Model lifecycle/event list above is supported by preserved developer documentation. The gameplay event examples are supported by public source. Neither lane implies that every native event is safe for mods to synthesize.

See [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
