# Auto-Unlock Without Bypassing Native Lock Rules

**Evidence status: PARTIAL.** The safe ownership boundary is established, but this case does not record a complete runtime validation matrix.

Working lineage: [Auto-Unlock Without Bypassing Every Lock Rule](../../../research/case-studies/lockpicking/auto-unlock-boundary.md).

## Goal

Skip the lockpicking **interaction/minigame** only when the hero is already legitimately allowed to open the lock.

Keep native:

- key-only rules;
- key possession checks;
- lock eligibility;
- crime/legal consequences;
- native unlock state.

## Process

Classify the target lock first:

```text
key-only lock
→ leave native

hero can open with key
→ leave native

ordinary lockpickable lock
+ active lockpicking interaction
→ optional auto path
→ call/use native unlock/crime route
```

The mod should skip the interaction, not invent the unlock result.

## Fail closed

If you cannot establish that the current target is the supported lockpicking case, do nothing.

Do not turn "locked object" into a universal unlock permission.

## Verification

Test:

- key-only lock remains protected;
- owned key route still works;
- ordinary lockpickable target auto-completes only when eligible;
- crime/witness behaviour remains native;
- quest/progression locks are not bypassed;
- disabling the feature restores normal minigame flow.

## Current proof boundary

The classification and owner-preserving rule are established. Full runtime validation for every lock class and crime/progression interaction remains to be demonstrated.
