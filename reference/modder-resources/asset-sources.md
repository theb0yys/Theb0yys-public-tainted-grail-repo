# Asset Sources

This catalogue separates **finding an asset** from **having the right to redistribute it**.

Status terms:

- **Repo-friendly** — source assets can often be committed when the exact item carries the stated open licence.
- **Per-item** — licence varies; inspect every item.
- **Project-use** — useful in a mod/project, but do not assume the source asset can be committed or repackaged.

## Models, textures, HDRIs and UI

| Source | Useful for | Licence posture | Public-repo guidance |
| --- | --- | --- | --- |
| Poly Haven | HDRIs, textures, 3D models | All Poly Haven assets are published as CC0 | **Repo-friendly.** Record the asset URL and download date even though attribution is not required. |
| Kenney | 2D/3D game assets, UI, icons, prototypes | Asset-page game assets are CC0 | **Repo-friendly.** Keep the included licence/provenance record. |
| OpenGameArt | 2D, 3D, textures, music, SFX | Multiple licences, including CC and OGA licences | **Per-item.** Filter/check the exact asset and attribution requirements. |
| Sketchfab | Downloadable 3D models | Creative Commons and other licence types vary by model; marketplace/editorial terms differ | **Per-item/project-use.** Do not infer rights from “downloadable”. |
| Quaternius | Stylised 3D models, animations and game packs | Current general licence page is Quaternius Asset License (QAL) v1.0 | **Project-use by default.** Current QAL permits project use but not reselling/redistributing the assets themselves as assets. Check the exact download and retained licence. |
| Fab | 3D, materials, audio, VFX, tools and more | Fab Standard License, CC-BY on some listings, and other listing-specific terms | **Project-use/per-item.** Standard-Licence assets are not standalone-redistributable. |
| Unity Asset Store | Unity packages, editor tools, models, audio, shaders | Standard EULA, Restricted Asset Terms, or provider-specific terms | **Project-use/per-item.** Never assume a free Asset Store package can be mirrored into a public repo. |

## Audio

| Source | Useful for | Licence posture | Public-repo guidance |
| --- | --- | --- | --- |
| Freesound | SFX, ambience, recordings | Files can be CC0, CC BY or CC BY-NC; check each sound | **Per-item.** Prefer CC0/CC BY for broadly redistributable mod projects. |
| OpenGameArt | Music and SFX as well as visual assets | Varies by item | **Per-item.** Preserve attribution/licence records. |
| Pixabay | Music, SFX, images and video | Pixabay Content License | **Project-use.** Modified/project use is allowed subject to terms, but standalone distribution is prohibited. Do not mirror raw content. |
| Openverse | Search across openly licensed/public-domain image and audio sources | Aggregator; rights belong to each underlying work | **Discovery only.** Verify the original source and licence before use. |

## Fonts and icons

| Source | Useful for | Licence posture | Public-repo guidance |
| --- | --- | --- | --- |
| Google Fonts | UI/display/body fonts | Fonts are released under open-source licences; exact licence is shown per family | **Usually repo-friendly with conditions.** Retain the font licence and obey OFL/Apache terms as applicable. |
| Kenney | UI icons, input prompts, interface packs | CC0 on asset pages | **Repo-friendly.** Useful for prototypes and generic UI. |
| Openverse | Images/icons from multiple collections | Per-item open licence/public-domain marking | **Per-item.** Verify at the original source. |

## Animation and rigging

| Source/service | Useful for | Licence posture | Public-repo guidance |
| --- | --- | --- | --- |
| Adobe Mixamo | Humanoid auto-rigging, characters and animation library | Adobe states characters/animations can be used royalty-free in personal, commercial and non-profit projects; service-specific terms also apply | **Project-use.** Do not treat Mixamo output as CC0 or a stock-asset redistribution source. |
| Quaternius | Pre-made game animation/model packs | QAL v1.0 on the current general licence page | **Project-use.** Keep raw source packs out of a public asset library unless the exact asset grants broader rights. |

## General marketplaces and creator stores

### itch.io assets

itch.io is a hosting/distribution platform, not one blanket asset licence. Creators retain ownership and can attach their own terms. Read the creator's page, included licence and download documentation for each pack.

Treat “free” or “name your own price” only as a price statement.

### Fab and Unity Asset Store

These are strong places to obtain production assets and tooling, but they are not public-domain libraries.

For this repository:

- link to the listing;
- record the exact licence/terms;
- keep source marketplace packages out of git unless explicit redistribution rights exist;
- use placeholders or documented local paths where practical;
- make release packaging respect the marketplace EULA.

## Recommended sourcing order for a public mod repo

When several equivalent assets exist, this order usually reduces redistribution friction:

1. self-created and explicitly publishable;
2. CC0/public domain;
3. CC BY with clean attribution;
4. other compatible open licences;
5. custom/project-use marketplace licences with placeholders and acquisition instructions;
6. unknown/no licence — do not use.

This order is about publication friction, not artistic quality.

Official source URLs and the date this catalogue was checked are recorded in [licensing and asset-source evidence](../../sources/licensing-and-asset-sources.md).
