---
document_type: investigation
scope: reproducible local game inspection
last_verified: 2026-09-20
---

# Local Game Investigation

Use this workflow when a FoA question depends on the installed game rather than repository source alone.

The goal is to produce a repeatable **local evidence workspace** without committing proprietary binaries, generated interop assemblies, bulk decompiled source, extracted assets, saves, or private machine data.

## Evidence boundary

Local inspection can establish facts about the exact artifacts you inspected:

```text
installed game artifact
→ local reference/decompilation view
→ type/member/call-path evidence
→ candidate owner/lifecycle
```

It does **not** by itself establish:

- that a Harmony patch installs;
- that a target executes at runtime;
- that the visible game behaviour matches the static model;
- persistence/save correctness;
- compatibility with other builds;
- release readiness.

Route those claims to runtime or persistence evidence separately.

## 1. Record the inspected build first

Before inspecting types or methods, record enough information to identify the local build you are using:

- game version/build identifier available from the installation or runtime;
- runtime lane: **Mono** or **IL2CPP**;
- relevant assembly/reference names;
- date of inspection;
- any local loader/interop version that materially affects the evidence.

If the exact build cannot be identified, mark version-sensitive conclusions **PARTIAL** rather than silently treating them as current.

## 2. Keep research output outside the public tree

Use an ignored local workspace such as:

```text
.local-research/
  <build-id>/
    references/
    decompiled/
    assets/
    logs/
    notes/
```

The directory names are conventional only. The important rule is that generated or extracted commercial material remains local.

Do not make repository documentation depend on an absolute path from one machine.

## 3. Choose the correct runtime lane

### Mono

Inspect the managed assemblies from the user's installed game. Typical questions may involve `TG.Main.dll` and relevant `Awaken.*` assemblies.

A local decompiler such as ILSpy can provide a navigable reference/decompilation view for:

- declaring types;
- members and overloads;
- call sites;
- attributes;
- candidate services/owners;
- data and lifecycle relationships visible in managed code.

Do not copy the resulting decompiled tree into this repository.

For a repeatable ILSpy command-line export, record the tool version and write only to the ignored local workspace:

```powershell
ilspycmd --version
ilspycmd --nested-directories -p -o ".local-research/<build-id>/decompiled/TG.Main" "<path-to-installed-TG.Main.dll>"
```

`-p` requests a project-style decompilation and `-o` supplies the output directory. Keep the actual installed-game path out of committed documentation and evidence records.

### IL2CPP

Treat IL2CPP as a separate evidence lane.

Generated interop/reference assemblies can help identify managed-facing types and signatures, but they are generated compatibility artifacts, not the original managed implementation.

If a claim depends on native IL2CPP implementation details, generated interop metadata alone is insufficient. Keep native/static evidence distinguished from generated managed-facing reference evidence.

## 4. Start from an exact question

Write the investigation question before searching.

Good:

> Which owner decides whether this inventory action is legal, and which method represents the final native decision?

Weak:

> Search inventory classes until something looks patchable.

Record:

- exact subject;
- desired observed outcome;
- current known behaviour;
- candidate owner(s);
- runtime/build scope.

## 5. Establish identity before semantics

For each candidate target, record only the minimum identity needed to reproduce the search:

- assembly;
- namespace;
- declaring type;
- member name;
- overload/signature when relevant;
- important caller/callee identities.

A matching name is not proof that two methods have the same role. Use [Name heuristic vs native identity](name-heuristic-vs-native-identity.md) when identities are ambiguous.

## 6. Trace ownership and lifecycle

Use static inspection to answer:

1. Who creates or resolves the object?
2. Which service/model/element owns authoritative state?
3. What means "ready"?
4. Which method/event represents the meaningful transition?
5. What downstream consumer uses the result?
6. What cleanup/restoration path exists?
7. Which state appears save-owned versus session-only?

Then compare the result with [Finding the native owner](finding-native-owner.md) and [Tracing a lifecycle](tracing-lifecycles.md).

## 7. Escalate unknown execution to runtime evidence

If the remaining question is "does this actually execute here?", stop extending the static claim.

Route the target to a runtime observation method instead:

```text
static target identified
→ exact runtime target configured
→ patch/observer installation checked
→ invocation observed
→ downstream behaviour checked separately
```

Gate 1 documents this handoff. Runtime tracer implementation belongs to the later runtime-tooling gate.

## 8. Preserve a public-safe evidence note

A public note may record:

- build/runtime scope;
- exact symbol identities;
- the owner/lifecycle conclusion;
- what static evidence supports;
- what remains unproven;
- the next evidence lane required.

Do not paste bulk decompiled method bodies merely to make the note look stronger.

## Result states

Use the repository status vocabulary:

- **PASSED** — the bounded static question was answered for the identified artifacts.
- **PARTIAL** — useful evidence exists but identity, semantics, or build scope remains incomplete.
- **FAILED** — inspected evidence contradicts the proposed model.
- **BLOCKED** — required local artifact/tool/reference is unavailable.
- **NOT_RUN** — inspection has not been performed.
- **NOT_APPLICABLE** — the method does not apply to the runtime/artifact in question.

## Public boundary

Never commit:

- proprietary game binaries;
- generated IL2CPP interop assemblies;
- bulk decompiled game source;
- extracted commercial assets;
- local saves;
- unredacted game/log dumps;
- absolute user/game-library paths;
- credentials or personal data.

The public repository should preserve **reproducible conclusions and methods**, not redistribute the commercial game.
