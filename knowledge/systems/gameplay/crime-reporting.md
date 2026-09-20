---
document_type: system
scope: native witnessed-crime reporting and bounty application
runtime: mono
evidence:
  static: CURRENT_BINARY_DECOMPILATION
  runtime: NOT_RUN_FOR_THIS_STATIC_PACKET
last_verified: 2026-09-20
source_artifact_sha256: 749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982
---

# Crime Reporting and Bounty

Use this page when your mod needs to understand **how a witnessed crime becomes a bounty**.

The inspected Mono build does not expose a rich native incident/report/case object before bounty.

Instead, deferred witnessed crimes use one Hero-owned `TemporaryBounty` as a shared pending batch.

## Native flow

~~~text
CrimeUtils.TryCommitCrime
→ evaluate owner/jurisdiction
→ CrimeUtils.InformWatchingNPCs

no relevant witness
→ no noticed legal application for that owner

guard witness / InstantReport
→ witness ReactToCrime
→ CrimeUtils.CommitCrime(ref crime, exactOwner)
→ CrimeUtils.AddBounty(exactOwner)

ordinary witnesses
→ TemporaryBounty.GetOrCreate
→ TemporaryBounty.RegisterCrime
→ witness reactions
→ shared pending batch
→ later ApplyCrimes
→ replay with InstantReport / watcher bypass
→ CrimeUtils.CommitCrime
→ CrimeUtils.AddBounty
~~~

## TemporaryBounty is a shared batch

The inspected `TemporaryBounty` has:

- one shared timer;
- pending crime collections.

It does not expose separate native IDs for:

- incident;
- report;
- authority case;
- delivery receipt.

A later crime can therefore share the same pending expiry window as an earlier one.

Do not invent per-incident native semantics that the inspected implementation does not expose.

## Known ways pending crime is flushed

Static evidence identified:

- timer expiry;
- qualifying guard watcher arrival;
- penalty/payment preparation.

`GuardApplyCrimes` applies the shared pending batch and does not take an incident/report/owner argument.

## What FoA already owns

Keep native ownership for:

- crime entry;
- jurisdiction/owner evaluation;
- witness reactions;
- deferred pending crimes;
- owner-specific `CrimeUtils.AddBounty`;
- bounty storage/clearing;
- guard search/intervention states.

A mod may maintain richer semantic records for its own features, but those should wrap the native legal state rather than become a second bounty truth.

## How to verify a crime feature

Check:

1. exact crime type;
2. exact owner/jurisdiction;
3. witnesses present;
4. guard vs non-guard path;
5. immediate vs deferred reporting;
6. pending `TemporaryBounty` state where relevant;
7. flush condition;
8. exact owner receiving bounty;
9. bounty amount/state;
10. guard/search consequences;
11. no double application.

## Evidence limits

This page is based on current Mono decompilation for the inspected build.

Runtime validation was not performed for this static packet.
