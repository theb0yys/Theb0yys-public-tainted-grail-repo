# Compatibility Claim Record

Use this record whenever documentation says a mod, hook, framework surface or mechanism “works”.

## Required fields

```text
Subject:
Claim:
Game build/version:
Runtime: Mono | IL2CPP | Merlin | hybrid
Loader/version:
Relevant dependency versions:
Relevant assembly/file hashes:
Mod/repository commit:
Validation lane:
Test performed:
Observed result:
Not tested:
Last verified:
```

## Validation lane

Use the repository's evidence vocabulary:

- Static/source inspected
- Build validated
- Loader validated
- Runtime proven
- Persistence proven
- Compatibility tested
- Release validated
- Unknown / not tested

See [Public Evidence Standard](../../sources/evidence-standard.md).

## Runtime separation

Do not write:

```text
compatible: yes
```

when the actual evidence is:

```text
Mono: Runtime proven on build X
IL2CPP: Build validated only
```

State both lanes separately.

## Dependency version separation

Do not collapse:

- public package version;
- BepInEx plugin version;
- assembly/file version;
- public API/contract version.

For shared infrastructure see [Public distribution and versioning](../../tooling/ecosystem/distribution-and-versioning.md).

## Update rule

When a game/framework update occurs:

1. keep the old record as historical scope if useful;
2. do not silently carry the claim to the new build;
3. rerun the smallest claim-fit validation;
4. update `Last verified` and exact scope.

“Latest” is not a reproducible version.
