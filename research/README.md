# Research

Use this section when you **do not yet know enough about a FoA behavior to build against it confidently**.

Research is where uncertain questions stay while you work out the exact identity, native owner, lifecycle, downstream effects, and runtime behavior.

## Choose what you need

### [Research methods](methods/README.md)

Use these when you need a repeatable way to answer a question such as:

- Which native system owns this behavior?
- Why is this hook too early?
- Does decompiled code prove what I think it proves?
- Is this GUID/name actually the exact object I need?
- What would count as a real proof of this mechanic?

### Investigations

Use investigations for one specific unresolved subject.

Current areas include:

- [Persistence](investigations/persistence/README.md)
- [Travel](investigations/travel/README.md)
- [Rule-pack-first bug recovery](investigations/bugfixes/rule-pack-recovery.md)
- [Verifying the current build before fixing a bug](investigations/bugfixes/current-build-first.md)
- [External dialogue-engine integration](investigations/dialogue/external-engine-integration.md)
- [Dialogue migration and fallback](investigations/dialogue/migration-and-fallback.md)

### [Case studies](case-studies/README.md)

Use these to see what happened in real mod work: the initial assumption, what failed, what evidence changed the model, what eventually worked, and what remained unproven.

### [Sources](sources/README.md)

Use this when you need to trace where a claim came from—Questline material, public mod source, exact-build static inspection, or another evidence source.

## A useful investigation should answer

~~~text
what exactly is the subject?
→ what owns it?
→ when does it exist?
→ who consumes the result?
→ what cleanup/restoration occurs?
→ what evidence proves the claim?
~~~

A hook compiling or firing once is not enough to move an uncertain claim into established Knowledge.
