---
document_type: mechanic
scope: native template identity in item serialization/restoration
runtime: mono
evidence:
  static: CURRENT_BINARY_STATIC_CONTRACT
  runtime: RESTORE_ORDER_NOT_FULLY_PROVEN
last_verified: 2026-09-20
---

# Template Save Identity

Current-binary static evidence establishes an important persistence dependency:

```text
Item serialization
→ write template identity by GUID
→ later item restoration
→ read GUID
→ TemplatesUtil / TemplatesProvider lookup
→ reconstructed Item requires resolvable ItemTemplate
```

## Consequence for custom templates

A custom item template must be available **before the native restoration path needs to resolve its GUID**.

This explains why “the item worked in the session where I created it” is not enough evidence for save safety.

## Remaining runtime questions

- exact registration vs restoration ordering;
- behaviour when the registrar/mod is missing;
- changed definition under the same custom GUID;
- copied saves;
- cross-version migration.

See [Works now but not after load](../../../guides/troubleshooting/works-now-but-not-after-load.md).
