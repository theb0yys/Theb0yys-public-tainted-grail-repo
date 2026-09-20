---
document_type: troubleshooting
scope: camera/body modification unexpectedly changes equipment, FOV or animation
last_verified: 2026-09-20
---

# Camera Change Has Unrelated Side Effects

If a perspective change unexpectedly reloads equipment/body state, changes FOV or affects animation, check whether you called the **native perspective transition** rather than a bounded camera-rig adjustment.

`VHeroController.ChangeHeroPerspective(bool)` is intentionally broad.

For a framing-only feature, prefer the existing third-person Cinemachine rig and the smallest proven field adjustment.

For body-aware FPP work, stay read-only until body/camera/weapon ownership is proven together.
