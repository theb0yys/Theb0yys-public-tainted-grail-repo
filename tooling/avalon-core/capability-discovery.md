# Capability Discovery

Use Core to answer questions such as:

- is the shared provider present?
- what contract version does it advertise?
- what capability IDs exist?
- what evidence/readiness posture is reported?
- what safety gates block execution?

## Consumer flow

```text
hard dependency / host available
→ query exact documented engine/registry
→ query exact capability/contract
→ verify version/readiness
→ choose read-only / none / named promoted handoff
```

## Fail closed

If the expected API/version/capability is absent:

- do not scan private provider internals;
- do not find a similarly named type by reflection and call it;
- do not recreate the shared capability in the consumer.

Log a useful status and disable only that integration.
