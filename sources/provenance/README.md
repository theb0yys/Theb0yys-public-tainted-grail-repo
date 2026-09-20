# Private-to-Public Publication Ledger

This ledger tracks **knowledge extraction**, not file copying.

- [Example extraction map](example-extraction-map.md)

## Waves 1–14

Covered PR #407, reusable indexes, major gameplay/presentation/tooling domains, shared frameworks/SDK, actor/companion ownership, save/travel, stats/magic/HUD, combat/storage/lockpicking/dialogue.

## Wave 15 — 2026-09-20

Mined Tainted BugFixes, focused quest-fix research, official Merlin quest evidence, and Missing Pebbles:

- rule-pack-first quest recovery with mutation disabled until exact evidence/backup/validation gates close;
- current-build reproduction requirement before maintaining historical quest fixes;
- official Merlin objective-marker authoring contract and NPC-marker/objective-marker separation;
- Stink and Burn static identity case with unresolved objective/placed-marker boundary;
- existing-item insertion into save-backed `SearchAction` rows;
- existing-item grant from hero Pickaxe/mining damage context;
- additional evidence-scoped candidate identities.

## Wave 16 — 2026-09-20

Extracted public-safe starter structure from the private mod-template family and reconciled it with existing public examples and the official Merlin project boundary:

- reusable Mono Harmony lifecycle wiring with a self-contained, non-game patch target;
- reusable small runtime-UI starter with explicit native-UI ownership limits;
- Merlin content-overlay structure that keeps the official Workshop project authoritative;
- hybrid Merlin + BepInEx layout with separate authoring/runtime lifecycles and an explicit identity contract;
- no private production implementation source was published.

Newly authored public starters retain their own validation state; private success does not make them runtime-proven.

## Wave 17 — 2026-09-20

Reorganized the public example surface around runtime/authoring roots and functional domains:

- `examples/` now exposes only `mono/`, `il2cpp/`, `merlin/`, and `hybrid/` as directories;
- Mono examples moved from a flat list into audio, combat, Harmony, items, magic, rendering, and UI domains;
- the IL2CPP first-change example moved under `il2cpp/basics/`;
- numeric ordering and redundant runtime/domain words were removed from example paths;
- project filenames were normalized to concise mechanism names;
- the private corpus was mapped into extraction domains without publishing private production implementation source.

## Wave 18 — 2026-09-20

Expanded `templates/` from six starter families into a reusable public template library:

- normalized Mono domains for Harmony, audio, combat, items, magic, rendering and UI;
- added copy-ready Mono starters derived from public-safe mechanism extractions;
- added IL2CPP Harmony, audio, UI and frame-sampling starters using the thin `BasePlugin` + registered-behaviour patterns seen in released IL2CPP mods;
- added a Tainted Framework dual-runtime consumer starter with one shared feature source and separate Mono/IL2CPP hosts;
- added `templates/SOURCE-MAP.md` to make the private-source-family-to-public-template relationship explicit without copying private production implementation wholesale.

The new templates remain starting points. Their source structure does not inherit private runtime, compatibility, persistence or release proof.

## Measurement rule

A knowledge unit counts only when claims are extracted from inspectable evidence, owner/lifecycle/reasoning are reconstructed, clean-room public prose is written, evidence limits are stated and the page is linked into canonical navigation.
