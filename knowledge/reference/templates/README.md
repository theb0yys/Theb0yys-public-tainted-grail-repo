# Templates

Use this page when you need to **find, enumerate, or resolve FoA template definitions** such as items, NPCs, statuses, or crafting definitions.

For how templates are loaded and registered, see [Templates and Registries](../../systems/core/templates-registries.md). For exact IDs, see [Identities](../identities/README.md).

## Look up a template by GUID

Public Mono mods use:

~~~csharp
TemplatesProvider provider = World.Services?.Get<TemplatesProvider>();
T template = provider.Get<T>(guid);
~~~

Do not call `Get<T>` until the provider is ready.

## Resolve a TemplateReference

Public code also uses typed references:

~~~csharp
StatusTemplate status =
    CommonReferences.Get?.OverEncumbranceStatus.TryGet<StatusTemplate>();
~~~

Useful distinctions:

- exact GUID → `TemplatesProvider.Get<T>(guid)`
- all loaded templates of a type → `TemplatesProvider.GetAllOfType<T>()`
- existing `TemplateReference` → `TryGet<T>()` or `Get<T>()`

## Common template types

- `Template`
- `TemplateReference`
- `ItemTemplate`
- `StatusTemplate`
- `NpcTemplate`
- `CraftingTemplate`
- `CommonReferences`

## When are templates ready?

In the inspected Mono build:

~~~text
TemplatesLoader.CreateAndLoad
→ LoadAssets
→ LoadAssetsInBuild
→ load "template"
→ load "templateSO"
→ AddToMap(guid, template)
→ FinishedLoading = true
~~~

`TemplatesProvider.AllLoaded` reflects that loader state.

The loader maintains both a GUID map and a type map. `AddToMap` inserts the template into those maps and assigns its GUID.

## Saved template references

The inspected Mono save-resolution path is:

~~~text
saved template GUID
→ SaveReader.ReadTemplate<T>()
→ TemplatesUtil.Load<T>()
→ World.Services.Get<TemplatesProvider>()
→ TemplatesProvider.Get<T>(guid)
~~~

This is why custom definitions that need to survive save/load must be registered early enough to resolve during restoration.

It does **not** prove that every custom-template registration method is save-safe.

## IL2CPP-native caution

A public native IL2CPP implementation encountered stale/freed pointers while traversing `CraftingTemplate.recipes` / `TemplateReference[]`.

That is a native-memory-lifetime problem. Do not generalize it to normal managed references, but do not assume pointer/class checks are sufficient either.

## What belongs elsewhere?

Exact template GUIDs belong in identity/reference pages.

This page is for **lookup, enumeration, readiness, and resolution behavior**, not a bulk dump of template values.

## Evidence

The exact loader/provider/save-resolution details are bound to the inspected Mono evidence in [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
