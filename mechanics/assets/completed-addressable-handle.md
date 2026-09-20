---
document_type: mechanic
scope: completed Addressables handle bridge for a runtime GameObject
runtime: mono
evidence:
  static: SOURCE_INSPECTED
last_verified: 2026-09-20
---

# Completed Addressables Handle Bridge

A specific weapon implementation demonstrates a bridge from a runtime-built `GameObject` into an Addressables-style operation:

```text
runtime prototype GameObject
→ Addressables.ResourceManager.CreateCompletedOperation<GameObject>(...)
→ AsyncOperationHandle<GameObject>
→ ARAsyncOperationHandle<GameObject>
→ native-facing ARAssetReference/handle path
```

## Use narrowly

This is evidence that a completed-handle bridge can serve a specific custom equipped prototype route.

It is **not** evidence that arbitrary Addressables substitution, arbitrary asset types, or global Addressables interception is safe.

In fact, the weapon work separately rejected global generic mesh/material Addressables hooks after they interfered with unrelated UI asset loads.
