# Recipe: Consume a Promoted Tainted Framework Service

Tainted Framework is **not** a general FoA SDK.

Use this recipe only when the service has a public consumer contract.

## Gate

Before adding the dependency, answer all of these:

1. What is the exact capability/service ID?
2. Which public assembly/type owns it?
3. Is the consumer surface promoted or still candidate/blocked?
4. Which runtime(s) and game build(s) have evidence?
5. Does the service mutate runtime state?
6. What is the failure/rollback behaviour?
7. What must your mod do if the provider is missing/incompatible?

If those answers are not documented, **do not consume the service yet**.

## Read-only example

`framework.runtime-report` is suitable for diagnostics.

It does not grant generic game API access, runtime mutation, Harmony brokerage, or arbitrary registration services.

## Shared registrar example

The native item registrar has a shared ownership direction but must be consumed only when the exact content lane is promoted.

Do not duplicate a private registrar in every consumer just to bypass a blocked shared gate.
