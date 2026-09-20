# Verify the Current Build Before Reapplying a Bug Fix

Use this when you are about to port or recreate an old community fix for a quest, progression blocker or gameplay bug.

The case behind this rule is [Patch Notes Can Invalidate an Old Fix](../../../research/case-studies/bugfixes/current-build-before-fix.md).

## Symptom

You found an old fix, patch or workaround and the affected game behaviour sounds familiar.

The temptation is:

```text
old bug existed
→ old community fix exists
→ reapply fix to current build
```

That is unsafe.

## Why

The base game may already have fixed the underlying defect.

An old workaround can then:

- duplicate a transition;
- force state that is now handled natively;
- reintroduce a stale assumption;
- conflict with the current quest/story implementation;
- create a new blocker.

## Correct process

1. record the exact current game build;
2. read relevant official patch history;
3. reproduce the issue on the current build;
4. capture the exact current broken state;
5. inspect the current owner/lifecycle;
6. only then decide whether a repair is still required.

## Fail closed

If you cannot reproduce the bug on the current build, do not ship the historical workaround as an active fix.

Keep it inactive/research-only until current evidence establishes the defect.

## Verification

A current-build bugfix claim should identify:

- current build/version;
- reproducible trigger;
- exact broken state;
- expected native state;
- current owner of the missing/incorrect transition;
- fix behaviour;
- regression/rollback test.

## Evidence boundary

The Oh Merry Men / Over My Dead Bodies research deliberately stayed inactive after official patch history indicated related fixes. That is the lesson: **current-build reproduction is part of the bugfix contract.**
