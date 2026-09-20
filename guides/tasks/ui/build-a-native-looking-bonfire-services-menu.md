# Build a Native-Looking Bonfire Services Submenu

**Evidence status: PARTIAL.** Native service calls and a representative Services entry worked; the newest full submenu still lacked complete input/layout/service-return validation.

Working lineage: [Native Service Reuse and Submenu Ownership](../../../research/case-studies/bonfire/native-service-reuse.md).  
Native services: [Bonfire / Fireplace Native Services](../../../knowledge/systems/world/bonfire-services.md).  
UI lifecycle: [UI, Cursor, Focus, and Input Ownership](../../../knowledge/systems/presentation/ui-input.md).

## Two separate proofs

Do not conflate:

1. **service invocation works**;
2. **custom submenu lifecycle works**.

This guide focuses on the second.

## Process

```text
bonfire UI initialized
→ clone/reuse native-looking button style
→ open mod-owned Services submenu
→ acquire UI/input scope
→ focus/select
→ invoke real FireplaceUI service action
→ service screen runs natively
→ return/reopen submenu as designed
→ close and restore input/cursor
```

## Required UI responsibilities

Your submenu owns:

- button objects;
- focus;
- disabled state;
- descriptions;
- Back/Cancel;
- cursor/input scope;
- teardown.

The native service still owns gameplay.

## Verification still required

Test:

- mouse;
- keyboard;
- controller;
- initial focus;
- disabled rows;
- service opens;
- returning from service;
- Back/Cancel;
- scene transition;
- plugin disable;
- cursor/input restoration;
- no duplicate menu objects.

## Current proof boundary

Representative service flow and native Services entry worked. Full native-style submenu lifecycle remains partial until the complete input/layout/return matrix is run.
