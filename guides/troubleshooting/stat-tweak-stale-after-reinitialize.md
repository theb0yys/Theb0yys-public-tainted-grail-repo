---
document_type: troubleshooting
scope: runtime stat tweak exists but game/UI shows old/vanilla-like value
last_verified: 2026-09-20
---

# Runtime Stat Tweak Went Stale After Reinitialization

If your tweak object still exists but the game/UI reads a different value, check whether the native stat wrapper rebuilt the underlying `Stat` object.

## Symptom

- config changes;
- tweak update runs;
- logs show expected modifier;
- character sheet/gameplay still reads old/vanilla-ish value.

## Fix pattern

1. Re-resolve the **current** hero/stat owner.
2. Discard the old mod-specific tweak.
3. Recreate the tweak against the current native stat object.
4. Reapply only from safe lifecycle/config events.

Do not assume a long-lived tweak reference survives native stat reconstruction.
