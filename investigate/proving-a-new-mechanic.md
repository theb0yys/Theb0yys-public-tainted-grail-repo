---
document_type: investigation
scope: promoting an unknown route into reusable knowledge
last_verified: 2026-09-20
---

# Proving a New Mechanic

Use this process when you have discovered a new possible FoA integration route.

## 1. Define the bounded capability

Bad: “custom actors”.

Better: “spawn one reviewed non-unique LocationTemplate as a session-only actor and cleanly discard it”.

## 2. Establish owner and lifecycle

Record exact identity, owner, readiness, downstream consumer and cleanup.

## 3. Separate observation from mutation

Use diagnostics/source inspection first where practical.

## 4. Select the smallest intervention

Prefer result adjustment, action guard or lifecycle sidecar over replacing the owner.

## 5. Verify terminal success

Do not stop at “method fired” or “object exists”.

## 6. Prove cleanup

Exercise the relevant close/dismiss/unload/scene/death path.

## 7. Keep adjacent lanes separate

Persistence, compatibility and packaging need their own proof.

## 8. Publish the boundary

A public mechanic must state what was **not** tested as clearly as what worked.
