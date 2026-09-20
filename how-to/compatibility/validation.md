# Validation and Compatibility

> **Reference page.** Use this whenever a claim moves from "we found a path" to "this works" or "this is safe to ship."

## What this system is

~~~text
source exists → project builds → plug-in loads → target resolves
→ feature executes → downstream behavior is correct
→ cleanup works → persistence works if required
→ packaged artifact is tested
~~~

One stage never implies the next.

## Who owns it in FoA

The native subsystem owns the behavior. The mod owns its intervention, diagnostics, cleanup and compatibility claim.

## Important identities, types, and methods

Record game build, runtime lane, BepInEx version, relevant assembly hash, mod version/commit, exact targets/identities, action, expected/observed result, failures, and not-tested boundaries.

## Where it exists in the lifecycle

Validation follows the feature's real lifecycle. Custom items require registration, provider lookup, Item creation, acquisition, presentation/use, and separate persistence/uninstall/collision tests if durable production use is claimed.

## How we interact with it

Use the smallest test that proves the exact claim. On failure, identify the earliest failed stage, change the prerequisite or hypothesis, record the failure, and narrow the claim.

## Why this route

The research corpus separates source maturity, runtime validation and permission. A documented fact may still be unsafe to mutate or persist.

## What goes wrong

Build called runtime proof; load called feature proof; current-session behavior called save proof; stale build evidence treated as current; one lane reused for another; missing negative cases; untested package differs from tested artifact.

## How to verify

Ask: exact claim? exact environment? failure cases? cleanup? persistence? packaged artifact? update revalidation?

## Current proof boundary

Unrun gates remain explicitly unproven.
