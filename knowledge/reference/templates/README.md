# Templates

Use this page when you need to resolve, enumerate, or reason about FoA templates.

For how the template system is loaded and owned, see [Templates/registries](../../systems/core/templates-registries.md). For GUID and identity rules, see [Identities](../identities/README.md).

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
- typed enumeration → `TemplatesProvider.GetAllOfType<T>()`
- `TemplateReference` → `TryGet<T>()` / `Get<T>()` where appropriate

## Useful related types

- `TemplatesProvider`
- `Template`
- `TemplateReference`
- `ItemTemplate`
- `StatusTemplate`
- `NpcTemplate`
- `CraftingTemplate`
- `CommonReferences`

## Current Mono static contract

Exact-build decompilation establishes:

~~~text
TemplatesLoader.CreateAndLoad
→ LoadAssets
→ LoadAssetsInBuild
→ process Addressables label "template"
→ process Addressables label "templateSO"
→ AddToMap(guid, template)
→ FinishedLoading = true
~~~

`TemplatesLoader` owns a GUID map and a type map. `AddToMap` inserts the template into both and assigns `template.GUID`.

`TemplatesProvider.AllLoaded` reflects loader completion. Its typed lookup validates readiness, GUID presence and requested type.

For saved template references, the inspected Mono chain is:

~~~text
saved template GUID
→ SaveReader.ReadTemplate<T>()
→ TemplatesUtil.Load<T>()
→ World.Services.Get<TemplatesProvider>()
→ TemplatesProvider.Get<T>(guid)
~~~

This is static contract evidence, not a universal persistence-safety claim for custom templates.

## Readiness

Knowing a GUID and knowing the provider type is not enough if templates are still loading.

The repository already tracks `TemplatesLoader.set_FinishedLoading(bool)` as an important readiness boundary. Public mods independently demonstrate that `TemplatesProvider` can be unavailable when accessed too early.

## Runtime-specific caution

A public IL2CPP-native implementation observed stale/freed pointers inside `CraftingTemplate.recipes` (`TemplateReference[]`) during direct native traversal. That memory-lifetime observation is specific to the native IL2CPP access lane; do not generalize it to ordinary managed references.

## Evidence boundary

Exact template GUIDs belong in identity/domain reference pages. This page owns lookup mechanics and readiness, not a bulk template dump.

The exact loader/provider/save-resolution details are bound to the inspected Mono evidence recorded in [Internal Evidence Intake Baseline](../../../research/sources/internal-evidence-baseline.md).
