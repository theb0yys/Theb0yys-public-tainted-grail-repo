# Research Method: From Question to Proven Process

> **Reference page.** Use this when investigating a new FoA system or deciding whether a working experiment is strong enough to teach as a reusable process.

## What this system is

The repository's goal is not to collect instructions without context.

A reusable process is built from:

~~~text
source / known facts
→ hypothesis
→ smallest diagnostic
→ controlled attempt
→ success or failure
→ corrected model
→ repeatable process
→ explicit proof boundary
~~~

Successful attempts explain **how**. Failed attempts explain **why the process has its current shape**.

## Who owns it in FoA

Native facts are owned by the system being researched: game code, runtime behavior, templates, services, saves, assets or UI.

Research notes and documentation do not create native truth. They record evidence about it.

## Important identities, types, and methods

For every investigation, capture:

- game build/runtime lane;
- exact assembly/type/method or asset/template identity;
- evidence source;
- hypothesis;
- mutation boundary;
- observable marker;
- result;
- failure reason when known;
- new rule learned;
- remaining unknowns.

## Where it exists in the lifecycle

Use research before broad implementation.

A useful progression is:

~~~text
read/inspect
→ map ownership
→ observe read-only
→ make one reversible change
→ verify downstream owner
→ test failure/duplicate cases
→ test persistence only when needed
→ generalise carefully
~~~

## How we interact with it

When researching a new system:

1. identify the native owner;
2. identify the strongest exact identity;
3. map lifecycle before choosing a hook;
4. test read-only observation first where practical;
5. change one variable;
6. record success and failure;
7. do not widen the process until the current mechanism is understood;
8. separate runtime, asset, UI and persistence claims.

## Why this route

The custom-item research is a good example:

- native item grants were already known;
- vendor stock was observed read-only;
- one custom clone was registered;
- one item was inserted;
- runtime UI proved the separate identity;
- batch expansion exposed unsupported assumptions;
- the process narrowed again;
- UI lifecycle research corrected the insertion point;
- later architecture added collision/idempotency/save requirements without pretending they were already proven.

That history is the process rationale.

## What goes wrong

Research quality degrades when:

- a plausible API is treated as proof;
- a source inspection is reported as runtime success;
- a visual result is upgraded into persistence proof;
- multiple changes are tested at once;
- failures are deleted instead of explained;
- a failed diagnosis is silently replaced rather than superseded;
- a framework abstraction is invented before two or more real use cases require it.

## How to verify

Before publishing a process, another reader should be able to answer:

- what native owner is involved?
- what identity is used?
- what lifecycle point is required?
- what exact action occurs?
- why that action?
- what failed alternatives established the rule?
- what proves success?
- what remains unproven?

## Current proof boundary

This method is the documentation standard for new reference/process pages in this repository.

Every technical page should use the same reasoning pattern:

~~~text
What this system is
Who owns it in FoA
Important identities, types, and methods
Where it exists in the lifecycle
How we interact with it
Why this route
What goes wrong
How to verify
Current proof boundary
~~~
