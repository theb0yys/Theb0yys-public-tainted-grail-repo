---
document_type: framework
scope: Avalon Core public dependency/discovery boundary
runtime: mono
evidence:
  source: ACCEPTED_PROJECT_CONTRACT
  runtime: READ_ONLY_CONSUMER_LIVE_LOAD_VALIDATED
last_verified: 2026-09-20
---

# Avalon Core: Public Dependency Boundary

Avalon Core is a **project-owned shared foundation**, not a replacement for BepInEx and not a native FoA API.

Its mature public baseline was deliberately narrow before runtime mutation was introduced.

## Public read-only baseline

A downstream consumer may:

- declare a BepInEx dependency on Avalon Core;
- read host identity/version;
- inspect copied trust/diagnostic reports;
- obtain documented built-in engines by exact ID;
- register/query declarative adapter descriptors;
- query capability descriptors;
- check service-contract versions;
- inspect safety-gate metadata;
- produce discovery/read-only reports.

## What descriptor registration means

```text
provider describes adapter/capability
→ Core validates metadata / duplicates / version rules
→ consumer discovers matching capability
→ consumer decides whether compatible/allowed
```

It does **not** mean Core executes the capability.

Runtime mutation, asset loading, spawning, UI, save writes and gameplay callbacks were explicitly separate gates.

## Fail-closed consumer rule

If a documented Core API is missing, incompatible or blocked, the consumer should fall back to `action=none` / `action=read-only`, not guess a private implementation path.

A shared dependency is valuable when it centralizes contracts without silently taking ownership of every feature.
