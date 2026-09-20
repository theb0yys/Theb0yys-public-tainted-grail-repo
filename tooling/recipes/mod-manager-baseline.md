# Recipe: Settings, Controller Action and Status

Use this for a user-facing mod that does not need a custom screen.

## 1. Define ordinary BepInEx config

```csharp
private ConfigEntry<bool> _enabled = null!;

private void Awake()
{
    _enabled = Config.Bind(
        "General",
        "Enabled",
        true,
        "Enable Example Mod.");
}
```

FoA Mod Manager can discover normal `Config.Bind` entries without a manager API call.

## 2. Add a controller action only if useful

```csharp
private const string ActionId = "author.foa.example.toggle";

private void Awake()
{
    FoAModManagerApi.RegisterControllerAction(
        ActionId,
        "Toggle Example",
        "Example Mod",
        "Toggle the feature.",
        ToggleFeature);
}

private void OnDestroy()
{
    FoAModManagerApi.UnregisterControllerAction(ActionId);
}
```

Do not synthesize keyboard input when the shared action registry can expose the command directly.

## 3. Add a cheap read-only status provider

```csharp
private const string StatusId = "author.foa.example.status";

private FoAModStatusSnapshot BuildStatus() => new()
{
    Level = FoAModStatusLevel.Ok,
    Summary = _enabled.Value ? "Enabled" : "Disabled",
    Schema = "example-status/1",
    UpdatedUtc = DateTime.UtcNow.ToString("O")
};
```

Register/unregister it with `FoAModManagerApi.RegisterStatusProvider` / `UnregisterStatusProvider`.

## Ownership

Your mod owns setting meaning and gameplay behaviour.

FoA Mod Manager owns shared settings presentation, controller registration and status presentation.
