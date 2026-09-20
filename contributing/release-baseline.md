# Public Release Baseline

This file records the public-release baseline used by the gated operational-hardening work.

## Baseline source

- Source branch: `main`
- Source commit: `0ee572dc0e8e514321a88dcb36fb6b66f1eb13da`
- Baseline date: 2026-09-20
- Repository posture: public, source-only community modding platform

## Runtime and authoring scope

The repository contains distinct Mono, IL2CPP, Merlin, and hybrid authoring/runtime lanes. Support is not assumed to be uniform across those lanes.

A repository-wide game-build compatibility claim is **not established by this baseline**. Version-sensitive claims remain scoped by the evidence recorded in the owning guide, knowledge page, research record, example, or component.

## Evidence posture

- `examples/` and `templates/` demonstrate source structure or bounded mechanisms. Their presence does not by itself prove runtime behaviour, persistence, compatibility, or release readiness.
- `research/case-studies/` may record concrete runtime evidence, but only to the extent explicitly stated by the individual case study.
- `knowledge/` should distinguish established facts from inference and state build/runtime scope where relevant.
- Static/source evidence, runtime evidence, persistence evidence, and release proof remain separate evidence lanes.

## Gate 0 acceptance criteria

Gate 0 is complete only when all of the following are true:

1. The public-surface guard passes on the Gate 0 branch head.
2. Known machine-specific public paths identified during the baseline audit are removed or parameterized.
3. A general repository licence decision is explicit and the repository documentation matches it.
4. No later operational-tooling work is mixed into the Gate 0 baseline.

## Current Gate 0 status

- Public-path cleanup: **IN_PROGRESS**
- Public-surface CI: **IN_PROGRESS**
- General repository licence: **BLOCKED** — owner licence choice is required; no licence is inferred from public visibility.
- Operational tooling phases: **NOT_RUN**
