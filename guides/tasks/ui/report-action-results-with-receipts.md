# Report Mod Actions with Real Receipts

Use this guide when a UI button triggers gameplay and you want the interface to report what **actually happened**, not merely that the user clicked.

Working lineage: [Action Receipts for Mod UI](../../../research/case-studies/ui/action-receipts.md).

## What you will build

```text
user selects action
→ capture stable action inputs
→ execute through gameplay owner
→ receive success/failure
→ create short receipt
→ UI renders Done / Blocked / Failed
```

This pattern is especially useful for cheat panels, tools, inventory actions and framework controls.

## Step 1 — capture stable inputs

At click time, capture the values required to execute the action safely.

For an item grant:

```text
template GUID
+ quantity
```

Do not rely on a stale UI object/reference if the underlying game state may have changed.

## Step 2 — re-resolve through the real owner

Before mutation, resolve the current native/mod owner state again.

For the proven grant flow, the selected item template is re-resolved by GUID before using the native item/inventory path.

## Step 3 — return an explicit operation result

Your gameplay method should return enough information to distinguish:

- success;
- blocked/not-ready;
- failure.

Do not infer success from:

- button pressed;
- window closed;
- request queued.

## Step 4 — create the receipt after execution

A useful receipt can contain:

- small sequence/id;
- state: Done / Blocked / Failed;
- action/item name;
- quantity or relevant argument;
- concise failure reason.

Keep it short enough to read during gameplay.

## Step 5 — render one authoritative receipt

Avoid multiple UI layers independently guessing the result.

The gameplay/action layer returns the outcome; the UI displays it.

## Verification checklist

1. one click dispatches the action once;
2. stable inputs are captured;
3. current owner state is re-resolved;
4. successful action returns success;
5. blocked action reports Blocked;
6. exception/failure reports Failed;
7. UI never reports Done when gameplay failed;
8. repeated clicks do not duplicate unintentionally.

## Evidence boundary

**Proven:** live item Grant action completed once through the gameplay owner and displayed a completed receipt.

**Not claimed:** a universal UI framework or automatic transactional semantics for arbitrary game actions.
