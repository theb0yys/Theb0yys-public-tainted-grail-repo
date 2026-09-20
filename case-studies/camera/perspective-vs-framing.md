---
document_type: case
scope: True Third Person / First Person Plus ownership findings
evidence:
  static: DECOMPILED_PLUS_PROJECT_HISTORY
last_verified: 2026-09-20
---

# Perspective Transition vs Camera Framing

Camera research found two very different classes of operation.

## Broad native transition

`VHeroController.ChangeHeroPerspective` changes perspective state, camera ownership, body/equipment load and FOV-related behaviour.

## Narrow presentation change

Existing `Cinemachine3rdPersonFollow` fields can be adjusted for bounded framing without replacing the camera or forcing perspective.

## Lesson

Choose the seam that matches the requested responsibility.

“Move the third-person shoulder slightly” should not invoke a full body/perspective transition.
