# Documentation Archetypes

Do not force every technical page into one body template.

## Write for the modder first

Before describing repository structure, evidence taxonomy, or ownership vocabulary, tell the reader:

1. what this page helps them do;
2. when they should use it;
3. what they need to know before acting.

Prefer:

> Use this page when a custom item works in the current session but disappears or fails after loading a save.

over:

> This page documents the persistence validation lane for item-registration consumers.

Technical terms such as **native owner**, **lifecycle**, **static evidence**, or **runtime evidence** are useful when they change a decision. Repository-maintenance terms such as **surface**, **lane**, **posture**, **promotion**, or **canonical owner** should generally stay out of reader-facing prose unless the page is specifically about repository governance.

## Native system

Explain what the system does, what game component actually controls it, the important types/data, when it exists, how data flows through it, how cleanup works, and what remains unknown.

## Mechanic

Explain the goal, prerequisites, game system involved, the full working path, where the mod intervenes, likely failure points, cleanup, save/compatibility effects, and how to verify it.

## Investigation

Record the question, known facts, hypotheses, observation strategy, findings, rejected assumptions, corrected understanding, remaining unknowns, and next verification step.

## Case study

Explain the intended result, the initial assumption, symptoms, observations, what turned out to be wrong, what changed, how it was verified, and what is still uncertain.

## Troubleshooting

Start from the user's symptom, find the earliest likely failed step, collect observations that distinguish the possibilities, then link back to the relevant system or mechanic page.

## Reference

Keep exact IDs, types, methods, hooks, versions, and compatibility notes compact and searchable.

## Framework or tooling contract

Explain what the shared tool provides, who should use it, initialization and cleanup, version/capability checks, registration/unregistration, failure handling, migration, and current availability.
