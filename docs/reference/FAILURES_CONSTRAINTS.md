# Failures and Constraints

> **Reference page.** Use this to understand why the handbook recommends a particular route. Failed attempts are evidence about assumptions, not clutter to hide.

## What this system is

A mature modding process records both:

- successful paths that established a usable mechanism;
- failed paths that exposed an incorrect assumption, bad lifecycle point, ownership mistake, or missing dependency.

The rule is:

> A failure should change the model. If it teaches nothing new, do not keep repeating it.

## Who owns it in FoA

The failure belongs to the subsystem whose invariant was violated.

Examples from working-repo research:

- template lookup before provider readiness;
- hero inventory mutation without a valid hero/inventory owner;
- custom item registration without proven save semantics;
- shop mutation using private/reflected stock internals;
- UI list timing mistakes;
- name-fragment spell classification mistaken for native identity;
- runtime visual shortcuts that bypass the real equipment owner;
- asset load being mistaken for gameplay registration.

## Important identities, types, and methods

Recurring constraint surfaces include:

- `TemplatesProvider.AllLoaded`;
- `Hero.Current` / `HeroItems`;
- private `TemplatesLoader.AddToMap`;
- `RestockableStock` decompression;
- `ShopUI.OnFullyInitialized`;
- `TemplateReference` / template GUIDs;
- native equip/rendering owners;
- save restore by template GUID.

## Where it exists in the lifecycle

Failures often identify a boundary:

~~~text
too early → owner/service not ready
too late → downstream system already cached/snapshotted state
wrong owner → object exists but native system does not know it
wrong identity → lookup/collision/restore fails
wrong proof → visible result is mistaken for a stronger claim
~~~

## How we interact with it

For each important failure, record:

1. attempted route;
2. expected invariant;
3. observed failure;
4. root cause or current best-supported cause;
5. correction;
6. what the corrected test proved;
7. what remains unknown.

## Why this route

The custom-item/shop history demonstrates the value:

- first prove native stock timing read-only;
- introduce one custom clone;
- observe a real custom item in shop UI;
- broaden too quickly and encounter merchant/list problems;
- reduce back to one proven descriptor;
- inspect/decompile the UI lifecycle;
- move insertion before the item-list snapshot;
- only then broaden again.

That sequence explains the final rule better than a recipe alone.

## What goes wrong

Failure documentation becomes useless when it:

- states guesses as root causes;
- erases superseded diagnoses;
- reports only "didn't work";
- mixes several simultaneous changes;
- treats a source-level correction as live proof;
- copies private paths/assets into public evidence.

## How to verify

A failure lesson is reusable when another person can answer:

- what assumption was false?
- what exact boundary did the failure reveal?
- what correction was made?
- what new evidence supported the correction?
- which future mistakes does this rule prevent?

## Current proof boundary

This public handbook summarizes reusable failure lessons without publishing private implementation details, proprietary assets, saves, or machine-specific diagnostics.

See [Research Method](RESEARCH_METHOD.md) for how failures become process rules.
