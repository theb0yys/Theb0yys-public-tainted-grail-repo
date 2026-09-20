---
document_type: investigation
scope: quest/save softlock recovery research
runtime: mono
evidence:
  project_design: ACCEPTED_READ_ONLY_RECOVERY_MODEL
  mutation: BLOCKED_BY_DEFAULT
last_verified: 2026-09-20
---

# Rule-Pack-First Quest Recovery

A generic quest editor is the wrong starting point for FoA recovery work.

Quest/objective/story-flag writes can trigger:

- native events;
- effectors;
- marker changes;
- GameplayMemory changes;
- follow-up state transitions;
- rewards or irreversible consequences.

## Safer model

Treat each repair as a **named rule pack** with:

- exact affected build(s);
- exact quest/objective/actor/item identities;
- detection inputs;
- evidence required;
- backup requirement;
- allowed first prototype;
- blocked actions;
- validation plan;
- rollback/recovery expectation.

Example rule-pack classes:

- missing/duplicate reward;
- tracker/marker repair;
- dialogue/interaction reset;
- NPC presence recovery;
- quest-item recovery/cleanup;
- quest-state patch.

## Default posture

Until a named recipe closes its evidence:

```text
diagnose
→ preview
→ backup readiness
→ dry-run action plan
→ buttonEnabled = No
→ writeEnabled = No
```

Do not expose generic “complete/fail/reset/toggle flag” controls merely because the methods exist.
