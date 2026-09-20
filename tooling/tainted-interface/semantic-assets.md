# Semantic UI Assets

Tainted Interface exposes curated resources through stable IDs rather than raw source paths.

## Typical access

```csharp
Texture2D? texture = TaintedInterfaceApi.GetUiTexture("...");
Texture2D? icon = TaintedInterfaceApi.GetIcon("...");
Texture2D? itemIcon = TaintedInterfaceApi.GetItemIcon(itemReference);
```

Catalog APIs let a tool/UI enumerate available descriptors by pack.

## Available pack concept

Examples of shared pack IDs include:

- `dark-fantasy-rpg-ui-toolkit`
- `fantasy-rpg-gui`
- `heat-complete-modern-ui`
- `fantasy-nouveau-ui`
- `tribal-ui-set`
- `game-ui-empty-box-set-4k`
- `fantasy-rpg-ui-kit`

Availability is build/pack dependent. Check descriptors/runtime availability rather than assuming every pack exists.

## Rule

A feature mod should depend on **semantic meaning**, not a private file path such as `Assets/User Interface/...`.
