# Recipe: Read-Only Avalon Core Discovery

Use this when your mod wants shared ecosystem metadata without taking execution authority.

A complete BepInEx example is available at [examples/mono/infrastructure/core-readonly](../../examples/mono/infrastructure/core-readonly/README.md).

## Flow

```text
hard dependency on Avalon Core
→ read TrustReports
→ require read-only posture
→ look up exact documented engine
→ inspect exact capability/contract/readiness
→ use discovery result or action=none
```

If execution is required, identify the named executor/service owner.

Do not reflect into provider internals because discovery said “missing” or “blocked”.
