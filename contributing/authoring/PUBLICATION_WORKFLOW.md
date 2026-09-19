# Mandatory Private-to-Public Publication Workflow

This process converts researched private knowledge into public documentation without copying private prose or losing evidence boundaries.

## 1. Select one knowledge unit

Write one reader question:

> The reader needs to understand / do / diagnose **X**.

If the subject contains multiple independent questions with different owners or evidence lifecycles, split the work before drafting.

## 2. Resolve provenance and scope

Record:
- exact source repository/commit or public source;
- game/runtime/build scope where relevant;
- technical claim set;
- evidence lanes available;
- missing or contradictory evidence;
- public/private redistribution boundary.

Stop when a consequential claim has no inspectable support.

## 3. Extract claims, not prose

Reduce source material into bounded claims:
- identity;
- owner;
- lifecycle;
- intervention;
- downstream consumer;
- failure;
- cleanup;
- evidence.

Do not copy a private narrative wholesale.

## 4. Reconstruct the reasoning

Record why the final process has its current shape:
- initial model;
- evidence that challenged it;
- corrected owner/lifecycle;
- rejected or unsafe alternatives;
- resulting bounded mechanic.

A final code path without ownership rationale is not enough for a teaching page.

## 5. Define the reader outcome

Specify:
- prerequisites;
- what the reader should understand afterward;
- what the reader should be able to do afterward;
- what the page deliberately does not teach.

Do not use “beginner” or “advanced” as the only audience definition.

## 6. Choose the document archetype

Use the mandatory types in [DOCUMENTATION_ARCHITECTURE.md](DOCUMENTATION_ARCHITECTURE.md).

Split the material when one page would need incompatible body structures.

## 7. Map canonical dependencies

Link to existing canonical:
- system explanations;
- mechanics;
- reference identities/hooks;
- evidence/compatibility notes;
- examples/cases.

Do not duplicate long background material for convenience.

## 8. Design the explanation

Plan only diagrams that answer a concrete question:
- owner;
- lifecycle;
- sequence;
- data/resource flow.

Select a worked example whose history actually demonstrates the lesson.

## 9. Write public-safe material

Use:
- clean-room prose;
- independently authored code;
- bounded pseudocode where necessary;
- exact public/researchable identities where appropriate;
- explicit failure handling;
- explicit cleanup.

Do not expose private source simply to make the public explanation complete.

## 10. Attach evidence status

State the exact status of:
- source/static;
- build;
- loader;
- runtime;
- persistence;
- compatibility;
- release/package.

The public rewrite does **not** inherit the private implementation's execution status.

## 11. Completeness review

The page fails completeness review if a required:
- owner;
- readiness condition;
- state transition;
- downstream consumer;
- terminal success condition;
- cleanup path;
- failure branch;
- proof boundary

is left implicit for that archetype.

## 12. Fidelity review

Map every material public technical claim back to inspectable evidence.

Stop when simplification:
- broadens the claim;
- erases a meaningful failure;
- converts static evidence into runtime wording;
- converts one runtime result into persistence/compatibility support;
- turns a candidate mechanic into universal guidance.

## 13. Validate the public artifact

Where the public page contains executable example code or a concrete procedure, validate that **exact public artifact** separately.

A proven private mechanism does not make a newly authored public example runtime-passed.

## 14. Publish and maintain

Record:
- version scope;
- last verified;
- known limitations;
- canonical owner page;
- superseded/legacy links where relevant.

Game/runtime/loader changes reopen review for patch-sensitive material.
