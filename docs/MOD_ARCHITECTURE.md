# Mod Architecture

A small mod is easier to debug when responsibilities are separated.

## Suggested shape

```text
MyMod/
  Plugin.cs          BepInEx entry point
  Config.cs          user-facing configuration
  Patches/           Harmony patches
  Services/          reusable logic not tied to a patch
  Diagnostics/       bounded logging/health checks
```

For tiny mods, keep this simpler rather than creating empty layers.

## Entry point

The entry point should:

1. bind configuration;
2. create required services;
3. install patches;
4. emit one concise startup line;
5. clean up patches/resources when the loader supports unload.

Avoid putting large gameplay systems directly in the plug-in startup method.

## Harmony

A good patch:

- targets one understood method;
- changes the minimum necessary behavior;
- validates assumptions;
- fails visibly rather than silently corrupting state;
- does not depend on fragile display names when a stronger identity exists.

Prefer postfix/prefix patches when sufficient. Use transpilers only when the behavior cannot be expressed safely otherwise.

## Configuration

Configuration should have:

- stable keys;
- meaningful defaults;
- descriptions;
- bounded numeric/string values where practical.

Do not treat config as a secret store.

## Logging

Use levels intentionally:

- Info: startup, important state changes;
- Warning: recoverable incompatibility or unexpected state;
- Error: operation could not complete.

Avoid per-frame logs and dumps of user paths, saves, or proprietary content.

## Compatibility

Treat game updates as compatibility events. If a patch depends on a concrete method/type shape, verify it again after updates.

A mod can be source-correct and still be runtime-incompatible.
