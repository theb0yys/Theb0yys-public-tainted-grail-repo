# Single AI Host Ownership

Avalon AI Runtime owns the shared scheduling/orchestration boundary.

The FoA host owns the reviewed mapping from provider-neutral package actions into game-system/native execution.

## Why one host

Multiple independent AI schedulers competing for the same actor create:

- duplicated observation;
- conflicting action authority;
- race conditions;
- incompatible cooldowns;
- unclear cleanup;
- impossible cross-package arbitration.

The single host provides one place for:

- package registration/lifecycle;
- actor ownership/leases;
- observation collection;
- policy evaluation;
- action proposal dispatch;
- fault containment;
- release/cleanup.

## Feature mod relationship

A feature mod owns its domain truth.

The AI package consumes that truth through its reviewed package/provider boundary; Runtime owns live decisions/execution scheduling.

Example:

```text
companion mod owns companion identity/state
→ AI package observes eligible companion state
→ Runtime evaluates package
→ host executes reviewed native follow/defend action
```
