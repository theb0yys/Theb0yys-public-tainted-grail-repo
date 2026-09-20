---
document_type: troubleshooting
scope: runtime success disappears across save/load or restart
last_verified: 2026-09-20
---

# Works Now but Not After Load

Runtime state and durable state are different contracts.

Ask:

1. Was the object/session state explicitly marked non-persistent?
2. Does the native owner serialize this state?
3. Does a custom template/identity exist before native restoration tries to resolve it?
4. Does the mod restore state too early?
5. Is the “working” route only a runtime shim?
6. Is the required mod/package absent during restore?
7. Was save/load ever actually tested?

Do not convert a runtime success into a persistence claim by wording.

For generic mod-owned state, see [Save-state boundary](../../knowledge/mechanics/save-state/README.md).
