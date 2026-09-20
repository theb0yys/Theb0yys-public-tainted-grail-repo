---
document_type: troubleshooting
scope: TGE manifest parses but extension does not activate
last_verified: 2026-09-20
---

# TGE Manifest Is Valid but the Extension Does Not Activate

Parsing and activation are separate phases.

A manifest can parse successfully and still be skipped/fail because:

- `enabled=false`;
- another enabled extension already reserved the same `extensionId`;
- the referenced local assembly file is missing;
- host/provider activation rejects the extension later.

## Check

1. exact top-level plugin directory;
2. exact filename `tge-extension.manifest`;
3. `manifestVersion=1`;
4. unique required keys;
5. local DLL filename only;
6. enabled state;
7. duplicate extension ID;
8. activation diagnostics.

Do not add path traversal or recursive discovery to “fix” a layout error.
