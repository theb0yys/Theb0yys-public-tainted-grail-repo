# Diagnose — UI Opens but the Action Does Not Fire

Document type: **troubleshooting**.

## Symptom

The menu or dialogue surface opens, but clicking/selecting an option does not execute the intended action.

The screen may even show hover or focus.

## First rule

Do not edit the command body until you have proved that the choice reaches the handler.

Use the full path:

```text
open
→ UI/input scope
→ cursor/focus
→ input reaches UI
→ selection
→ dispatch
→ handler entry
→ command
→ close/restoration
```

## 1. Establish the last proven marker

Examples:

- screen-open marker exists;
- cursor becomes visible;
- row hover/focus changes;
- choice-click marker exists or does not;
- handler-entry marker exists or does not;
- command marker exists or does not.

The missing **next marker** identifies the first unproven transition.

## 2. If the menu opens but nothing hovers/focuses

Inspect:

- cursor lock/visibility;
- EventSystem/input module;
- Rewired/controller UI reads;
- gameplay-input suppression accidentally suppressing UI input;
- focus/selection initialization;
- competing UI scope owners.

## 3. If hover works but selection does not

Inspect:

- `Button.onClick`;
- pointer/submit routing;
- target-only debounce/release gates;
- manual fallback hit testing if that route is intentionally supported;
- whether mouse/controller routes converge on the same dispatcher.

Hover is presentation/input-position proof, not dispatch proof.

## 4. If selection marker exists but handler entry does not

Inspect the dispatch boundary:

- listener subscription;
- row-to-command binding;
- stale/disabled row state;
- duplicate-suppression/debounce logic;
- owner mismatch.

Do **not** change the command implementation yet.

## 5. If handler entry exists but command fails

Only now inspect command-specific logic.

At this point input/dispatch ownership is substantially proven.

## 6. Verify close/restoration too

A working click path is still incomplete if close leaves:

- cursor locked/unlocked incorrectly;
- gameplay input disabled;
- UI scope active;
- subscriptions alive;
- duplicate owned screens.

## Evidence to collect

```text
open marker
scope owner
cursor state
focus/hover
choice/select marker
handler-entry marker
command marker
close marker
restored cursor/input state
```

## Stop conditions

No choice/handler marker → stay upstream of command logic.

Choice marker but no handler → debug binding/dispatch.

Handler marker but wrong action → debug command.

## Related case

[UI opens but choice dispatch does not complete](../../examples/failures-and-corrections/ui-opens-but-choice-does-not-dispatch.md)

## Canonical system

[UI, Cursor, Focus, and Input Ownership](../../systems/ui-input/README.md)
