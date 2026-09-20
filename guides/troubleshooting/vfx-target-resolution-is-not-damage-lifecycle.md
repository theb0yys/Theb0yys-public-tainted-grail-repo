---
document_type: troubleshooting
scope: post-damage VFX hooked at target-resolution stage
last_verified: 2026-09-20
---

# Target Resolution Is Not the Damage Lifecycle

A real Tainted Blood regression came from treating:

`Damage.DetermineTargetHit(Collider, ...)`

as a stable VFX lifecycle hook.

It is target resolution **before damage**, not post-damage presentation ownership.

## Failure lesson

If a visual feature causes unrelated hit/damage behaviour or “stone hit” style regressions:

1. remove mutation/side effects from target-resolution hooks;
2. establish whether the target actually received native damage;
3. move living-hit presentation to a post-damage owner;
4. move terminal presentation to a death owner;
5. keep repeated corpse-hit behaviour unsupported until a safe post-damage/death-owned path exists.

Do not retain a wrong lifecycle seam merely because it exposes a convenient collider.
