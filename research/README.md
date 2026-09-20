# Research

Use this section when a FoA behaviour, identity, lifecycle, or integration path is not yet known well enough to document as established fact.

- [Finding the native owner](methods/finding-the-native-owner.md)
- [Tracing a lifecycle](methods/tracing-lifecycles.md)
- [Static/source evidence vs runtime evidence](methods/source-vs-runtime.md)
- [Name heuristic vs native identity](methods/name-heuristic-vs-native-identity.md)
- [Proving a new mechanic](methods/proving-a-new-mechanic.md)
- [Persistence investigation](investigations/persistence/README.md)
- [Travel investigation](investigations/travel/README.md)
- Bug fixes
  - [Rule-pack-first recovery](investigations/bugfixes/rule-pack-recovery.md)
  - [Verify current build first](investigations/bugfixes/current-build-first.md)
- Dialogue
  - [External engine integration](investigations/dialogue/external-engine-integration.md)
  - [Migration and fallback](investigations/dialogue/migration-and-fallback.md)

A useful investigation should answer, in order:

```text
what is it?
→ what owns it?
→ when does it exist?
→ who consumes the result?
→ how is it cleaned up?
→ what evidence proves the behaviour?
```

Do not turn an assumption into a Knowledge page just because a hook compiles or fires once.
