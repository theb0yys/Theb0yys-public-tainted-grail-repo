---
document_type: troubleshooting
scope: custom inventory source looks correct but runtime readiness is unproven
last_verified: 2026-09-20
---

# Inventory UI Source Looks Correct but Runtime Proof Is Missing

Do not collapse source review into UI readiness.

For a custom inventory host, separately prove:

- open from normal gameplay;
- projection count/sample;
- repeated open/close without duplicate host;
- mouse, keyboard and controller focus;
- device switching;
- search focus/Back;
- native inventory change while open;
- deterministic selection if an item disappears;
- final Escape/Cancel close;
- cursor/controller-cursor restoration;
- gameplay-input restoration;
- world-time restoration;
- dialogue/menu/scene conflict teardown;
- 16:9/ultrawide/scaling/localisation;
- large-inventory timing.

Until that matrix passes, mutation adapters such as equip/use/quick-slot/transfer should remain blocked behind the unproven host lifecycle.
