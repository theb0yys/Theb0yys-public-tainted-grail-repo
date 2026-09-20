# Private APIs, Reflection, and Compatibility

> **Reference page.** Use this whenever a working path depends on private fields/methods, decompiled internals, reflection, or build-sensitive member shapes.

## What this system is

Many useful FoA modding surfaces are not stable public APIs.

Examples from working paths include:

- private `TemplatesLoader.AddToMap`;
- `TemplatesProvider._loader`;
- reflected `RestockableStock` fields/backing fields;
- exact private/overloaded methods used by Harmony.

A private surface can be technically correct and still be high-risk across game updates.

## Who owns it in FoA

The native game assembly owns the member.

Reflection/Harmony only discovers or intercepts it.

Therefore a mod must treat the exact assembly/build as part of the contract.

## Important identities, types, and methods

For each private target record:

- assembly;
- assembly hash/MVID when available;
- fully qualified type;
- exact member;
- signature;
- private field/property/method kind;
- evidence source;
- expected failure when missing.

## Where it exists in the lifecycle

Reflection discovery often occurs at plug-in startup, but use must still obey the native lifecycle of the underlying system.

Finding `AddToMap` at startup does not mean the template loader is ready to accept a custom registration yet.

## How we interact with it

### Resolve once

Cache reflected handles rather than performing reflection repeatedly in hot paths.

### Fail closed

If the expected member does not exist or has an unexpected signature, disable only the affected operation.

### Verify the owner state

Private member availability and lifecycle readiness are separate checks.

### Revalidate after updates

A game update can:

- rename a member;
- change a signature;
- move ownership;
- change semantics while the symbol still exists.

## Why this route

Several working proofs need private surfaces because the game exposes normal lookup but not public registration/mutation APIs.

The correct lesson is not "private reflection is bad" or "private reflection is fine."

The lesson is:

> private reflection is a version-scoped intervention that needs explicit compatibility and failure handling.

## What goes wrong

- repeated reflection every frame;
- broad name-only lookup selecting the wrong overload;
- member exists but semantics changed;
- private collection is mutated before/after its valid phase;
- reflection exception propagates into a native UI/gameplay lifecycle;
- a one-build success is described as stable API support.

## How to verify

For each private integration:

1. resolve exact member;
2. verify signature/type;
3. record build/assembly identity;
4. verify lifecycle precondition;
5. test intended operation;
6. test missing/invalid member failure;
7. re-run after relevant game update.

## Current proof boundary

This handbook includes private surfaces only where there is concrete evidence and a clear reason. Private members should never be presented as stable public APIs.
