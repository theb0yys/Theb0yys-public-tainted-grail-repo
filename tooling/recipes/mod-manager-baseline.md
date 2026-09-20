# Recipe: Settings, Controller Action and Status

Use this baseline for a user-facing mod that does not need a custom screen.

```text
BepInEx Config.Bind
→ FoA Mod Manager displays settings
→ optional RegisterControllerAction
→ optional RegisterStatusProvider
→ unregister actions/providers on teardown
```

## Ownership

Your mod owns config meaning and feature behaviour.

FoA Mod Manager owns shared presentation/controller/status infrastructure.

Do not create a second settings UI unless the feature genuinely requires a custom screen.
