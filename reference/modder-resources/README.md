# Modder Resources

Use this section when you need to answer one of these questions:

- Can I legally use or redistribute this asset or code?
- Where can I find assets suitable for a public mod project?
- Which external tools are useful for Unity/Tainted Grail modding?
- What provenance should I record before an asset enters a project?
- What should I check before committing or publishing a mod?

This is a practical reference, not legal advice. Always read the exact licence or terms attached to the exact item you obtained.

## Fast rule

**No known licence = no redistribution.**

Also keep these separate:

- free of charge != public domain;
- allowed in a game/mod != allowed to publish the raw source asset;
- owning a marketplace licence != owning copyright;
- inspection/extraction tooling != permission to redistribute extracted content.

## Start here

| Need | Reference |
| --- | --- |
| Decode common content and software licences | [Licensing](licensing.md) |
| Find models, textures, audio, fonts, UI and animation resources | [Asset sources](asset-sources.md) |
| Find creation, inspection and modding tools | [Tools and services](tools-and-services.md) |
| Record where an asset came from and why it is publishable | [Asset provenance](asset-provenance.md) |
| Check a repository/release before publishing | [Publishing checklist](publishing-checklist.md) |

## Tainted Grail-specific routes

Do not duplicate game-specific runtime guidance here.

- Asset transport, bundles and Addressables: [Assets, Addressables, and Presentation](../assets/README.md)
- Shared mod ecosystem tooling: [Tooling and Shared Infrastructure](../../tooling/README.md)
- Starter projects: [Project Templates](../../templates/README.md)
- First mod path: [Make your first mod](../../learn/first-mod/README.md)
- Public evidence rules: [Public Evidence Standard](../../sources/evidence-standard.md)

## Public-repository posture

For this repository, prefer one of these routes:

1. self-created assets with explicit publication rights;
2. CC0/public-domain assets with recorded provenance;
3. permissively licensed assets whose attribution/notice requirements can be satisfied cleanly;
4. placeholders plus acquisition instructions for marketplace or otherwise non-redistributable assets.

Do not commit proprietary game content, paid marketplace source files, game DLLs, Unity DLLs, generated interop assemblies, saves, or extracted commercial assets.

Source URLs and verification notes for this section are tracked in [licensing and asset-source evidence](../../sources/licensing-and-asset-sources.md).
