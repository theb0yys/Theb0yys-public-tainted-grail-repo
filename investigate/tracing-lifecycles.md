---
document_type: investigation
scope: lifecycle reconstruction
last_verified: 2026-09-20
---

# Tracing a Lifecycle

A usable lifecycle map should answer more than “which method fires”.

Trace:

```text
construction / restore
→ readiness
→ normal entry
→ state transitions
→ downstream effects
→ terminal success/failure
→ cleanup / release / discard
```

## Questions to record

- Who creates the object?
- Which owner is authoritative after creation?
- Which event/method means “ready”?
- Which transitions are native vs mod-owned?
- What happens on scene change, hide/show, death, dismissal or unload?
- Which resources must be released?
- Which state is save-owned?
- Which state is session-only?

Creature, weapon, UI and save research all show that one successful midpoint is not proof of the entire lifecycle.
