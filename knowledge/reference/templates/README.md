# Templates

Canonical architecture: [Templates/registries](../../systems/core/templates-registries.md). Identity rules: [Identities](../identities/README.md).

## Public lookup patterns

Public Mono mods demonstrate direct typed lookup:

~~~csharp
TemplatesProvider templatesProvider = World.Services?.Get<TemplatesProvider>();
T template = templatesProvider.Get<T>(guid);
~~~

They fail closed when `TemplatesProvider` is not available yet.

Public code also demonstrates typed template references:

~~~csharp
StatusTemplate status =
    CommonReferences.Get?.OverEncumbranceStatus.TryGet<StatusTemplate>();
~~~

The exact null-handling differs by caller, but the useful distinction is:

- GUID → `TemplatesProvider.Get<T>(guid)`
- `TemplateReference` → `TryGet<T>()`

## Useful related types

- `TemplatesProvider`
- `Template`
- `TemplateReference`
- `ItemTemplate`
- `StatusTemplate`
- `NpcTemplate`
- `CraftingTemplate`
- `CommonReferences`

## Readiness

Knowing a GUID and knowing the provider type is not enough if templates are still loading.

The repository already tracks `TemplatesLoader.set_FinishedLoading(bool)` as an important readiness boundary. Public mods independently demonstrate that `TemplatesProvider` can be unavailable when accessed too early.

## Runtime-specific caution

A public IL2CPP-native implementation observed stale/freed pointers inside `CraftingTemplate.recipes` (`TemplateReference[]`) during direct native traversal. That memory-lifetime observation is specific to the native IL2CPP access lane; do not generalize it to ordinary managed references.

## Evidence boundary

Exact template GUIDs belong in identity/domain reference pages. This page owns lookup mechanics and readiness, not a bulk template dump.
