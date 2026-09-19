# Case Study — UI Opens but Choice Dispatch Does Not Complete

Document type: **case study**.

## Symptom

A custom companion command/dialogue UI opens.

Some versions of the failing route even show hover state.

But selecting a choice does not produce the expected command/choice marker and does not reach the intended handler.

## Why this matters

“Screen opened” is a weak UI proof.

A complete interactive path is:

```text
open
→ acquire UI/input/cursor ownership
→ usable cursor/focus
→ input reaches EventSystem/Rewired/UI
→ selection dispatch
→ handler entry
→ command
→ close
→ restoration
```

Any one stage can fail while the screen remains visible.

## Failure history

The case passed through several partial states:

- the menu opened;
- a mod-owned virtual cursor appeared in one failed route;
- options were still unusable;
- later versions registered row hover but did not accept choices;
- command logs showed a menu open without the expected following choice/dispatch marker.

That evidence moved the failure upstream of command behaviour.

## Corrected comparison

The known working route used one coherent UI ownership path:

- shared custom-UI scope;
- real cursor capture/reassert/restore;
- gameplay-input suppression while UI input remains available;
- Rewired/EventSystem pass-through;
- Unity `Button.onClick`;
- bounded real-pointer manual fallback;
- both selection routes converging on the **same command dispatcher**.

The failing target also carried an extra submit/release/debounce condition on the normal button route.

Removing target-only ownership/dispatch gates restored full-path parity while retaining only same-frame duplicate suppression.

## What this case proves

- visible menu ≠ usable input;
- hover ≠ selection;
- selection ≠ handler entry;
- handler entry ≠ command success;
- one copied scope API is not full-path equivalence;
- a target-only debounce/release gate can break dispatch even when UI presentation looks correct.

## Reusable diagnostic rule

If the menu opens but the command does not happen:

```text
do not edit the command body first
→ find the last proven marker
→ require a fresh selection/handler-entry marker
→ compare input/cursor/focus/dispatch against a working route
```

## Diagnostic handoff

Use [UI opens but action does not fire](../../diagnose/ui/opens-but-action-does-not-fire.md).

## Canonical system

- [UI, Cursor, Focus, and Input Ownership](../../systems/ui-input/README.md)

## Evidence status

Underlying case: **live failure observations plus source/path comparison**.

This public case-study rewrite: **documentation-only**.

Exact UI APIs remain version/framework specific.
