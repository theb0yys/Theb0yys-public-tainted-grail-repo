# Armour Importer Pipeline

The Tainted Armour project is the standalone armour-importer framework. Its public value is the **pipeline shape**: it separates geometry intake, deformation analysis, Kandra package work, registration, and later item/equip integration instead of treating "the mesh loaded" as success.

## Current status

The typed importer core, Unity geometry adapter, deformation-validation stage, loose Kandra writer, package validation, registration preflight/candidate stages, guarded registration stages, same-mesh A/B tooling, compatibility analysis, and candidate full-section package generation all exist as bounded pieces.

**Production armour conversion is not yet an established end-to-end path.** Candidate-map application, production Kandra conversion, target-armour runtime registration, item/equip integration, save behaviour, and release readiness remain separate later gates.

## Pipeline

~~~text
Unity/skinned source
→ geometry snapshot
→ ArmorImporter.Import
→ source + target contracts
→ deformation validation
→ optional live visual observation
→ packed Kandra section generation
→ loose .mdkandra/.ixkandra writer
→ package validation
→ registration preflight
→ registration candidate
→ registration dry run
→ explicit host invocation
→ registration proof
→ target-package plan
→ visual/decode comparison
→ production conversion decision
→ item/equip/save integration
~~~

## 1. Capture source geometry

The importer-owned Unity adapter bakes the source renderer and records the geometry required by the importer:

- positions and submesh topology;
- rig/bind-pose information;
- bone weights and influences;
- material-slot and mesh identity;
- deterministic fingerprints.

The adapter should capture and clean up temporary Unity objects. It should not mutate the source asset or silently perform downstream conversion.

## 2. Enter through one typed importer

The core entry point is conceptually:

~~~text
ArmorImportRequest
→ ArmorImporter.Import(...)
→ ArmorImportResult
~~~

The request identifies the source and intended target. The result carries the deformation result and explicit blockers.

Do not build a second editor-only deformation implementation that disagrees with the production importer.

## 3. Validate deformation before conversion

The deformation stage compares controlled baseline and posed geometry. It measures displacement, collapsed triangles, orientation reversals, and aggregate blockers.

This stage answers:

> Is this source/target geometry relationship acceptable enough to continue investigating?

It does **not** answer:

> Can this armour be registered, equipped, saved, or released?

If deformation is blocked, stop there.

## 4. Keep runtime observation separate

An optional live-observation request can add evidence from the running game to the normal importer result.

Use it to compare the source/target assumptions against real FoA body/equipment state. It is an observation stage, not permission to convert or register.

## 5. Write Kandra packages only from known packed data

The Kandra writer emits loose:

~~~text
modDirectory/
  Kandra/
    <Name>.mdkandra
    <Name>.ixkandra
~~~

The writer serializes already-packed Kandra sections in the recovered runtime order.

It is **not** the semantic Unity-to-Kandra converter. Writing files successfully does not prove that the packed vertex/index data is correct.

## 6. Validate the package before registration

Package validation reads the loose files back and checks:

- expected paths;
- file presence;
- byte counts;
- hashes;
- writer result identity;
- required runtime-observation facts.

No registration call should happen here.

## 7. Preflight the registration metadata

Before calling the game, compare the intended Kandra mesh metadata with the validated package:

- mod directory;
- mesh name;
- vertex/index counts;
- bind-pose count;
- blend-shape count;
- payload-layout version.

Build a typed registration candidate only after that preflight passes.

## 8. Separate dry run from invocation

The registration dry run verifies the exact runtime contract and assembly fingerprint for the Kandra registration route.

Only a later explicit invocation stage may call the host-supplied registration bridge, and only when the dry-run identity, package hashes, runtime method fingerprint, and approval all match.

That separation prevents "we found a Register method" from becoming accidental game mutation.

## 9. Require a host proof before target registration

A host proof demonstrates that the guarded host path can register a controlled proof package and read back the expected registration/memory state.

A proof package is not the target armour.

The target armour still needs its own validated package, matching registration plan, and later live invocation.

## 10. Resolve unknown packed fields experimentally

The current armour work includes a same-mesh A/B route for disputed packed Kandra fields. The important pattern is reusable:

1. preserve a known-good mesh;
2. change one disputed field only;
3. keep every other byte identical;
4. register each controlled variant;
5. collect runtime and visual evidence;
6. select an encoder only when one candidate is actually supported.

Do not infer an encoder from "it registered".

## 11. Generate a full candidate package only after the codec decision

Full-section package generation belongs after the required source channels and packed-field decisions are established.

A generated candidate can feed the same package-validation and registration-preflight stages. It still does not automatically become production armour.

## Current hard boundary

Today, the safe public lesson is:

**capture → analyse → package → validate → preflight → guarded proof**

not:

**drop any skinned mesh in and get working armour**.

Production conversion still depends on the remaining semantic codec/stream mapping, visual evidence, target registration, native item/equip integration, lifecycle, persistence, and release validation.
