# Recipe: Read-Only Core Discovery

Use this when your mod wants to integrate with shared ecosystem metadata without taking execution authority.

```text
hard dependency on Avalon Core
→ verify expected Core version
→ query exact registry/engine
→ inspect capability/contract/readiness metadata
→ if compatible: enable read-only/discovery feature
→ otherwise: action=none
```

Do not fall back to reflection into provider internals.

If the feature needs execution, identify the **named promoted executor/service owner** instead of assuming Core itself executes it.
