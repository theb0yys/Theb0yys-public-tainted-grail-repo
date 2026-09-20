# Debug the First Failed Stage

The fastest useful debugging question is:

> What is the earliest stage that failed?

## Stage order

Use this sequence:

```text
loader starts
→ plug-in discovered
→ dependencies resolve
→ patch/registration installs
→ native target resolves
→ owner is ready
→ action executes
→ downstream consumer reacts
→ cleanup/restoration works
→ persistence works if claimed
→ packaged release behaves the same
```

Do not debug stage 8 while stage 4 is already failing.

## Evidence to collect

For each stage, capture the smallest useful fact:

| Stage | Evidence |
| --- | --- |
| loader | BepInEx startup/version/runtime |
| plug-in | plugin GUID/name/version load line |
| dependencies | resolved/missing assembly/plugin versions |
| patch | exact target + patch installed |
| native identity | type/method/GUID resolves |
| readiness | exact owner/readiness predicate |
| execution | bounded action marker/result |
| downstream | native consumer/result observed |
| cleanup | unregister/discard/release marker |
| persistence | save/restart/load proof |
| release | actual packaged artifact/hash |

## Fix the first reliable error

Later exceptions can be consequences.

Example:

```text
dependency failed to load
→ patch class never initializes
→ target appears “not patched”
→ feature appears broken
```

The dependency failure is the primary defect.

## After a game update

Re-prove the stack from the bottom.

Do not start by rewriting the gameplay patch before confirming loader, runtime lane and target identity.

For symptom routing see [Diagnose](../../diagnose/README.md).
