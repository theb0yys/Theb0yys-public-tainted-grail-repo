# Ship and Maintain Mods

This section covers the work around the mod itself: repository structure, CI, public safety, versioning, validation, packaging, and releases.

## Keep source separate from local/generated files

Do not mix authored source with:

- build output;
- extracted game content;
- local game references;
- logs;
- release archives;
- test evidence containing private data.

Someone opening the repository should be able to tell immediately which files are source and which are generated or local-only.

## CI

Useful CI checks include:

- formatting and static checks;
- unit tests;
- source-only builds where dependencies allow them;
- public-repository safety checks;
- package-layout validation.

Hosted CI cannot prove behavior that requires a licensed local game installation unless that environment is actually available to the runner.

This repository includes a public-surface guard that rejects common binaries, archives, commercial-style assets, oversized files, obvious secrets, and private absolute-path patterns.

## Contribution workflow

A simple contribution flow is:

1. create a focused branch;
2. make one coherent change;
3. run the checks that apply;
4. open a pull request;
5. state exactly what you tested;
6. review the change;
7. merge only the reviewed work.

A useful pull request says:

- which runtime or content workflow it affects;
- which checks were run;
- which game/runtime environment was used for any live observation;
- what remains untested or uncertain.

Do not describe a compile/static check as proof that the feature worked in game.

## Versioning

Use a consistent version scheme. `major.minor.patch` is common, but consistency and clear release notes matter more than the exact scheme.

Release notes should answer:

- What changed?
- Which game/runtime versions were tested?
- Did dependencies change?
- Are configuration or saves affected?
- Can the user roll back safely?
- What is known not to work?

## Reproducible releases

Where practical, record:

- the source commit;
- build configuration;
- packaged artifact;
- checksum.

Do not claim that a development DLL and a published archive are identical unless you actually verified their identity.

## What a green check means

A green CI run proves only the checks that ran.

It does not automatically prove:

- Unity/editor behavior;
- in-game compatibility;
- save safety;
- deployment safety;
- release readiness.

State each kind of evidence separately.
