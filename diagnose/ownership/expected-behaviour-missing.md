# Diagnose — Expected Behaviour Is Missing from the Object You Inspected

Document type: **troubleshooting / investigation handoff**.

## Symptom

You inspect the object that visibly represents a feature, but the behaviour/state/method you expect is not there.

Examples:

- the visible actor does not expose the movement strategy;
- the UI object does not own the authoritative state;
- the renderer does not own gameplay identity;
- the controller you found is not the live writer.

## First rule

Treat **wrong owner** as a real hypothesis.

Do not immediately:

- reflect every field;
- patch a nearby method;
- add polling;
- create a parallel state variable.

## 1. Restate the exact behaviour

Bad:

> Where is mount movement?

Better:

> Which object owns the hero's mounted movement strategy, and which object supplies native mount running velocity?

One feature can span several owners.

## 2. Separate visible object from authoritative owner

Ask:

- who creates this object?
- who stores the state?
- who writes the value each frame?
- who consumes the value?
- who transitions the lifecycle?
- who tears it down?

A visible object may only be presentation.

## 3. Follow callers and ownership links

Search:

```text
candidate type
→ constructor/init
→ owner/service/model
→ callers
→ downstream consumers
```

If the expected behaviour is absent, that is evidence against the original ownership model.

## 4. Observe the corrected owner before mutation

Once a better owner is found, design a read-only or minimally invasive probe.

Confirm:

- the owner is live;
- the relevant state changes at the expected transition;
- the consumer actually reads it.

Only after this should you choose a patch/API.

## 5. Remember that one feature can have multiple owners

The mount investigation demonstrates:

```text
hero mounted movement strategy
≠ mount velocity getter
≠ NPC/root-motion writer
```

Finding one correct owner does not authorize claims about every adjacent subsystem.

## 6. Do not confuse mod-local state with native state

A log such as:

```text
movementSuspended=true
```

proves only that the mod's Boolean is true unless you also observe the native controller/root-motion/navigation owner.

## Evidence to collect

- exact subject/type;
- suspected owner;
- initialization/caller relationship;
- live instance identity;
- state before/after the relevant transition;
- downstream read/consumer;
- rejected owner and why it was rejected.

## Related investigation

[The expected mount behaviour was on the wrong owner](../../examples/investigations/mount-wrong-owner-discovery.md)

## Canonical method

- [Research Method](../../investigate/research-method.md)
- [Native Object Ownership](../../systems/core/native-object-ownership.md)
- [Intervention Selection](../../mechanics/intervention-selection.md)
