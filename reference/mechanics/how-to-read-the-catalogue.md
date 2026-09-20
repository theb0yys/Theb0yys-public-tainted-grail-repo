# How to Read the Mechanics Catalogue

The catalogue is a **claim index**, not a maturity score for an entire feature area.

Canonical catalogue: [Mechanics Catalogue](README.md).

## Read each row as one bounded claim

A row can say:

```text
custom weapon registration + Drake prototype
→ static + bounded runtime evidence
→ persistence/full lifecycle separate
```

That means exactly that combination was supported to the stated ceiling.

It does not mean:

- every weapon type works;
- every runtime is proven;
- save/load is proven;
- uninstall is safe;
- the mechanism is a public stable API.

## Public-state meanings

Typical catalogue states include:

- **Static/source inspected** — source/decompilation/metadata supports the mechanism.
- **Consumer-used** — at least one real consumer uses the path; inspect the exact consumer/runtime scope.
- **Bounded runtime evidence** — a specific runtime scenario passed.
- **Blocked by static verdict** — current inspected architecture rules out the proposed route.
- **Under evaluation** — candidate path exists but proof is incomplete.

Always read the boundary column.

## Move from catalogue to implementation

Use this route:

1. identify the mechanic row;
2. open the canonical mechanic/system page;
3. confirm runtime/build/version scope;
4. inspect the exact owner/lifecycle;
5. use a template/example only if it matches that scope;
6. run claim-fit validation for your consumer.

Do not implement directly from one table row.

## When to update a row

Update the catalogue only when the claim state itself changes—for example:

- new static evidence raises/lowers the mechanism ceiling;
- a runtime gate passes;
- a persistence gate passes;
- a previously usable route is invalidated by a game update.

A new tutorial or code sample does not automatically change mechanic maturity.
