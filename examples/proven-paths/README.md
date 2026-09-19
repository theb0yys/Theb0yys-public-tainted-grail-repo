# Proven Path Examples

These examples demonstrate reusable implementation shapes taken from working Tainted Grail mod paths.

| Example | Use it when |
| --- | --- |
| [Harmony result postfix](01-harmony-result-postfix-mono/README.md) | the game owns the calculation and the mod only adjusts the returned result |
| [Harmony action guard](02-harmony-action-guard-mono/README.md) | the game owns the action and the mod needs a narrow allow/deny gate |
| [Runtime UI overlay](03-runtime-ui-overlay-mono/README.md) | the mod owns presentation while game state remains native |
| [Audio replacement gate](04-audio-replacement-gate-mono/README.md) | custom audio should replace a narrowly identified native event |
| [Skybox runtime ownership](05-skybox-runtime-ownership-mono/README.md) | a visual mod must capture, replace and restore an owned runtime visual state |

Use these as implementation patterns, then bind them to the exact game owner for your feature.
