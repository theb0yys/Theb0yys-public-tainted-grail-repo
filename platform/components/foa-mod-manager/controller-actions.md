# Controller Actions

Register named mod actions instead of synthesizing keyboard input.

```csharp
FoAModManagerApi.RegisterControllerAction(
    "my-mod.open-panel",
    "Open My Panel",
    "My Mod",
    "Open the mod panel.",
    OpenPanel);
```

Unregister the action during teardown:

```csharp
FoAModManagerApi.UnregisterControllerAction("my-mod.open-panel");
```

## Callback rule

Keep callbacks small.

A controller callback should normally request/open the feature; it should not perform long scans, file IO, blocking work or unsafe game mutation inline.
