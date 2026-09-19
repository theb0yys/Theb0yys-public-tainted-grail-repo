# Action Receipts for Mod UI

Player-facing action panels should report the **result of the action**, not merely the button click.

## Working implementation lineage

Avalon Cheat Panel's item browser uses a row-level `Grant` action. Live screenshot evidence showed the item was granted once and the UI displayed a completed receipt.

## Working flow

~~~text
user selects row/action
→ capture stable action inputs (for example template GUID + quantity)
→ execute through the gameplay owner
→ gameplay owner returns success/failure
→ UI renders a short receipt
~~~

For item grants, the action re-resolves the selected template at execution time before using the native item/inventory path.

## Good receipt contents

A useful receipt can include:

- short sequence/id;
- Done / Blocked / Failed state;
- the item/action name;
- quantity when relevant;
- concise failure reason.

## Do not use click state as success

The UI should not say an operation succeeded because:

- the button was pressed;
- a window closed;
- a request was queued.

Tie the receipt to the actual result returned by the gameplay operation.
