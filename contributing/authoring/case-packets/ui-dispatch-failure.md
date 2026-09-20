# Case Packet — UI Opens but Input / Dispatch Does Not Complete

## Reader symptom

> A custom menu opens and may even show hover state, but clicking or selecting a choice does not reach the intended command handler.

## Canonical owners

- custom UI owner;
- cursor ownership;
- gameplay-input suppression;
- Rewired/EventSystem/UI input;
- focus/selection;
- dispatch;
- command handler;
- close/restoration.

## Evidence-backed case history

Reviewed private companion-menu evidence records several partial states:

- the dialogue/menu opened;
- a mod-owned virtual cursor appeared in one failed route;
- options remained unusable;
- later builds showed hover without accepted choices;
- command logs showed menu-open without the expected subsequent choice/dispatch marker;
- source comparison found an extra submit/release/debounce gate on the target path that the known working route did not use;
- the corrected model used one shared UI scope, real cursor ownership, EventSystem/Rewired pass-through, and both Unity `Button.onClick` and bounded manual hit-test fallback converging on the same command dispatcher with only same-frame duplicate suppression.

## What this case proves

- visible/open UI is not proof of usable input;
- hover is not proof of dispatch;
- a missing handler/choice marker moves the failure upstream of command behaviour;
- copying one scope API or one button handler is not full-path equivalence.

## What this case does not prove

- every UI must use the same framework/API;
- a manual hit-test fallback is universally required;
- the public case rewrite is itself runtime-passed.

## Canonical dependencies

- [UI, Cursor, Focus, and Input Ownership](../../../systems/presentation/ui-input.md)
- [Evidence status](../../../reference/evidence/README.md)

## Public outputs

- case study: `examples/failures-and-corrections/ui-opens-but-choice-does-not-dispatch.md`
- diagnosis: `diagnose/ui/opens-but-action-does-not-fire.md`

## Evidence status

- underlying case: live failure observations plus source comparison;
- public rewrite: documentation-only;
- exact API compatibility remains version/surface-specific.
