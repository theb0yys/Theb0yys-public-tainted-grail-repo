---
document_type: troubleshooting
scope: custom/native Hero Storage browser opens with missing items
last_verified: 2026-09-20
---

# Hero Storage Browser Opens but Items Are Missing

Hero Storage may be compressed when nobody is actively using the live item representation.

Check the native lifecycle:

1. Did the UI/consumer call `HeroStorage.RequestItems()`?
2. Is the storage user/reference count active?
3. Has `CreateAllItems()` materialised compressed entries?
4. Are you reading `HeroStorage.Items` only after materialisation?
5. On close, are you allowing `ReleaseItems()` / `StashAllItems()` to restore the native compressed representation?

Do not build a parallel stash list from serialized/compressed records merely because the live list is empty before the native request lifecycle.
