# What Upstream Tool Sources Prove

Upstream documentation is useful, but it proves only the tool/framework behavior it actually documents.

## BepInEx

https://github.com/BepInEx/BepInEx

Useful for:

- loader/plugin lifecycle;
- dependency metadata;
- configuration APIs;
- Mono vs IL2CPP loader architecture;
- logging/plugin packaging expectations.

It does **not** prove which FoA native class owns a game behavior.

## HarmonyX

https://github.com/BepInEx/HarmonyX

Useful for:

- Harmony patch semantics;
- prefix/postfix/transpiler behavior;
- patch ownership/cleanup patterns.

It does **not** prove that a chosen FoA method is the correct gameplay owner.

## Unity documentation

https://docs.unity3d.com/

Useful for:

- Unity engine API contracts;
- scenes, GameObjects, rendering, audio and asset behavior;
- general lifecycle semantics.

It does **not** prove Questline's custom Model/World/Story/renderer ownership.

## Rule

Use upstream tool sources to establish **generic platform mechanics**.

Use FoA source/static/runtime evidence to establish **game-specific ownership and behavior**.

One lane does not substitute for the other.
