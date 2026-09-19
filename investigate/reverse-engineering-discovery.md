<!-- Canonical documentation path. Migrated from docs/reference/REVERSE_ENGINEERING_DISCOVERY.md. -->
# Reverse Engineering and Discovery

> **Reference page.** Use this when the handbook does not yet contain the GUID, type, method, lifecycle, or ownership relationship you need.

## What this system is

Reverse engineering here means evidence gathering for interoperability and modding.

The working research keeps separate evidence lanes:

- official/developer documentation;
- official/open source snapshots;
- managed assembly inspection/decompilation;
- IL2CPP metadata/interop inspection;
- native binary/metadata inspection where required;
- repository implementation source;
- runtime diagnostics/logs/screenshots;
- controlled experiments.

Each lane can support different claims.

## Who owns it in FoA

The native subsystem being investigated remains the owner.

Decompiler output does not create a public API.

A mod implementation does not become canonical game truth simply because it compiles.

## Important identities, types, and methods

Capture exact evidence coordinates:

- game build/version/branch;
- Mono or IL2CPP;
- assembly/file name;
- SHA-256 and MVID where practical;
- fully qualified type;
- exact method signature;
- field/property name;
- template GUID;
- Addressables address;
- scene/object path;
- source commit;
- runtime marker/log line.

Preserve the exact native reference separately from any friendly handbook/project label.

## Where it exists in the lifecycle

A useful discovery progression is:

~~~text
question
→ search official/source material
→ locate exact identity/type
→ inspect caller/owner
→ map lifecycle
→ design read-only diagnostic
→ observe runtime
→ make one controlled mutation
→ validate cleanup/persistence
→ publish reusable rule
~~~

Do not begin with mutation when observation can answer the question.

## How we interact with it

### Managed Mono research

Mono is especially useful for:

- readable type names;
- method bodies;
- service/template lifecycle;
- serialization relationships;
- call graphs.

But Mono is a supported modding lane and analysis target—not proof that IL2CPP has identical member layout.

### IL2CPP research

Use the generated interop/metadata for the exact installed build.

Verify the target against that generated surface rather than assuming a Mono signature survives unchanged.

### Official source snapshots

Use Questline/Merlin source to establish intended relationships, editor structures and lifecycle concepts.

For patch-sensitive runtime behavior, compare that source against the current game binary/build instead of treating a source snapshot as timeless.

### Runtime diagnostics

Prefer one bounded marker per question:

- hook fired;
- GUID resolved;
- service ready;
- item count before/after;
- exact state transition;
- cleanup marker.

## Why this route

Most failed mod attempts in the research came from collapsing discovery into implementation:

- a name looked right;
- a method sounded related;
- a prefab loaded;
- a field was writable;
- one screen opened.

Mapping **owner + identity + lifecycle + downstream consumer** first removes those false positives.

## What goes wrong

### Newest-file-wins

New evidence does not automatically erase older evidence. Contradictions need explicit reconciliation or supersession.

### Decompile = runtime proof

A decompiled method can prove call shape without proving your live hook/feature works.

### Mod source = native identity truth

A source mod can contain a heuristic, guessed identity, compatibility shim or stale target.

### Bulk GUID dumps without relationships

A thousand GUIDs are less useful than one identity with known owner, type, relationships, version and lifecycle.

### Rerunning an unchanged failure

If no prerequisite changed, rerunning the same failed experiment usually produces noise rather than evidence.

## How to verify

Before promoting a discovery into handbook knowledge, record:

- claim;
- evidence class;
- exact locator;
- version/build scope;
- contradiction state;
- runtime validation when required;
- allowed use;
- forbidden generalisations;
- review/promotion state.

## Current proof boundary

This page documents the evidence discipline used to build the public handbook. It intentionally does not publish bulk decompiled source, proprietary assets, private machine paths, or unreviewed data dumps.
