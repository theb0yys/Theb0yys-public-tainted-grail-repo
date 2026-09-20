# Release Evidence and Artifact Identity

A release is not proven because the development DLL worked.

## Release chain

```text
source commit
→ build
→ staged package
→ archive/artifact
→ install from artifact
→ loader/runtime validation
→ published file
```

Each transition can introduce mistakes.

## Record artifact identity

Where practical, record:

- source commit;
- build configuration;
- package version;
- archive filename;
- SHA-256;
- expected file list;
- tested runtime/game build.

This lets support reports identify what was actually installed.

## Package scan

Before publication, inspect the final archive for:

- only intended plugin/runtime files;
- required README/changelog/licence material;
- no game DLLs;
- no BepInEx loader binaries unless explicitly and legally part of the product;
- no generated interop assemblies unless explicitly authorized/required;
- no saves/logs/configs/private paths;
- no unlicensed source assets;
- no duplicate shared infrastructure DLLs.

## Test from the package

Install from the staged/final package, not from the development output directory.

Then verify the exact claims you intend to publish.

## Rollback

For changes that affect configuration, saves or durable content, state whether rollback is:

- safe;
- safe only before save;
- migration-required;
- unsupported.

Do not leave rollback behavior implicit.

See [Publishing checklist](../../reference/modder-resources/publishing-checklist.md).
