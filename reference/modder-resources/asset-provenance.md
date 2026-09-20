# Asset Provenance Record

Every externally sourced asset should have enough metadata that another maintainer can answer:

- where did it come from?
- which licence/terms applied when we obtained it?
- what did we change?
- may the source asset be committed?
- may the resulting mod/release contain it?
- what attribution or notices must ship?

## Copyable record

Use one record per externally sourced asset or tightly defined asset pack.

    Asset:
    Local path / identifier:
    Creator / publisher:
    Original source URL:
    Listing / asset ID:
    Date obtained:
    Date licence verified:
    Licence name:
    Licence version:
    Licence / terms URL:
    Original licence file retained: yes/no
    Price at acquisition: free/paid/unknown
    Commercial use permitted: yes/no/unclear
    Modification permitted: yes/no/unclear
    Raw/source redistribution permitted: yes/no/unclear
    Redistribution inside a finished project permitted: yes/no/unclear
    Attribution required: yes/no
    Share-alike / source obligation:
    Other restrictions:
    Trademark / likeness / privacy concerns:
    Modifications made:
    Attribution text to ship:
    Include source asset in public repo: yes/no
    Include derived asset in public repo: yes/no
    Include in release package: yes/no
    Decision rationale:
    Reviewed by:
    Review date:

## Decision vocabulary

Prefer explicit values over vague notes:

- YES — permission is established for the stated action.
- NO — terms prohibit the stated action.
- CONDITIONAL — permitted only when recorded obligations are satisfied.
- UNCLEAR — evidence is insufficient; fail closed and do not redistribute.

## Recommended repository pattern

For assets that cannot be committed but can be used locally:

    assets/
      README.md              # acquisition/import instructions
      .gitkeep               # optional
    docs/
      attribution.md         # required notices/credits
    provenance/
      example-asset.md       # source/terms record

Do not add the raw file and rely on .gitignore after the fact. Confirm the public tree before push/release.

## What to retain

Where permitted, retain a local/private copy of:

- the licence text that shipped with the asset;
- listing URL and asset ID;
- creator/publisher name;
- purchase/download record if relevant;
- version/date;
- required attribution wording;
- any explicit permission correspondence.

Do not place private receipts, account identifiers or personal information in the public repository.

## Why date/version matter

Marketplace and creator terms can change. A current web page may not describe the exact terms under which an older asset was obtained, and old community posts may describe a licence that is no longer current.

Record the licence that actually applied to your copy, not only the licence you remember.

See [Licensing](licensing.md) and [Asset sources](asset-sources.md).
