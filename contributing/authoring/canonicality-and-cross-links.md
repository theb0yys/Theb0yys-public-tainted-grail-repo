# Canonicality and Cross-Link Rules

Use this rule when two documentation pages begin explaining the same architecture, lifecycle, or capability.

## One canonical owner per explanation

Every durable technical explanation should have one canonical page.

Other pages may:

- summarize the minimum context needed for their task;
- link to the canonical page;
- explain a different reader concern such as diagnosis, procedure, evidence, or consumer behavior.

They should not maintain a second full copy of the same architecture.

## Choose the canonical location by role

| Knowledge | Canonical location |
| --- | --- |
| native FoA ownership / architecture | `systems/` |
| reusable bounded capability | `mechanics/` |
| unresolved ownership/evidence | `investigate/` |
| symptom-first troubleshooting | `diagnose/` |
| task procedure | `how-to/` |
| exact lookup / catalogue | `reference/` |
| source/evidence provenance | `sources/` |
| shared mod infrastructure | `tooling/` |
| reusable starter code | `templates/` |
| mechanism demonstration | `examples/` |
| lessons from real work | `case-studies/` |

## Overview vs detailed subsystem pages

A broad overview may name a subsystem and its relationship to neighboring owners.

The detailed subsystem page owns:

- full lifecycle;
- core types;
- storage/transport details;
- invariants;
- subsystem-specific proof boundary.

Example:

```text
systems/world/story-quest-dialogue.md
  owns runtime Story/quest/dialogue consumer behavior

systems/world/story-graphs/README.md
  owns Story Graph authoring → compilation → binary/runtime architecture
```

## Cross-link rule

When content belongs elsewhere, write:

1. one sentence explaining why the reader needs it;
2. one direct link to the canonical page;
3. only the local implications needed for the current page.

Do not copy several sections “for convenience”.

## Index rule

A directory `README.md` is navigation, not hidden storage.

It must expose:

- direct Markdown child pages;
- direct child sections that have their own `README.md`.

The documentation verifier enforces this.

## Legacy path rule

When a page moves:

- update internal links to the canonical path;
- do not preserve stale links in authoring/migration packets;
- use an explicit redirect/shim only when external compatibility justifies one.

## Front matter rule

Front matter is optional unless a document family requires it.

When present, it must include:

- `document_type`
- `scope`
- `last_verified` in `YYYY-MM-DD`

Additional evidence/runtime/build keys remain document-specific.

Do not add empty metadata merely to make pages look uniform.

## Duplication review

When reviewing a suspected duplicate, decide among:

- **merge** — same role and same claims;
- **split** — different reader roles, but repeated architecture should move to one owner;
- **redirect** — one page has no independent role;
- **keep** — overlap is only enough context to operate.

The goal is not zero repetition. The goal is one authoritative explanation for each technical claim family.
