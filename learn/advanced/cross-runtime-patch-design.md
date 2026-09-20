# Cross-Runtime Patch Design

Advanced patching should isolate runtime differences instead of duplicating entire features.

## Start from the native owner

Before writing a Harmony patch:

1. identify the exact native owner;
2. identify the invariant the original method maintains;
3. choose prefix/postfix/transpiler only after understanding the original lifecycle;
4. define what happens when the target changes or cannot be resolved.

## Prefer bounded shapes

Good recurring shapes:

- postfix adjusts one returned value;
- prefix guards an action and otherwise preserves native behavior;
- sidecar observes a lifecycle and adds mod-owned behavior.

Avoid replacing an entire method when a narrow seam exists.

## Cross-runtime layout

Prefer:

```text
shared feature policy
        ↓
public/shared contract
        ↓
Mono adapter    IL2CPP adapter
        ↓            ↓
native owner   native owner
```

Runtime-specific types should stay near the host/adapter boundary.

## Patch failure behavior

For optional features, fail closed:

- log exact target/version;
- disable the feature;
- leave native behavior intact.

Do not guess a nearby overload after the expected target disappears.

## Conflict discipline

When multiple mods may patch the same method:

- preserve original execution when possible;
- avoid global state;
- make prefixes conditional;
- keep side effects idempotent;
- unpatch only your own Harmony owner;
- log enough identity/version context to diagnose ordering issues.

Cross-runtime support must be validated separately for every runtime you claim.
