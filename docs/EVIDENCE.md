# Testing and Evidence Status

Most readers do not need to memorize the labels on this page.

When a guide says something is **confirmed from source**, it means the relevant code or authoring contract was inspected.

When it says something was **run in the editor or game**, that means somebody actually executed that exact step in the stated environment.

Those are different kinds of confidence. A build, source inspection, editor check, plug-in load, and in-game behaviour test each prove different things.

## The simple rule

Do not turn one result into a stronger claim than it supports.

For example:

- source inspection does not prove a mod loads;
- a successful build does not prove the game loads the DLL;
- a plug-in loading does not prove the feature behaves correctly;
- editor success does not prove game-runtime behaviour;
- a development DLL working does not prove a packaged release is correct.

The exact environment matters too: game build, runtime lane, loader/toolkit version, and the exact example or artifact tested.

## Formal labels used in this repository

The labels below are mainly for reference pages, cookbook entries, technical reviews, and compatibility records.

### Source-path labels

**STATIC_CONFIRMED**

An editor/toolkit authoring contract, menu, component relationship, inheritance rule, addressable grouping, or preparation path is confirmed in inspected source.

This label is normally used for content-authoring contracts rather than runtime plug-in behaviour.

**SOURCE_CONFIRMED**

A concrete implementation, type, method, field, or target path exists in inspected source, but the reviewed material does not establish a completed build, load, or feature-behaviour result for that path.

**SOURCE_BUILD_EVIDENCED**

The relevant source/target path was identified and a corresponding build result exists, but useful live feature behaviour has not been established.

**LOAD_EVIDENCED**

Build/deploy/plug-in-load or patch-registration evidence exists, but the intended feature behaviour itself is not fully demonstrated.

**RUNTIME_EVIDENCED**

The relevant mechanism or feature path produced useful observed behaviour in a running game or equivalent runtime environment.

This still applies only to the environment and scope that were actually tested.

### Exact execution-status labels

**NOT_RUN**

The exact public example, recipe, editor path, or runtime path being described was not executed as part of the stated documentation/test pass.

**RUNTIME_PASSED**

The exact stated runtime test was executed and the documented success condition was observed in the stated environment.

Use this only when the page identifies what was run closely enough for the claim to be meaningful.

**NOT_PROVEN**

The current material does not establish the claimed behaviour for that exact target or adaptation.

This is useful when a reusable mechanism is known but a new target, port, or public rewrite has not itself been verified.

## Why an underlying path and a public example can have different statuses

A public teaching example may be rewritten from a mechanism that has strong runtime evidence in a maintainer workspace.

That does **not** automatically make the rewritten public example runtime-proven.

For example:

- underlying mechanism: **RUNTIME_EVIDENCED**
- newly written public teaching example: **NOT_RUN**

The public example gains its own stronger status only when that exact example is built and tested.

## Editor, runtime, and release checks are separate

For content authoring, useful checkpoints include:

1. source/tool contract confirmed;
2. Unity/editor project opens cleanly;
3. definition can be created and saved;
4. editor-side validation or preview succeeds where applicable;
5. the authored content is tested in the game;
6. the final packaged artifact is tested through its intended install path.

For runtime plug-ins, useful checkpoints include:

1. target/runtime research;
2. source builds;
3. loader starts;
4. plug-in loads;
5. patch or hook registers;
6. intended behaviour is observed;
7. rollback/unload behaviour works where relevant;
8. the packaged release is tested.

A page should claim only the checkpoints it actually reached.

## Recording a useful compatibility result

When recording that something worked, include enough context to reproduce the claim:

- game version/build;
- Mono or IL2CPP;
- BepInEx/toolkit version or commit;
- mod/example version or source commit;
- exact action performed;
- expected result;
- observed result;
- known limitations.

This turns "it worked" into a result another person can use.
