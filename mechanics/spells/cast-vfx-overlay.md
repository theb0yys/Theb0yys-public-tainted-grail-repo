---
document_type: mechanic
scope: attach mod-owned VFX when player magic casting begins
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: NOT_GENERALIZED
  identity: HEURISTIC
last_verified: 2026-09-20
---

# Spell Cast VFX Overlay

One source-inspected route patches:

`VCCharacterMagicVFX.CastingBegun`

and attaches a short-lived mod-owned VFX prefab to the native magic VFX component when the cast belongs to `Hero.Current`.

## Shape

```text
native CastingBegun
→ verify player-owned item
→ classify template into mod VFX family
→ load mod-owned prefab
→ instantiate under native magic VFX transform
→ destroy after bounded lifetime
```

## Critical identity boundary

The inspected implementation classifies spell families from **template-name fragments**.

That is useful project logic, but it is **not proof of native spell/VFX identity relationships**.

Do not publish heuristic family names as native game truth. Use [Name heuristic vs native identity](../../investigate/name-heuristic-vs-native-identity.md) when promoting spell knowledge.
