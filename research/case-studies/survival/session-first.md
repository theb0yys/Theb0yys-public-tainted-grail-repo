# Session First, Persistence Later

Tainted Survival built its feature in stages:

```text
observe native rest/movement/damage/status/environment
→ session-only fatigue dry run
→ passive overlay
→ optional non-saved stat effects
→ food/rest/environment integration
```

It did not begin by inventing a saved “survival stat”.

## Lesson

A session-local model can validate whether the mechanic is worth keeping, which native events are stable, and how it feels before taking on save migration and compatibility risk.
