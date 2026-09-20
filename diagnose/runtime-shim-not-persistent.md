---
document_type: troubleshooting
scope: feature appears active in-session but is absent after reload
last_verified: 2026-09-20
---

# Runtime Shim Is Not Persistence

Some routes deliberately make state appear available only for the current runtime.

Examples include:

- treating a proof recipe as known without writing `HeroRecipes`;
- session-only companions marked not saved;
- runtime visual/prototype bridges;
- temporary overlays.

If the feature disappears after load, first ask whether the route was **designed** to persist.

Do not debug the save system for a route whose contract explicitly said “runtime-only”.

For durable-state work, separate:

```text
runtime mutation
≠ native model persistence
≠ mod-owned persistence
≠ compatibility across missing/updated mods
```
