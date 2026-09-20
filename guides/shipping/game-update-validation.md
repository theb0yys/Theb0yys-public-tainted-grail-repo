# Validate Mods After a Game Update

Treat a FoA update as a change in the evidence base, not as proof that every mod broke.

The maintenance workflow moves from cheap identity checks to targeted runtime checks:

~~~text
record previous build identity
→ identify the new build
→ refresh local references/decompilation
→ compare important binary fingerprints
→ export Harmony symbol anchors from source
→ verify anchors against the new local assemblies
→ inspect changed or missing owners/signatures
→ rebuild affected mods
→ inspect the live Harmony patch table
→ trace selected target invocation where needed
→ test the feature
→ test persistence only when the feature owns persistent state
→ update the documented build/version scope
~~~

## 1. Keep a build fingerprint

Before or after a known-good build, record the important local game/loader identities with the repository's fingerprint tooling.

A fingerprint change means the inspected artifacts changed. It does not identify which behavior changed.

## 2. Refresh local inspection material

Regenerate the ignored local reference/decompilation workspace from the updated installation.

Follow [Local game investigation](../../research/methods/local-game-investigation.md).

Do not compare against a stale decompilation tree and describe the result as current-build evidence.

## 3. Export source-declared Harmony anchors

Generate a fresh manifest from the mod source:

    .\research\tools\symbol-anchors\Export-FoASymbolAnchors.ps1 -Root "." -OutputPath ".\.local-research\symbol-anchors.json"

Review the Unresolved collection. Dynamic target construction must be inspected deliberately; the extractor does not invent a target.

## 4. Verify anchors against the updated Mono assemblies

For a Mono installation:

    .\research\tools\symbol-anchors\Test-FoASymbolAnchors.ps1 -Manifest ".\.local-research\symbol-anchors.json" -AssemblyRoot "<GameRoot>\Fall of Avalon_Data\Managed" -OutputPath ".\.local-research\symbol-anchor-report.json"

Start with:

- missing types;
- ambiguous type matches;
- missing members;
- missing exact signatures.

A surviving symbol proves identity presence only. Reinspect semantics if the owning assembly changed or the feature is high-risk.

## 5. Re-establish the owner when something moved

Do not replace a missing target with a similar-looking method by name alone.

Return to:

- [Finding the native owner](../../research/methods/finding-native-owner.md);
- [Tracing a lifecycle](../../research/methods/tracing-lifecycles.md);
- [Static/source evidence vs runtime evidence](../../research/methods/static-vs-runtime-evidence.md).

Confirm the new owner, lifecycle transition and downstream consumer before changing the patch.

## 6. Rebuild the affected projects

Build against the updated local references.

Treat compiler errors and missing references as evidence about project/reference compatibility, not runtime behavior.

## 7. Inspect actual Harmony installation

Load the mod and the [Harmony runtime audit](../../research/tools/harmony-runtime-audit/README.md).

Filter by the mod's Harmony owner ID and, when useful, provide expected canonical targets.

Compare:

~~~text
source-declared anchor
→ target still present in updated assembly
→ expected Harmony owner present on live target
~~~

These are three separate facts.

## 8. Observe invocation when execution is the remaining question

Use the [runtime tracer](../../research/tools/runtime-tracer/README.md) on one exact, verified target.

Prefer a bounded interaction that can be triggered manually. Avoid broad update-loop tracing.

Patch installation is not target invocation. Target invocation is not downstream feature correctness.

## 9. Test the user-visible feature

Exercise the smallest scenario that proves the intended behavior.

Record:

- game/build identity;
- mod version/build;
- relevant configuration;
- exact interaction;
- expected behavior;
- observed behavior;
- any known compatibility constraints.

## 10. Test persistence only where relevant

If the feature writes or depends on persistent state, test the native save/load lifecycle separately.

A runtime interaction that works before saving says nothing by itself about restoration after reload.

## 11. Update version scope

Update the owning guide, reference page or release note with the build scope actually inspected.

Do not silently carry an old version claim forward merely because compilation succeeded.

## Failure routing

~~~text
fingerprint changed
→ inspect affected artifacts

type/member missing
→ static owner investigation

patch absent from live table
→ target resolution / patch installation investigation

patch installed but no invocation
→ lifecycle / trigger investigation

invocation observed but behavior wrong
→ downstream consumer / semantics investigation

runtime behavior correct but reload wrong
→ persistence investigation
~~~

The workflow is intentionally layered so one kind of evidence cannot substitute for another.
