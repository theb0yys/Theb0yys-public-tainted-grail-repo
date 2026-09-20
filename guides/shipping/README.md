# Ship and Maintain Mods

Infrastructure is the machinery around the mod or framework itself.

This section covers repository structure, CI, public safety, contribution flow, versioning, validation, and releases.

- [Validate mods after a game update](game-update-validation.md)

## Repository structure

Keep authored source separate from:

- generated build output;
- extracted game content;
- local references;
- logs;
- release artifacts;
- test evidence that contains private data.

A clean repository should make it obvious which files are source and which are generated.

## CI

Useful hosted CI checks include:

- formatting/static checks;
- unit tests;
- source-only builds where dependencies allow them;
- public-surface checks;
- package-layout validation.

Hosted CI cannot prove behaviour that requires a licensed local game installation unless that exact environment is actually available. Keep local runtime validation as a separate evidence lane.

This repository already contains a public-surface guard intended to reject common binaries, archives, assets, oversized files, obvious secrets, and private-path patterns.

## Contribution workflow

A simple contribution loop:

1. create a focused branch;
2. make one coherent change;
3. run applicable checks;
4. open a pull request;
5. state exactly what was tested;
6. review;
7. merge only the reviewed scope.

Good pull-request evidence identifies:

- affected runtime/content lane;
- exact checks run;
- exact runtime environment when runtime behaviour was observed;
- known limitations.

Do not describe a static check as a runtime pass.

## Versioning

Use a consistent versioning scheme.

A common model is major.minor.patch, but consistency matters more than the specific scheme.

Release notes should answer:

- What changed?
- What game/runtime versions were tested?
- Did dependencies change?
- Are configuration or saves affected?
- Is rollback possible?
- What is known not to work?

## Reproducible releases

The public [developer-tool scripts](../../platform/developer-tools/README.md) include a project doctor, sanitized diagnostic bundle generator, and reproducible release-package builder for both runtime lanes.

Where practical, trace a release to:

- source commit;
- build configuration;
- packaged artifact;
- checksum.

Do not claim a development DLL and a published archive are identical unless artifact identity was verified.

A useful pre-package sequence is:

~~~text
Test-FoAModProject
→ Build-FoAMod
→ New-FoARelease
→ inspect release-manifest.json + SHA256SUMS.txt
→ perform runtime/feature/save validation separately
~~~

New-FoARelease does not perform runtime or feature validation; package construction cannot substitute for those checks.

## Infrastructure is not authority

A green CI check proves only the checks that actually ran.

It does not automatically prove:

- editor behaviour;
- game runtime compatibility;
- save safety;
- deployment safety;
- release readiness.

Keep each evidence lane honest and explicit.
