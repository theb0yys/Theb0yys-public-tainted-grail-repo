# Research Method: From Unknown Behaviour to Bounded Modding Knowledge

Document type: **investigation**.

Use this process when the public documentation does not yet establish the identity, owner, lifecycle, or safe intervention you need.

## The core reasoning loop

```text
question
→ exact subject / identity
→ candidate native owner
→ lifecycle / data-flow hypotheses
→ read-only source or runtime observation where possible
→ smallest discriminating test
→ success or failure
→ corrected model
→ bounded mechanic
→ explicit proof boundary
```

The purpose is not to find a method name quickly. The purpose is to establish **who owns the behaviour, when it is valid, what consumes the result, and what evidence actually proves the route**.

## 1. State the exact question

Bad:

> How do mounts work?

Better:

> Which native owner supplies player movement while mounted, and which state transition makes that owner active?

A research question must be narrow enough that evidence can falsify it.

## 2. Establish the strongest identity

Capture the strongest available identifier for the subject:

- fully qualified type or method;
- template GUID;
- Addressables address;
- scene/object path;
- plugin/framework identity;
- game build/runtime lane.

Do not use a display name as machine identity when something stronger exists.

## 3. Find the owner, not just the visible object

A visible Unity object, screen, prefab, or field may only be presentation.

Ask:
- who creates it?
- who stores authoritative state?
- who receives input?
- who performs the mutation?
- who tears it down?
- who restores it after load or transition?

When expected behaviour is absent from the object you are inspecting, treat **wrong owner** as a primary hypothesis.

## 4. Map lifecycle before choosing a hook

Identify:

```text
creation / discovery
→ readiness
→ activation
→ mutation / dispatch
→ downstream consumer
→ close / disable
→ cleanup / release
```

The right method at the wrong lifecycle point is still the wrong integration.

## 5. Observe before mutating

Prefer, where practical:

- source inspection;
- bounded logging;
- read-only enumeration;
- state snapshots;
- before/after values;
- one event marker;
- one lifecycle transition.

Do not introduce a broad patch merely to learn whether a path exists.

## 6. Change one variable

A useful experiment changes the minimum required variable.

Avoid changing identity, hook, assets, values, acquisition, presentation, and persistence in the same test.

A failure is useful only when it changes the model.

## 7. Follow the complete path

For a candidate mechanic, map the relevant path:

```text
entry
→ native owner
→ lifecycle precondition
→ action / dispatch
→ downstream consumer
→ terminal success
→ cleanup / restoration
→ proof
```

A shared API call, a method name, a visible panel, or one log line is partial evidence.

## 8. Preserve failures and corrections

Record:

```text
initial assumption
→ evidence that did not fit
→ corrected owner / lifecycle
→ new intervention
→ new proof boundary
```

Do not silently replace an incorrect model. The correction is part of the reusable knowledge.

## 9. Keep evidence lanes separate

Source/static inspection can establish structure.

Build can establish compilation.

Loader proof can establish plugin discovery/registration.

Runtime evidence can establish observed live behaviour.

Persistence requires save/load or equivalent durable-state proof.

Compatibility requires an exact environment matrix.

Release/package claims require the actual distributed artifact.

One lane does not inherit another.

## 10. Promote only the bounded result

At the end of an investigation, publish one of:

- a canonical system explanation;
- a bounded mechanic;
- a case study;
- a troubleshooting rule;
- an exact reference entry;
- or an explicit unknown/blocker.

Do not generalize one proven content/process path across unrelated domains.

## Completion test

Before calling an investigation reusable, another technically capable reader should be able to answer:

- What exact subject was investigated?
- Who owns the relevant state or transition?
- What lifecycle must be true?
- Which intervention or observation was chosen?
- Why that seam rather than the rejected alternatives?
- What downstream behaviour proves success?
- What cleanup is required?
- What evidence actually ran?
- What remains unproven?

Related:
- [Documentation architecture](../contributing/authoring/DOCUMENTATION_ARCHITECTURE.md)
- [Publication workflow](../contributing/authoring/PUBLICATION_WORKFLOW.md)
- [Intervention selection](../mechanics/intervention-selection.md)
