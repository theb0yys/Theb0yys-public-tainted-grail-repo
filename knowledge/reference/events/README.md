# Events

Event architecture: [MVC/models/elements/events](../../systems/core/mvc-models-elements-events.md).

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

This is a selective modding reference, not a dump of every `Events` nested type in the game.
