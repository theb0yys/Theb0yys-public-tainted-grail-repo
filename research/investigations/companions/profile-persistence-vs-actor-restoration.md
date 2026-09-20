---
document_type: investigation
scope: persist companion profile without persisting native actor ownership
last_verified: 2026-09-20
---

# Companion Profile Persistence vs Actor Restoration

These are different problems.

A future sidecar/profile can safely aim to preserve **project-owned progression metadata** while refusing to restore an actor instance.

## Candidate profile fields

- schema version;
- reviewed roster/template identity;
- display/review metadata;
- trust/loyalty scores/levels;
- last project command/follow setting;
- project bond state.

## Do not persist as profile truth

- Location ID;
- actor instance ID;
- scene/coordinates;
- health;
- combat/movement state;
- native action component state;
- current target;
- “owned actor exists” marker;
- auto-respawn/re-adoption request.

## Restore model

```text
load project profile
→ no actor is assumed alive/owned
→ user/native acquisition creates a fresh actor later
→ project profile may influence policy after that new actor is safely owned
```

This keeps long-lived project progression separate from fragile runtime object identity.
