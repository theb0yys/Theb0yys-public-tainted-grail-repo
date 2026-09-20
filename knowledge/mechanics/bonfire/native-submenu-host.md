---
document_type: mechanic
scope: runtime-built submenu hosted under native VFireplaceUI
runtime: mono
evidence:
  static: SOURCE_AND_DECOMPILE_INSPECTED
  build: PASSED_FOR_PRIVATE_0_6_0
  runtime: PARTIAL
last_verified: 2026-09-20
---

# Native-Style Bonfire Submenu Host

Better Bonfire Menu researched a route that keeps the native fireplace view as the interaction/focus owner while adding a runtime-built services submenu.

## Host contract

`VFireplaceUI` exposes the native button content, description path, default focus and close prompt.

The plugin route:

- clones an initialized native button rather than inventing a parallel control style;
- registers through the existing button/description path;
- keeps controls in the current native focus base;
- reuses native selected/disabled visuals;
- reuses the native description path;
- repurposes the existing Close prompt as Back only while the plugin owns the submenu;
- destroys plugin-owned objects and restores vanilla content if construction fails.

## Input ownership

A close interception must be extremely narrow. The private source review explicitly corrected an overly broad `FireplaceUI.Close(bool)` interception so forced combat/service closure would not be swallowed.

## Proof boundary

Private 0.6.0 source/build/deploy passed, but the newest native grid still required in-game mouse/controller/auto-scroll/Back/service-return/ultrawide validation.

Document the architecture as source/build-backed and keep full runtime UI readiness partial.
