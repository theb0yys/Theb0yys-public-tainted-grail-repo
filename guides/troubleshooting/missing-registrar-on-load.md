---
document_type: troubleshooting
scope: custom item save fails to restore because template is unavailable
last_verified: 2026-09-20
---

# Custom Item Missing on Load

If a custom item existed when saved but fails during restoration, check the **template identity dependency** before debugging inventory code.

Native item serialization is template-GUID based in the inspected contract. Restoration needs that GUID to resolve again.

## Check

1. Is the custom registrar/plugin loaded?
2. Has template registration completed before item restoration needs the GUID?
3. Does the exact custom GUID resolve through `TemplatesProvider`?
4. Has the definition changed under the same identity?
5. Is this a copied save or missing-mod case that was never validated?

Do not patch the item deserializer merely because the template is missing. Fix the identity/readiness contract first.
