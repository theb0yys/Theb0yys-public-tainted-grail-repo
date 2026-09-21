# Item Pipeline Failure Catalogue

## Template object exists but cannot be resolved

Earliest likely failure: registration. Re-check readiness, collision checks, insertion, and provider re-resolution.

## Item exists but does not appear in shop UI

Earliest likely failure: acquisition timing or UI snapshot timing. Verify stock mutation happened before ShopUI captured its item list.

## Native item changed instead of creating a new item

Verify identity/source isolation: the custom template should have a separate GUID/name and the native source object should remain unchanged.

## One item works but a batch fails

Do not generalise the source profile across unrelated ItemTemplate families. Return to one known-good descriptor and establish each family separately.

## Icon/model is wrong

Do not reopen registration first. Confirm the registered identity and runtime Item still pass, then diagnose the presentation lane.

## Same-session success but load fails after restart

Persistence was never proven. Check registration ordering during cold load, saved identity resolution, and missing-package behaviour.

## Duplicate or inconsistent runtime objects

Check idempotency, collision handling, acquisition insertion guards, and whether the same custom identity is being registered/created more than once.
