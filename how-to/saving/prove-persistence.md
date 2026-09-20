# Prove Persistence Instead of Assuming It

Use this when a feature is expected to survive save/load, restart, mod update or disable/reenable.

Canonical ownership: [Saving and Persistence](../../systems/world/saving-persistence.md).

## Runtime success is not persistence

Treat these as separate gates:

```text
feature works now
→ native/mod save completes
→ process exits
→ definitions/providers are available on next start
→ save restores
→ feature state/identity is correct
```

Do not collapse them into one “works” claim.

## Procedure

1. Define exactly what must persist.
2. Identify the real persistence owner for each piece of state.
3. Give durable custom identities stable values.
4. Ensure required definitions/providers are available before restore needs them.
5. Use a disposable test save.
6. create/change the state;
7. complete a save through the intended owner;
8. exit the game/process;
9. restart;
10. load the same slot;
11. verify identity and behavior;
12. repeat to test duplicate/replay behavior.

## Negative cases

For a production persistence claim, also define/test as applicable:

- mod disabled/missing;
- dependency missing;
- older/newer mod version;
- schema/identity migration;
- duplicate registration;
- uninstall/orphan state;
- failed/partial save;
- scene transition before re-application.

If the intended policy is “unsupported when the mod is absent”, document that explicitly rather than leaving the behavior unknown.

## Session-only alternative

If the feature does **not** need persistence, prefer an explicit session-only design and verify that no mod-owned object/state survives into a later load accidentally.

## Sidecar caution

The current research direction for truly mod-owned durable state is a namespace-isolated sidecar coordinated with native save/load milestones.

That architecture is still under evaluation and is not a generally proven public recipe.

## Evidence record

Record:

- game build/runtime;
- mod/dependency versions;
- save slot used;
- identity/schema version;
- save-complete evidence;
- restart evidence;
- load result;
- duplicate/replay result;
- negative cases run/not run.

For general validation semantics see [Validation and Compatibility](../compatibility/validation.md).
