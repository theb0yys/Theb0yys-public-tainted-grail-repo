# Armour: Source Geometry, Deformation, Kandra, Native Clothes, and Equip Process

> **Reference/process page.** Armour is the most layered of the four documented content domains. The private working evidence contains a substantial production importer, deformation pipeline, Kandra package writer/validator, guarded runtime registration path, and native clothes/Kandra lifecycle research. Those lanes must remain separate. A Kandra package registering successfully does **not** mean a custom armour item has been equipped successfully.

## Document status

**Domain:** armour / wearables / Kandra / equipment presentation  
**Public conclusion:** **PARTIAL PATH PROVEN**  
**Importer/deformation framework:** implemented and exercised on real sample geometry  
**Kandra loose-package writer/validation:** implemented  
**Kandra runtime registration:** guarded host proof exists for a proof mesh  
**Kandra same-mesh A/B/decode research:** implemented as a dedicated evidence lane  
**Native clothes/equip architecture:** current-build static evidence exists  
**End-to-end target custom armour item:** **NOT YET GENERALLY PROVEN**  
**Persistence/uninstall/migration:** not generally proven

The first accepted live Kandra host-registration proof is a proof fixture, not a production armour item.

This distinction is mandatory.

---

# 1. What the armour problem actually contains

A production custom-armour path crosses several different systems:

~~~text
source asset/provenance
→ source geometry capture
→ canonical source contract
→ native target contract
→ source↔target compatibility
→ bone/weight/rig mapping
→ deformation validation
→ Kandra semantic conversion
→ packed Kandra payload
→ loose package write
→ package validation
→ Kandra metadata/preflight
→ runtime registration
→ visual/decode validation
→ custom ItemTemplate/equipment identity
→ native clothes/equip owner
→ ClothStitcher / KandraRig integration
→ body cover/culling/material state
→ unequip/re-equip cleanup
→ scene/load behavior
→ save/load/missing-mod behavior
~~~

These are not one step.

A skinned mesh loading in Unity proves very little about the rest of the chain.

---

# 2. Ownership

## 2.1 Importer owner

The standalone Tainted Armour system owns:

- provider-neutral import contracts;
- geometry capture adapters;
- source/target contracts;
- deformation validation;
- Kandra package-generation stages;
- package validation;
- registration planning/gates;
- evidence receipts;
- compatibility analysis;
- future target conversion.

It is not a feature of another mod.

## 2.2 Native logical item owner

`ItemTemplate` owns the logical armour item identity and item-level behavior.

Custom armour still needs a valid item/equipment definition just as a weapon does.

## 2.3 Native clothing presentation owner

Static evidence identifies `BaseClothes` as the native clothes/equip presentation owner.

Its path uses:

- a target `KandraRig`;
- a clothing asset reference;
- `ClothStitcher`;
- Kandra renderer redirection/stitching;
- native cleanup/release on unequip.

## 2.4 Native Kandra runtime owner

Kandra runtime registration is owned by:

- `KandraRenderer`;
- `KandraRendererManager`;
- `KandraMesh`;
- `KandraRig`;
- streaming/mesh/bone/blendshape/skinning managers;
- `SkinnedBatchRenderGroup`;
- culling/material/mipmap managers.

The importer cannot pretend a byte file alone is “a Kandra renderer.”

---

# 3. The three major evidence lanes

The armour process must keep these lanes separate.

## Lane A — importer/conversion evidence

Questions:

- Can source geometry be captured deterministically?
- Can source and target rigs be described?
- Can compatibility be measured?
- Can deformation be validated?
- Can a Kandra payload be constructed deterministically?

## Lane B — Kandra registration/runtime evidence

Questions:

- Does the package match the runtime reader contract?
- Does the Kandra manager accept the renderer?
- Does deferred finalization complete?
- Does `IsRegistered` pass?
- Does mesh memory become available?
- Does visual decoding look correct?
- Does unregister/cleanup work?

## Lane C — native armour/equip evidence

Questions:

- Does a custom armour `ItemTemplate` exist?
- Does native equipment selection use it?
- Does `BaseClothes` load the clothing asset?
- Does `ClothStitcher` attach it to the intended `KandraRig`?
- Are body-cover/culling/material rules correct?
- Does unequip cleanly release?
- Does save/load rebuild correctly?

Passing one lane does not pass the others.

---

# 4. Canonical importer entry point

The production importer has one typed entry point:

~~~text
ArmorImporter.Import(ArmorImportRequest)
→ ArmorImportResult
~~~

The goal is to keep the core provider-neutral.

The production importer must not require live FoA mutation merely to reason about geometry.

---

# 5. Source geometry capture

The importer-owned Unity adapter captures baked geometry and topology into provider-neutral contracts.

Important principle:

~~~text
capture source geometry
without mutating source
without performing downstream runtime operations
~~~

The geometry capture is evidence input.

It is not Kandra conversion.

---

# 6. Source contract

A source armour contract needs enough information to reason about the original asset.

Examples include:

- source mesh identity;
- renderer identity;
- material slots;
- submesh topology;
- rig;
- bone names/order;
- bind poses;
- bone weights/influence counts;
- geometry counts;
- source fingerprints.

This turns “this FBX looks like armour” into an inspectable source definition.

---

# 7. Native target contract

The target contract identifies the FoA body/armour target.

It may need:

- native target item/template identity;
- target body variant;
- target Kandra rig;
- target renderer;
- target mesh/material identities;
- body-cover/culling behavior;
- bone order;
- bind poses;
- expected runtime owner;
- exact fingerprints.

The target contract is not permission to convert or equip.

---

# 8. Compatibility is not conversion

The compatibility layer compares the source and target contracts.

It should distinguish states such as:

- exact;
- structurally compatible;
- transfer required;
- unresolved;
- fail.

A compatibility result answers:

~~~text
what transformation problem exists?
~~~

It does not perform that transformation.

---

# 9. Deformation validation

Armour must deform correctly on the target rig.

The framework-owned deformation stage evaluates metrics such as:

- calibrated displacement;
- collapsed triangles;
- orientation reversal;
- aggregate thresholds;
- blocker policy.

This is critical because geometry can be:

- structurally loadable;
- package-valid;
- runtime-registrable;

and still deform incorrectly.

---

# 10. Real-sample lesson

A real Dragon Knight iron/no-cape sample was run through the typed production importer.

The production flow reproduced:

~~~text
444 orientation reversals
0 collapses
~~~

across the measured imports, leaving those imports deformation-blocked.

The correct result was **blocked**, not “close enough.”

That is exactly why deformation is a first-class gate.

---

# 11. Candidate-map boundary

Bone/candidate mapping must be treated as data with explicit approval.

The framework preserves candidate-map application as false until the mapping/deformation gates allow it.

Do not silently “best match” bones and proceed.

Unresolved mapping is a blocker.

---

# 12. Bind-pose transfer boundary

Bind-pose transformation policy is another independent decision.

A source may have bones that need:

- exact mapping;
- rest-space rebake;
- transfer coefficients;
- deferred unresolved handling.

Unknown coefficients remain unknown.

The importer must not invent numeric transfers merely to finish a conversion.

---

# 13. Visual runtime observation

The importer can accept an optional visual runtime observation contract.

This lane is intentionally observation-first.

It can capture:

- current FoA body context;
- equipped renderers;
- animation state context;
- visual metrics needed to evaluate a candidate.

It should not perform conversion or item/equip mutation simply because observation data exists.

---

# 14. The Kandra writer is not the converter

The first Kandra writer slice writes already-packed sections.

It produces loose:

~~~text
<Name>.mdkandra
<Name>.ixkandra
~~~

under the recovered mod seam:

~~~text
<mod directory>/Kandra/<Name>
~~~

Important boundary:

~~~text
writer
!= semantic Unity/FBX → Kandra converter
~~~

The writer receives packed payload data.

It does not, by itself, prove that those packed bytes were encoded correctly.

---

# 15. Managed Kandra stream layout

Current managed static evidence proves the Kandra mesh-data section order.

The `.mdkandra` stream is read as:

1. compressed vertices;
2. additional vertex data;
3. packed bone weights;
4. bind poses;
5. blendshape sections, one per declared blendshape.

The `.ixkandra` stream contains index data sized according to mesh metadata.

Exact counts must agree with `KandraMesh` metadata.

A byte-count mismatch is a real contract failure.

---

# 16. KandraMesh runtime metadata

A Kandra package is not self-describing enough to replace runtime metadata.

Relevant `KandraMesh` facts include:

- local bounds;
- bounding sphere;
- submeshes;
- blendshape names;
- vertex count;
- index count;
- bind-pose count;
- UV distribution value;
- mod directory;
- mesh name.

Registration needs metadata and bytes to agree.

---

# 17. Loose-mod Kandra seam

Current static evidence identifies a loose mod path.

When `KandraMesh.modDirectory` is non-empty, streaming resolves approximately:

~~~text
ModManager.ModDirectoryPath
/<modDirectory>
/Kandra
/<mesh.Name>.mdkandra

and

/<mesh.Name>.ixkandra
~~~

This is the key non-archive seam used by the armour framework.

It avoids claiming that native `kandra.arch` mutation is required for proof work.

---

# 18. Package validation stage

After writing the loose package, validation reads it back.

The package gate verifies things such as:

- expected seam/path;
- file presence;
- byte counts;
- hashes;
- writer boundary flags;
- matching metadata;
- associated registration/memory proof when the stage requires it.

Passing package validation means:

~~~text
the produced package matches the expected package contract
~~~

It does not mean:

~~~text
this is valid production armour
~~~

---

# 19. Registration preflight

Before live Kandra registration, compare the selected `KandraMesh` metadata contract with the validated loose package.

Preflight should verify:

- mod directory;
- mesh name;
- vertex count;
- index count;
- bind-pose count;
- blendshape count/names as required;
- payload layout version;
- package hashes;
- expected files.

No registration call occurs at this stage.

---

# 20. Registration candidate

The next stage constructs a typed registration candidate.

It carries:

- Kandra mesh metadata identity;
- package paths;
- counts;
- hashes;
- layout version;
- recovered registration contract identity.

Creating this object is planning.

It is not a live registration.

---

# 21. Runtime registration dry run

The dry-run stage binds the candidate to the recovered native registration contract.

It checks:

- exact package identity;
- exact hashes;
- exact assembly fingerprint;
- exact recovered method fingerprint;
- required native surface;
- no-write boundary.

The dry run explicitly does **not** call:

- `Register`;
- `CanRegister`;
- object creation/activation;
- item/equip mutation;
- save mutation;
- candidate-map application;
- archive mutation.

A passing dry run proves the invocation prerequisites were checked.

---

# 22. Explicit invocation approval

Live Kandra registration is a separate transition.

The invocation stage requires:

- accepted dry run;
- matching explicit approval;
- same package hashes;
- same native contract fingerprint;
- same assembly identity;
- host-supplied invoker.

Only then may the host call the recovered registration route.

The provider-neutral core itself does not pretend to own FoA object construction.

---

# 23. Kandra normal registration entry

Static evidence identifies normal component entry as:

~~~text
KandraRenderer.OnEnable()
→ KandraRendererManager.Register(renderer)
→ rig.RegisterActiveRenderer(renderer)
~~~

But registration is not complete at that call.

---

# 24. Deferred registration finalization

`KandraRendererManager.Register` submits/queues the renderer.

Full registration is deferred to the manager's EarlyUpdate lifecycle.

Conceptually:

~~~text
Register(renderer)
→ slot submitted
→ mesh/index async reads start
→ EarlyUpdate
→ FinalizeRegistration()
→ wait for reads
→ capacity/readiness checks
→ rig registration
→ mesh upload
→ bones
→ blendshapes
→ skinning output
→ BRG/submesh/material registration
→ culling/root data
→ animator registration
→ mipmap material state
→ mark fully registered
~~~

Therefore:

~~~text
Register() returned
!= renderer fully registered
~~~

---

# 25. Registration readiness checks

Finalization uses several native managers.

Examples include:

- rig manager;
- mesh manager;
- bones manager;
- blendshapes manager;
- skinning manager.

If required resources/capacity are unavailable, full registration is deferred/requeued rather than magically complete.

A proof must observe the terminal registered state, not only the call.

---

# 26. Required registration proof markers

The guarded host proof requires at least:

- native `Register(KandraRenderer)` invocation occurred;
- runtime registration succeeded;
- `IsRegistered == true`;
- mesh-memory lookup succeeds;
- forbidden downstream mutations remained false.

That is the minimum current live Kandra registration proof.

---

# 27. First live host proof boundary

The first accepted live host proof uses:

~~~text
AvalonAwakenedProof / RealSkinnedTriangle
~~~

This proves the recovered registration seam can accept a correctly formed proof renderer/package under the guarded host route.

It does **not** prove:

- production armour conversion;
- target armour package correctness;
- native armour item integration;
- equipping;
- deformation;
- persistence.

Do not reuse the proof fixture identity/payload as though it were the target armour.

---

# 28. Target registration planning

A target-package registration planner exists after host proof.

It deliberately rejects reuse of the proof fixture identity/payload.

Its output is a typed target invocation context.

It still does not:

- call the host;
- create/activate FoA target objects;
- apply candidate maps;
- run conversion;
- mutate items/equipment/saves.

Planning is not runtime proof.

---

# 29. Same-mesh A/B proof

One disputed packed field required a dedicated experiment rather than assumption.

The framework builds exactly controlled variants from the same source mesh bytes.

The variants preserve everything except the explicitly tested field.

This is a model for codec research:

~~~text
change one field
keep everything else byte-identical
register every variant through same host route
capture decode/visual evidence
select or reject candidate based on evidence
~~~

Do not “improve” multiple packed fields at once.

---

# 30. Visual decode evidence

Registration success alone cannot validate a packed geometry field.

A package can register and still decode incorrectly.

The same-mesh route therefore adds visual evidence tied to:

- the same test ID;
- the same package hashes;
- the same registration results;
- the exact variant.

A reviewed decision can:

- accept one non-original candidate;
- reject all candidates;
- remain blocked if evidence is ambiguous.

---

# 31. Full-section package generation

Only after codec/field decisions can a fuller package-generation stage create candidate Kandra packages from imported vertex channels.

That stage may generate:

- compressed vertex section;
- additional vertex data;
- bone weights;
- indices;
- bind poses;
- blendshape metadata/sections;
- matching Kandra metadata;
- metadata-to-stream mapping.

Even here, production target armour remains gated by later runtime/equip evidence.

---

# 32. Kandra runtime object contract

A `KandraRenderer` requires coherent renderer data.

Static evidence includes:

- target `KandraRig`;
- `KandraMesh`;
- renderer bone mapping;
- root bone;
- root matrix;
- bounds;
- materials;
- filtering settings;
- blendshape state where applicable.

The package bytes are only one part of the contract.

---

# 33. Kandra rendering pipeline ownership

Full registration fans out into native managers for:

- rig memory;
- mesh buffers;
- index buffers;
- bone mappings;
- blendshape buffers;
- skinning output;
- batch renderer group;
- culling;
- animator visibility;
- mipmap material state.

The final render path uses Unity `BatchRendererGroup`.

This explains why replacing a normal `SkinnedMeshRenderer` assumption is insufficient for Kandra-native content.

---

# 34. Kandra unregister lifecycle

Disable does not immediately destroy every Kandra resource.

Conceptually:

~~~text
KandraRenderer.OnDisable()
→ remove active renderer from rig
→ KandraRendererManager.Unregister()
→ cancel pending registration OR mark fully registered renderer pending-unregister
→ EarlyUpdate FinalizeUnregistration()
→ release rig/mesh/bone/blendshape/skinning/BRG/culling/animator/material state
→ invalidate renderer ID
→ clear manager slot
~~~

A production armour proof must cover cleanup.

---

# 35. Kandra destroy/dispose behavior

Destroy/dispose may also need to remove:

- merged rig state;
- blendshape weight storage;
- material instances;
- original material references;
- manager tracking.

Therefore “disable GameObject” is not automatically complete cleanup.

---

# 36. Native clothes path

Separate static research establishes the FoA clothing/equip path.

The important owner chain is:

~~~text
equipped armour Item
→ ItemEquip/native equipment owner
→ BaseClothes
→ resolve/load clothing GameObject
→ target KandraRig
→ ClothStitcher.Stitch(clothPrefab, KandraRig)
→ KandraRenderer.RedirectToRig / related stitch state
→ body cover/culling/material/VFX rebinding
→ active native clothing presentation
~~~

This is the target integration owner for actual armour.

---

# 37. BaseClothes ownership

`BaseClothes` owns a `KandraRig` and the clothes presentation lifecycle.

The decompiled static route includes:

- load;
- equip/stitch;
- unequip;
- safe unequip;
- instance destruction;
- asset release.

The exact methods include:

- `EquipTask`;
- `Equip`;
- `Unequip`;
- `SafeUnequip`.

This is stronger evidence than treating armour as “a skinned mesh parented to the body.”

---

# 38. ClothStitcher ownership

`ClothStitcher.Stitch(GameObject, KandraRig)` is the native stitching seam.

The Kandra-specific path includes evidence for:

- `KandraRenderer.RedirectToRig`;
- mesh-cover/culling state;
- VFX renderer rebinding;
- armour-feature bone mapping.

A custom armour path that bypasses these contracts may render while breaking body-cover, rigging, VFX or teardown behavior.

---

# 39. Why Kandra registration is not native armour equip

The Kandra registration proof answers:

~~~text
can this Kandra renderer/payload enter the Kandra runtime?
~~~

Native armour equip answers:

~~~text
can a saved/equipped Item cause the native clothes owner to load/stitch/manage this clothing correctly?
~~~

Different questions.

Do not merge them.

---

# 40. Logical item/equipment identity

A target armour package ultimately needs a custom item/equipment identity.

That requires item-domain work:

- custom `ItemTemplate`;
- stable custom GUID;
- correct armour classification;
- correct equip attachment;
- exact slot/equipment type;
- presentation reference;
- acquisition;
- persistence identity.

Kandra success does not automatically create any of those.

---

# 41. Body-cover and culling behavior

Armour may need to hide or cover parts of the base body.

A correct integration must preserve the game's body-cover/culling semantics.

Otherwise users may see:

- body clipping through armour;
- missing body parts;
- incorrect visibility in FPP/TPP;
- culling artifacts;
- exposed duplicate surfaces.

This is a separate visual correctness check from bone deformation.

---

# 42. Materials and shader compatibility

A Kandra mesh can be structurally correct but visually wrong because of:

- incompatible material;
- shader mismatch;
- wrong property layout;
- transparency mode;
- filtering/layer settings;
- texture binding;
- mipmap/streaming behavior.

Registration success is not material correctness.

---

# 43. Bounds and root data

Kandra visibility uses bounds/root data.

Bad bounds can cause:

- disappearing armour;
- overdraw;
- culling at incorrect camera angles;
- shadow/light issues.

A production target must validate its bounds and root transform assumptions.

---

# 44. Rig mapping

Renderer bones map into target rig memory.

The importer therefore needs exact bone mapping.

Problems include:

- missing bones;
- wrong ordinal mapping;
- duplicated source bone;
- target bone with no safe source equivalent;
- influence loss;
- invalid bind pose.

Do not resolve these with name similarity alone.

---

# 45. Deformation failure classes

Armour validation should distinguish at least:

- excessive vertex displacement;
- triangle collapse;
- triangle orientation reversal;
- seam mismatch;
- clipping;
- bone-weight distortion;
- bind-pose error;
- normal/tangent decoding error;
- body-cover mismatch;
- material/shader failure.

These have different causes and fixes.

---

# 46. Why the importer is staged

A monolithic “convert and equip” command would hide where failure occurred.

The staged process allows a result such as:

~~~text
source capture: pass
target contract: pass
compatibility: transfer required
deformation: fail
Kandra writer: not run
registration: not run
equip: not run
~~~

That is more useful than a final generic “import failed.”

---

# 47. Evidence/receipt identity

A production pipeline should bind stage results to:

- source artifact hash;
- canonical source identity;
- target identity;
- geometry fingerprint;
- mapping decision;
- deformation policy;
- Kandra codec/payload version;
- package hashes;
- Kandra assembly/runtime fingerprint;
- registration receipt;
- visual evidence;
- later item/equip identity.

Otherwise a pass from one artifact can be accidentally reused for another.

---

# 48. Source immutability

The importer should not destructively rewrite the original source asset.

Canonical/intermediate outputs should be derived.

This supports:

- reproducibility;
- comparison;
- rollback;
- cache invalidation;
- audit.

---

# 49. Deterministic canonical representation

The Tainted Armour research establishes a canonical intermediate/provenance direction.

Important identities include separate concepts for:

- canonical object identity;
- lineage identity;
- artifact/content identity.

Do not use one overloaded filename as all three.

The public process should preserve that conceptual separation even when implementation details evolve.

---

# 50. Content-addressed output

Deterministic serialized artifacts and hashes allow the pipeline to answer:

- did the same semantic input produce the same output?
- which stage changed?
- which cached output is still valid?
- which downstream evidence is stale?

This is an importer reliability feature, not proof of native correctness by itself.

---

# 51. Cache invalidation

Any material upstream change should invalidate dependent evidence.

Examples:

- source geometry changes;
- target rig changes;
- candidate map changes;
- bind-pose policy changes;
- codec changes;
- Kandra runtime assembly changes;
- package bytes change.

Do not reuse a registration receipt for different bytes.

---

# 52. Native archive boundary

Current proof work uses the loose Kandra mod seam.

It does not require mutating native `kandra.arch`.

That is deliberate.

Native archive mutation is a different, higher-risk operation and should not be smuggled into custom armour proof.

---

# 53. Runtime host boundary

The provider-neutral core should not instantiate proprietary runtime objects casually.

A host adapter/invoker owns the exact FoA call.

This allows:

- static/conversion tests without game runtime;
- explicit runtime permission;
- exact assembly fingerprint checks;
- no-write dry runs;
- bounded live invocation.

---

# 54. Failure history and resulting rules

## Failure/risk: “valid Unity skinned mesh = armour”

**Rule:** native armour includes Kandra runtime and native clothes ownership.

## Failure/risk: “Kandra package wrote successfully = converter correct”

**Rule:** writer output needs package validation, registration and visual/decode evidence.

## Failure/risk: “Register returned = fully registered”

**Rule:** registration finalizes later; prove terminal `IsRegistered` and mesh memory.

## Failure/risk: “proof mesh registered = target armour works”

**Rule:** proof fixtures establish only the tested registration seam.

## Failure/risk: “registration = equip”

**Rule:** Kandra runtime registration and `BaseClothes/ClothStitcher` integration are separate.

## Failure/risk: “mesh deforms mostly okay”

**Rule:** orientation reversals/collapse metrics can hard-block conversion.

## Failure/risk: “name-based bone transfer”

**Rule:** unresolved mapping/transfer remains blocked until evidence resolves it.

## Failure/risk: “one packed-field guess”

**Rule:** use same-mesh A/B with one-field mutation and visual evidence.

## Failure/risk: “visible = cleanup safe”

**Rule:** prove unregister, asset release and native unequip.

---

# 55. Target end-to-end armour process

A full target-armour import should eventually run:

1. Record source provenance/licence.
2. Hash source inputs.
3. Capture baked source geometry.
4. Build canonical source contract.
5. Resolve exact native target body/armour profile.
6. Build target contract.
7. Run source-target compatibility.
8. Resolve exact bone/candidate mapping.
9. Resolve bind-pose transfer policy.
10. Run deformation validation.
11. Stop on collapse/orientation/metric blockers.
12. Produce canonical conversion input.
13. Encode Kandra vertex/additional/bone/bind-pose/index/blendshape data.
14. Build matching `KandraMesh` metadata.
15. Write loose Kandra package.
16. Read package back.
17. Validate hashes/counts/layout.
18. Run registration preflight.
19. Build registration candidate.
20. Run exact-runtime dry run/fingerprint check.
21. Obtain explicit runtime invocation approval.
22. Host constructs exact Kandra renderer contract.
23. Invoke native registration.
24. Wait for deferred finalization.
25. Prove `IsRegistered`.
26. Prove mesh memory.
27. Capture visual/decode evidence.
28. Validate materials/bounds/culling.
29. Register/create custom armour `ItemTemplate`.
30. Create/acquire native `Item`.
31. Equip through native equipment owner.
32. Enter `BaseClothes` lifecycle.
33. Load clothing asset.
34. Stitch to target `KandraRig`.
35. Verify `KandraRenderer.RedirectToRig`/stitch ownership.
36. Verify body cover/culling.
37. Verify materials/VFX.
38. Verify movement/animation deformation.
39. Unequip.
40. Verify native cleanup/release.
41. Re-equip.
42. Verify no duplicate/stale renderer.
43. Test perspective/body variants required by profile.
44. Test scene transition.
45. Save/load unequipped.
46. Save/load equipped.
47. Test missing package.
48. Test disabled mod.
49. Test migration/update.
50. Only then consider a reusable production profile.

Current evidence has **not** completed all fifty stages for arbitrary target armour.

---

# 56. Minimum deformation proof

- source and target identities fixed;
- geometry snapshots bound to hashes;
- evaluated vertex/triangle set explicit;
- metric thresholds explicit;
- displacement result;
- collapse count;
- orientation-reversal count;
- blocker decision;
- no hidden manual override.

A blocked geometry result must stay blocked.

---

# 57. Minimum Kandra package proof

- expected `.mdkandra`;
- expected `.ixkandra`;
- exact file hashes;
- exact byte counts;
- Kandra metadata counts;
- section-order contract;
- package path/seam;
- read-back validation;
- no native archive mutation;
- no implied registration.

---

# 58. Minimum Kandra registration proof

- exact `Awaken.Kandra` fingerprint;
- exact registration contract fingerprint;
- accepted package/preflight;
- explicit invocation approval;
- exact renderer metadata;
- native Register marker;
- deferred finalization observed;
- `IsRegistered=true`;
- mesh memory available;
- no forbidden downstream mutation.

---

# 59. Minimum visual/decode proof

- proof tied to same package/test ID;
- screenshot/frame evidence or equivalent reviewed visual evidence;
- correct geometry;
- no gross decode corruption;
- normals/tangents plausible;
- materials expected;
- bounds/culling stable;
- comparison baseline retained;
- candidate decision explicit.

---

# 60. Minimum native equip proof

- custom armour item identity registered;
- item acquired normally;
- correct equipment slot;
- `BaseClothes` owner invoked;
- target `KandraRig` resolved;
- clothing asset loaded;
- `ClothStitcher.Stitch` completes;
- expected renderer redirects/attachments exist;
- body cover/culling correct;
- movement/animation deformation correct;
- unequip destroys/releases clothing instance;
- re-equip produces one clean instance.

---

# 61. Minimum persistence proof

If durable custom armour is claimed:

- save with armour in inventory;
- cold load;
- identity resolves;
- item restores;
- equip after load works;
- save while equipped;
- cold load;
- `ItemEquip` restore rebuilds native clothing;
- Kandra assets resolve;
- no duplicate renderer;
- missing-package behavior recorded;
- migration/update behavior recorded;
- uninstall policy explicit.

Static save architecture alone is insufficient.

---

# 62. Current proof boundary

## Proven / strongly evidenced

- standalone importer ownership;
- typed production importer entry/result;
- importer-owned Unity geometry capture;
- source/target contract model;
- deformation-validation implementation;
- real sample execution producing blocking orientation-reversal evidence;
- loose Kandra writer;
- package validation;
- registration preflight/candidate/dry-run architecture;
- explicit host-invocation gate;
- live Kandra host-registration proof for a proof mesh;
- `IsRegistered`/mesh-memory proof concept;
- same-mesh A/B package/evidence architecture;
- full-section candidate generation work;
- managed Kandra stream order;
- loose mod Kandra path;
- native Kandra registration/finalization/unregister lifecycle;
- static native `BaseClothes → ClothStitcher → KandraRig/KandraRenderer` equip/unequip architecture.

## Partially proven

- semantic Kandra encoding;
- target package registration planning;
- source↔target compatibility for selected samples;
- visual decode selection for researched fields;
- native target armour contracts.

## Not generally proven

- arbitrary production source conversion;
- complete production-equivalent Kandra codec;
- arbitrary target armour runtime registration;
- end-to-end custom armour `ItemTemplate` + equip;
- correct deformation across production armour families;
- all body variants;
- body-cover/culling correctness for custom targets;
- complete material/shader behavior;
- native clothes teardown for a production custom target;
- cold save/load of custom target armour;
- missing-mod behavior;
- uninstall/migration;
- IL2CPP equivalence;
- universal release process.

---

# 63. The rule to carry forward

The correct armour process is:

~~~text
source truth
→ geometry truth
→ target truth
→ deformation truth
→ Kandra payload truth
→ runtime registration truth
→ visual truth
→ item/equip truth
→ cleanup truth
→ persistence truth
~~~

No stage may borrow authority from the next.

The hardest armour failures come from collapsing those layers into “the mesh loaded.” The Tainted Armour process exists specifically to prevent that mistake.
