# Intervention Seam Selection Matrix

Use this after identifying the native owner but before choosing a patch/API.

Canonical rule: [Intervention Selection](README.md).

| Desired change | Preferred seam | Preserve |
| --- | --- | --- |
| change a calculated value | bounded postfix/result adjustment | original state transitions and side effects |
| conditionally prevent an action | prefix/action guard | native path when condition allows |
| add telemetry/presentation | sidecar observation | gameplay owner |
| replace a resource | owner-aware replacement + restore | native logical state and lifecycle |
| add content definition | proven registrar/registry path | normal downstream provider/consumer path |
| integrate another mod | public contract/service | provider implementation ownership |
| span Mono + IL2CPP | shared contract + thin runtime adapters | common feature policy |

## Escalation test

Before choosing a broader seam, ask:

1. Did the narrower seam fail because it cannot express the feature?
2. Or did it fail because the owner/readiness/identity is still unknown?

Only the first justifies escalation.

## Bad escalation examples

- polling all objects because the correct event was not investigated;
- replacing a whole method because one output field was inconvenient;
- taking transform ownership because the native movement system was not traced;
- writing save data because current-session registration works;
- patching another mod's private field instead of using its public API.

## Proof rule

The broader the intervention, the more lifecycle and negative-case proof it requires.

A prefix that blocks one native action has a smaller proof surface than replacing a whole native subsystem.
