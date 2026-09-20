# Licensing Reference for Modders

> Practical summary only. The licence text attached to the exact asset or code is controlling.

## Licence triage

Before an external file enters a public mod repository, record:

1. the exact item and creator;
2. the exact licence name/version or provider terms;
3. the source URL;
4. the date obtained or verified;
5. whether modification is permitted;
6. whether commercial use is permitted;
7. whether attribution or notices are required;
8. whether raw/source redistribution is permitted;
9. whether share-alike or source-code obligations apply;
10. whether trademarks, publicity/privacy rights, or other third-party rights may still matter.

If any of those are unknown, treat the item as **not redistributable** until resolved.

## Content licences

| Licence / condition | What it generally means | Public mod-repo posture |
| --- | --- | --- |
| CC0-1.0 | Copyright and related rights are waived/dedicated as far as legally possible; copying, modification and distribution are permitted, including commercially. | Strong default for redistributable art/audio. Still record provenance. |
| CC BY 4.0 | Use, modify and redistribute, including commercially, with attribution, licence link and indication of changes. | Usually usable if attribution is carried with the release/source. |
| CC BY-SA 4.0 | CC BY obligations plus adaptations must be shared under the same or a compatible licence. | Usable only if the share-alike obligation fits the asset/release structure. |
| CC BY-NC 4.0 | Attribution plus use restricted to noncommercial purposes. | Do not assume a free mod automatically qualifies as noncommercial. Case-specific review required. |
| CC BY-ND 4.0 | Redistribution of the unadapted work is permitted with attribution, but adaptations cannot be distributed. | Poor fit for pipelines that resize, retarget, edit, bake or otherwise transform assets. |
| CC BY-NC-SA / CC BY-NC-ND | Combines the listed restrictions. | High-friction; review before use. |
| Public Domain Mark | Marks a work believed to be free of known copyright restrictions; it is not the same mechanism as CC0. | Verify the underlying status and source before relying on it. |
| Custom marketplace/EULA | Rights come from the provider contract, not an open licence. | Often usable inside a finished project but not redistributable as raw/source assets. |
| No licence / “all rights reserved” | No public permission to copy, modify or redistribute beyond applicable law. | Do not redistribute without explicit permission. |

### Creative Commons abbreviations

- BY — attribution.
- SA — ShareAlike.
- NC — NonCommercial.
- ND — NoDerivatives.
- CC0 — public-domain dedication/waiver tool, not an attribution licence.

A “free download” button does not add any of those permissions.

## Common software licences

Use SPDX identifiers in dependency records where possible.

| Licence | Practical summary for mod authors |
| --- | --- |
| MIT | Permissive. Redistribution is allowed; preserve the copyright and licence notice. |
| BSD-2-Clause / BSD-3-Clause | Permissive. Preserve notices; BSD-3-Clause also includes a non-endorsement condition. |
| Apache-2.0 | Permissive with copyright/licence/NOTICE obligations where applicable, change notices, and an express patent licence. |
| MPL-2.0 | File-level copyleft. MPL-covered modified files remain MPL-covered, while separate files can generally remain under other licences. |
| LGPL-2.1 / LGPL-3.0 | Library-focused copyleft. Distribution, modification and relinking obligations can depend on how the library is used. Review the exact version and integration method. |
| GPL-2.0 / GPL-3.0 | Strong copyleft for covered derivative/combined software. Plugin, linking and distribution boundaries can be legally significant; do not guess. |
| OFL-1.1 | Open font licence. Common for fonts; embedding/use is broad, while redistribution, modification and Reserved Font Name rules need to be followed. |

Do not treat a dependency's open-source licence as permission to copy unrelated game code, game data or third-party assets.

## Marketplace licences are different

### Fab

Fab's Standard License allows use and modification in projects and commercial distribution of projects with Fab assets incorporated, but does not allow reselling or redistributing the asset on a standalone basis. Personal and Professional pricing tiers grant the same scope of Standard-License rights; Reference-Only is different because it does not provide source-format content.

For a public source repository, the safe default is to publish configuration, scripts, placeholders and acquisition instructions rather than the raw Fab asset.

### Unity Asset Store

Unity's standard Asset Store EULA permits qualifying non-Restricted assets to be incorporated into a larger Licensed Product, subject to the EULA. It does not create a general right to republish Asset Store packages or their source files. Some listings use Restricted Asset Terms or separate provider terms.

Check the exact listing before use.

## A practical decision tree

Use an asset in the public repository only when all applicable answers below are “yes”:

1. Do we know who supplied it and where it came from?
2. Do we have a licence/permission covering our actual use?
3. Does that permission allow modification if our pipeline modifies it?
4. Does it allow redistribution in the form we plan to publish?
5. Can we satisfy attribution/share-alike/source/notice requirements?
6. Are there no unresolved trademark, privacy, publicity or third-party-right issues?
7. Have we saved the licence/provenance record?

If the answer to #4 is “no, project use only”, keep the source asset out of the repository and document how a user obtains it legally.

See [asset provenance](asset-provenance.md) for the record format and [source evidence](../../sources/licensing-and-asset-sources.md) for official references.
