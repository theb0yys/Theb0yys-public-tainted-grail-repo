---
document_type: case
scope: Better Bonfire Menu native service integration
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  runtime: MIXED
last_verified: 2026-09-20
---

# Native Service Reuse and Submenu Ownership

Better Bonfire Menu demonstrates two separate lessons.

## Service actions

The game already exposes real owners for stash, crafting, rest, level-up, gems/gear services, save, travel and pet recall. The mod can route to those instead of reproducing their gameplay.

## Custom host

A native-looking submenu is a separate problem involving button cloning, focus, disabled state, descriptions, Cancel/Back ownership and teardown.

The private project had representative working service flow and a working native Services entry, while the newest 0.6.0 full native submenu still had unrun input/layout/service-return validation.

## Lesson

Do not let “we know how to invoke the service” silently become “our replacement UI lifecycle is fully proven”.
