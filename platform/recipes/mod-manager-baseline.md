# Recipe: Settings, Controller Action and Status

Use this for a user-facing mod that does not need a custom screen.

A complete source project is available at [examples/mono/infrastructure/mod-manager-baseline](../../examples/mono/infrastructure/mod-manager-baseline/README.md).

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

Use `FoAModManagerApi.RegisterControllerAction(...)` and unregister the same stable action ID on teardown.

Do not synthesize keyboard input when the shared action registry can expose the command directly.

## 3. Add a cheap read-only status provider

Use `RegisterStatusProvider(...)` for dependency readiness, operating mode and concise blocked/error state.

The provider must remain cheap and read-only.

## Ownership

Your mod owns setting meaning and gameplay behaviour.

FoA Mod Manager owns shared settings presentation, controller registration and status presentation.
