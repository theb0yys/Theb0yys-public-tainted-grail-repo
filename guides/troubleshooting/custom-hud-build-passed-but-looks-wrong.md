---
document_type: troubleshooting
scope: custom HUD resource/build success but bad in-game layout
last_verified: 2026-09-20
---

# Custom HUD Builds but Looks Wrong In Game

A successful build, embedded-resource decode and deploy prove packaging—not layout.

Check separately:

- actual live screen resolution/aspect ratio;
- native HUD rectangles versus guessed hierarchy paths;
- theme resource dimensions/aspect;
- anchor/pivot and bottom/top alignment;
- controller/mouse prompt placement;
- vanilla backdrop visibility;
- hide-context interaction;
- scaling/localisation;
- whether the target native rectangle was misidentified.

The HUD project had a concrete example where an assumed Wyrd-skill-bar rectangle actually corresponded to a smaller icon cluster, causing the custom stats panel to collapse into the wrong region.

Use screenshot/runtime rectangle evidence to correct the owner/geometry assumption.
