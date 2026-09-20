# Capture the Smallest Useful Diagnostic Evidence

Use this when you need evidence for a failure without dumping excessive game/user data.

Canonical discipline: [Debugging and Diagnostics](README.md).

## Capture around the first failed stage

Start from:

```text
expected invariant
→ observed result
→ earliest stage where they diverge
```

Then capture only what proves that divergence.

## Minimal evidence bundle

A useful support bundle often needs only:

- game build/version;
- Mono or IL2CPP;
- BepInEx version;
- mod/plugin GUID + version;
- dependency versions;
- exact trigger steps;
- short log window around the failure;
- relevant native/custom identity;
- expected vs observed result.

Add screenshots/dumps only when they prove a specific visual/runtime fact.

## Redact and exclude

Do not share:

- full save files unless explicitly required and safe;
- whole game folders;
- game/Unity DLLs;
- extracted commercial assets;
- credentials/tokens;
- personal paths/usernames when unnecessary;
- huge logs unrelated to the trigger.

Prefer a narrow excerpt with enough preceding context to show the first failure.

## Diagnostic probe rules

A temporary probe should be:

- read-only where possible;
- bounded in frequency/rows;
- explicitly enabled;
- easy to remove/disable;
- attached to the actual owner being tested;
- clear about evidence state.

Do not let a diagnostic probe silently become production mutation logic.

## Useful before/after pattern

For a bounded state transition:

```text
before owner state
→ trigger
→ after owner state
→ downstream consumer state
```

This is often more useful than a large object dump.

## Formal proof

When diagnostics become evidence for a repository claim, record the proof lane using the [Public Evidence Standard](../../sources/evidence-standard.md).
