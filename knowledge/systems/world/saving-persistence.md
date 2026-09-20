# Saving and Persistence

Use this page before you claim that custom content **survives save/load, restart, uninstall, downgrade, or a missing dependency**.

A feature working in the current session is not the same as persistence.

A custom item can:

- register successfully;
- appear in inventory;
- work in combat or a shop;

and still fail when the save is loaded later.

## How native template references are saved

For the inspected Mono build, template-backed state uses GUID-based restoration:

~~~text
SaveWriter.WriteTemplate<T>
→ write template GUID

SaveReader.ReadTemplate<T>
→ TemplatesUtil.Load<T>
→ World.Services.Get<TemplatesProvider>()
→ TemplatesProvider.Get<T>(guid)
~~~

For items, static research also shows that item serialization records the template relationship and quantity, and deserialization resolves the saved `ItemTemplate`.

The practical consequence is important:

> If a save refers to your custom template, that template must be registered and resolvable before restore needs it.

Runtime registration that happens too late can still produce a broken load even if the item worked perfectly before saving.

## Session-only Models

The inspected Mono Model contract includes:

- `Model.MarkedNotSaved`;
- `Model.IsNotSaved`;
- `Model.IsValidAfterLoad()`;
- save preparation that skips normal `OnSave()` for not-saved Models.

Some working one-session actor/companion paths deliberately use `Location.MarkedNotSaved = true`.

Use this when the object is intentionally disposable/session-only. It does not replace proper cleanup.

## Native save requests and completion

Useful researched surfaces include:

- `LoadSave.CanAutoSave()` — native save guard;
- `LoadSave.Save(SaveSlot, bool)` — native save request;
- `LoadSave.QuickSave()` — native quicksave request;
- concrete cloud-service `EndSave(string)` implementations — completed slot-write observation points.

A callback that observes a completed native slot write is **not** a general custom serialization API.

## Custom native save domains

Current Mono research did not recover a supported mutable registrar for arbitrary mod-owned native save domains.

The native domain set appears to be game-owned. Unknown `<name>.data` payloads may pass through parts of the archive/cache path, but that does not mean native restore will deserialize arbitrary mod data.

So do not design a persistence system around the assumption that you can simply "register another native save domain."

## Sidecar state

For mod-owned data that does not naturally belong to an existing native object/template, a separate namespace-isolated sidecar is a reasonable research direction.

That requires its own answers for:

- when to write;
- how to associate sidecar data with a save;
- when it is safe to apply after load;
- duplicate/replay behavior;
- schema migration;
- missing/corrupt sidecar handling;
- uninstall behavior.

The general sidecar architecture is still under evaluation and should not be described as universally proven.

## Designing durable custom content

Before calling a feature persistent, decide:

- stable identity;
- when the definition is registered;
- what native or mod-owned system saves the state;
- what happens if the mod is missing;
- whether repeated load/apply is idempotent;
- how schema/identity changes migrate;
- uninstall/orphan behavior;
- rollback strategy.

If persistence is not required, make that explicit and keep the object/session state disposable.

## Common failure cases

- the save contains a custom template GUID but the mod is missing;
- custom registration happens after restore has already tried the GUID;
- the GUID changes between versions;
- two mods claim the same GUID;
- a runtime-only recipe/item is mistaken for persistent content;
- a session-only actor accidentally enters save-owned state;
- a save-request hook is mistaken for disk-write success;
- a mod patches global serializers without a proven ownership/compatibility contract;
- sidecar data is applied before the native world is ready.

## How to prove persistence

Use a disposable test save and verify each stage:

1. create/use the custom content;
2. save;
3. exit the game completely;
4. restart;
5. load with the same mod/version;
6. verify identity and behavior;
7. repeat save/load to catch duplication;
8. test the documented missing/disabled-mod case;
9. test migration if IDs/schema changed;
10. record the exact game, loader, mod versions, and relevant hashes.

If you did not test a cold restart/load, do not call the feature restart-safe.

## Evidence limits

Custom template GUID serialization/lookup and the `MarkedNotSaved` Model contract are supported by exact Mono static research.

The public custom-item route does **not** currently prove universal missing-mod, uninstall, downgrade, or cold-save safety.
