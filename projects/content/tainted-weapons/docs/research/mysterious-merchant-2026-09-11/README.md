# Mysterious Merchant report intake — 2026-09-11

**State: research intake published in [PR #404](https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/pull/404); formal research review pending.** This is intake/source comparison of a returned report, not formal RH1/RH2 completion, independent approval, implementation permission or a new static/runtime gate. The report remains E1 research context.

The owner's supplied report contains substantive first-party source findings. It narrows the candidate to a dedicated native Location with ShopAttachment and a registered ShopTemplate, opened through Shop.OpenShop. Its proposed persistent owner is still a hypothesis requiring exact installed-build and save/lifecycle proof.

## Preserved artifacts

- [Original report text](original-report.md): unchanged wording and citation tokens, with CRLF normalized to LF by the repository text policy. The exact 44,576 original bytes are preserved as Base64 in `intake.json` at `source.raw_bytes_base64`; raw SHA-256 `c2e2749f47eb318858c39353e11703f298c438d48e1f21212ccc282b949bf021`.
- [Cleaned derivative](report.md): same report narrative with a labelled intake preface and normalized citations. It is explicitly not the untouched original.
- [Intake metadata](intake.json): source locator/hash, transformation, original citation occurrences, unresolved citations, review limits and publication state.
- [Source ledger](sources.json): 30 first-party source files opened at `AR-Questline/merlin-workshop` commit `073bdab3e09d6adad5003339fc49b021738d71e6`, with Git blob identities and narrow symbol/line locators.
- Original brief context: `mods/tainted-weapons/docs/mysterious-merchant-research-brief-2026-09-11.md` exists in the original local checkout only. It is not included in this six-file PR or the GitHub main base. Its source leads and preservation requirements remain separate from the published toolkit evidence.

All thirty public code files are one first-party source group. Source replacements are newly matched to claims and inspected symbols; the report's original conversation citation payloads were not supplied. Fourteen original citation IDs remain unresolved. Some have corroborating information elsewhere in the ledger, but their exact original references are not reconstructed. No global absence claim about third-party implementations was independently established.

## Preliminary claim comparison

These are intake findings limited to the named public snapshot. The source IDs link through `sources.json`; they do not assert parity with the installed game.

| Claim group | Inspected support | Intake finding / limit |
| --- | --- | --- |
| Location → ShopAttachment → Shop | `ShopAttachment`, `Shop`, `AttachmentContract`, `Location` | Source supports attachment ownership and pre-initialize template assignment. The attachment interface cautions against dynamically added elements. A dedicated runtime merchant is not tested. |
| Native runtime Location and name | `LocationTemplate`, `LocationCreator`, `RuntimeLocationInitializer`, `Location` | Spawn API, generated model ID, saved initializer/template/name and attachment setup exist. One merchant per save is a proposed deduplication policy, not a proven feature. |
| Domain persistence | `Location`, `GameplayUniqueLocation` | Default scene domain and an explicit gameplay-domain relocation mechanism are visible. A hidden merchant's correct cross-scene ownership remains unknown. |
| Template authoring and lookup | `Mapping`, `Authoring`, `TemplatesUtil`, `Loader`, `Provider` | LocationTemplate maps to `Templates.Locations`; ShopTemplate maps specifically to `Templates.Npc`. Authoring supplies labels/GUID-bearing addresses; loader builds maps. No arbitrary runtime GUID is thereby proven restorable. |
| Mod catalog ordering | `Mod`, `ModManager`, `ModService`, `ApplicationScene` | Published source loads content catalogs and registers ModService before TemplatesProvider. Current installed startup timing still requires its own gate. |
| Template save identity | `SaveWriter`, `SaveReader`, `TemplatesUtil` | Saved template GUID is resolved through native template lookup. Missing-template disable/removal behavior is not validated. |
| Stock open/close | `Shop`, `Stock` | Open requests hero storage, opens stock, adds UI and binds discard; close compresses stock/releases storage. Adding Item instances while compressed is rejected. |
| Unique and replenishing stock | `UniqueStock`, `RestockableStock`, `ShopTemplate` | Separate native policies exist. This report does not choose the desired six-weapon stock policy. |
| Transactions and pricing | `TradeUtils`, `Price` | Native affordability, item transfer, seller/buyer wealth changes and price calculation are visible. No actual purchase or price/balance acceptance occurred. |
| Merchant tabs and buyback | `ShopUITabs`, `BoughtFromHeroStock`, `ShopUI` | Public snapshot exposes Buy, Sell, Sell From Stash and Buyback. A buy-only restriction would be a separate change. |
| Bonfire entry and close | `VFireplaceUI`, `FireplaceUI`, `ShopUI` | Native callback/focus/overlay pattern and Back/discard paths exist. Forced bonfire close while a new shop is open still needs explicit coordination and proof. |
| Release identity | GitHub release/tag APIs in `sources.json` | Tag resolves to the stated source commit; API reports 2026-02-06 release and the stated archive digest. Archive bytes were not downloaded or independently hashed. |
| Repository pin | Current local Git HEAD and GitHub commit API | `1a30867675ceef1ddfdb5e86cbb41ffb43ccb3d2` exists locally. GitHub returned 422, `No commit found for SHA`. The public lookup failure does not erase local evidence. |

## Qualifications and remaining questions

1. **Merchant wealth fields are not a finite-budget guarantee.** The inspected `Shop.RestockCurrency` sets currency to `float.MaxValue` and comments out the finite-restock branch. Retain the report's field inventory, but do not infer that setting `maxWealth` alone limits this merchant.
2. **The toolkit is not the installed DLL.** The report and this intake preserve that distinction. No new decompilation, runtime, build, gameplay, save/load or deployment operation occurred during intake.
3. **Runtime Location creation is not evidence for a complete merchant lifetime.** Stable rediscovery, repeated activation, domain changes, hidden owner validity, existing saves and disabling/removing content remain unresolved.
4. **C0 still protects the working weapon binary.** No report finding authorizes rebuilding the incomplete Tainted Weapons source or choosing a separate plugin as an unreviewed workaround. The six weapon packages, IDs, damage, animations, grips, icons and grants are untouched.
5. **Product preferences remain open.** One-time versus replenishing stock, normal merchant selling/buyback, price policy and eventual shop-only acquisition need to be settled within implementation scope. These are product decisions, not evidence that native merchant APIs are absent. No inventory removal or grant change is authorized by this intake.
6. **Report labels are not gate outcomes.** Its `PASSED at E1` wording describes the author's public-source assessment. Formal review, installed-build static proof and runtime transaction/persistence proof are pending. Deep Research execution is recorded as **RETURNED**, with an author-reported **PARTIAL** result; this intake does not rewrite it to not-run or unavailable.

## Process and publication boundary

Controlling sources inspected:

- Root `AGENTS.md`, Source Authority Lock, Repository Mutation and Pull Request Boundary, and Required Workflow.
- `documents/process/research-standard/deep-research-report-intake-standard.md`, sections 2–8: preserve/review/normalize/store/index, then authorized GitHub persistence before follow-on work. Section 6 permits the mod-owned `mods/<mod-id>/docs/research/` destination.
- `documents/process/research-standard/README.md`, Source artifact → claim → reviewed finding hierarchy and mandatory review triggers; `documents/process/research-lifecycle.md`, intake state and non-authority rule. This is initial intake, not relocation or promotion of an authoritative research claim.
- `documents/process/validation/validation-matrix.md`, Level 0 documentation checks and proof-lane separation.
- Original-checkout context: `mods/tainted-weapons/AGENTS.md` and `mods/tainted-weapons/docs/known-good-weapon-process.md`, rules 1–10, preserve the working native weapon pipeline and separate proof. These two local files are absent from the GitHub main base and are not published by this PR.

Only the six report-intake files under this research directory/index are in the publication scope. The owner explicitly approved a separate checkout on `codex/mysterious-merchant-research-intake` from GitHub `main`, committing these six files and opening a PR. The inspected base is `1cc85a1b3cd05839c81c72372b1c8cd12679676f`. Report commit `10560122e449938280ad7c0deb13c2250b204d79` was pushed and [PR #404](https://github.com/theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods/pull/404) opened to `main`; GitHub confirmed exactly six changed files and an open, unmerged PR. This receipt update records that observation; the PR carries its current head. This is research intake, not implementation approval or gameplay readiness.

The original checkout stays on local `main` at `1a30867675ceef1ddfdb5e86cbb41ffb43ccb3d2`. Its 26 staged icon files and other local changes are excluded. The research branch starts from the remote base, so earlier unpublished project commits are not part of this proposed change. No protected, game, package or combat files are changed.

## Local documentation verification

Initial intake checks passed on 2026-09-11: raw attachment preservation, receipt hashes, all 126 original citation occurrences, 30 unique pinned-source IDs/URLs/blob identities/locators, fourteen unresolved citation IDs and the original-checkout relative links. The report narrative matched the original after only the declared citation replacement and line-ending normalization.

Publication preparation retains the raw attachment bytes in Base64 inside the existing receipt because `.gitattributes` normalizes Markdown to LF. Readable text hashes are updated separately. Links to the unpublished local brief and golden rules are now explicitly limited context paths rather than broken GitHub links. Final raw-byte preservation, three readable hashes, 126 citation records, 30 pinned source locators, fourteen unresolved IDs, all ten publication-relative links, all six staged blob identities and the six-path boundary passed. The original checkout index hash remained unchanged. `git diff --cached --check` returned 2 for 26 preserved two-space Markdown hard breaks in the original and derivative; each diagnostic was inspected, with no other whitespace defect found. No native, gameplay, build or deployment checks are part of this documentation-only intake.

## Next researched task

Report publication is complete; formal promotion is not. The next technical step requires separately scoped pinned-DLL comparison of attachment-based merchant creation, template restoration, stock/transaction/close semantics and deterministic owner rediscovery. Do not launch another Deep Research topic or implement the shop automatically from this report.
