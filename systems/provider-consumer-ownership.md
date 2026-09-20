---
document_type: system
scope: project/provider and consumer ownership
last_verified: 2026-09-20
---

# Provider and Consumer Ownership

Reusable mod ecosystems need one owner per shared responsibility.

```text
provider owns contract + lifecycle
→ consumer registers/requests through narrow API
→ provider returns bounded result/receipt
→ consumer owns its feature state
→ unregister/cleanup does not seize unrelated ownership
```

A provider owning a contract does **not** mean the capability is production-ready.

For example, a framework can own the **native item registrar contract lane** while runtime mutation, public API promotion, consumer migration, save behaviour and compatibility remain blocked behind later evidence.

Avoid:

- multiple consumer-local registrars for the same native map;
- hidden reflection paths duplicated across mods;
- providers taking ownership of active consumer gameplay state;
- treating contract ownership as runtime proof.

See [Native item registrar ownership](../tooling/tainted-framework/native-item-registrar.md).
