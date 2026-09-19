# Weapons: Native Item, Equip, Combat, Presentation, and Importer Process

> **Reference/process page.** This page reconstructs the complete known weapon path from the maintainer's private working evidence. It distinguishes the **native weapon architecture**, the **implemented registration/presentation machinery**, and the **still-unproven end-to-end generic importer**. A custom weapon is not considered solved because a template registers or a mesh becomes visible once.

## Document status

**Domain:** weapons / equipment / combat / presentation  
**Primary public conclusion:** **PARTIAL PATH PROVEN**  
**Native ownership graph:** strongly established by current-build static evidence  
**Custom native registrar:** implemented/source-confirmed, with bounded runtime-oriented design and fail-stop semantics  
**Custom presentation:** substantial Drake framework implementation and consumer evidence exists  
**Complete generic importer:** **NOT PROVEN / NOT PROMOTED**  
**Persistence/missing-package/migration:** unresolved as a generic release contract  
**Cross-build/IL2CPP:** separate evidence lane; do not infer

The private owner-level importer process is explicitly marked as proposed/review-required rather than promoted current authority. This public page therefore does not turn that proposal into a completed claim. It documents:

1. what the native game owns;
2. what the current implementation actually does;
3. what the full importer must eventually prove;
4. which failures produced the current constraints;
5. which stages remain blocked.

---

# 1. What a weapon actually is in FoA

A FoA weapon is not merely:

~~~text
ItemTemplate + mesh
~~~

The native ownership chain is approximately:

~~~text
registered ItemTemplate
→ runtime Item : MVC Model
→ ItemEquipSpec attachment
→ ItemEquip : Element<Item>
→ representation selection
→ ARAssetReference / equipped prefab load
→ CharacterHandBase : View<Item>
→ melee implementation such as CharacterWeapon
→ native character attach/bind lifecycle
→ native combat sweep/hit behavior
→ native renderer/presentation backend
→ native inventory/equipment/save ownership
~~~

For Drake-backed rigid weapons the presentation chain additionally includes Drake-owned renderer/resource state.

The durable gameplay object is the native `Item`.

A custom importer should not invent a parallel universal “Weapon Model” that bypasses this chain.

---

# 2. The full required outcome

A weapon package is **not** fully imported merely because:

- an AssetBundle exists;
- an importer API accepted a manifest;
- a custom `ItemTemplate` registered;
- a custom `Item` appears in inventory;
- one mesh rendered;
- one equip event happened;
- one attack worked;
- one save reload worked.

The complete target process is:

~~~text
source intake
→ deterministic authoring inputs
→ target-platform bundle build
→ package/filesystem validation
→ isolated bundle inspection
→ runtime package admission
→ asset-provider ownership
→ native ItemTemplate registration
→ native Item acquisition
→ native equip lifecycle
→ FPP presentation
→ TPP presentation
→ inventory-preview presentation
→ combat behavior proof
→ unequip/release proof
→ scene-transition proof
→ save/load/reload proof
→ missing-package proof
→ migration proof
→ multi-package compatibility proof
→ release-package proof
~~~

Every arrow represents a separate claim.

No earlier stage may imply a later stage.

---

# 3. One system, one truth

The importer architecture is designed around a single-owner rule.

Once a consumer is migrated to the shared weapon importer/framework, that consumer must not maintain a second active implementation that:

- calls `TemplatesLoader.AddToMap` itself;
- clones/registers the same weapon template independently;
- owns a second bundle loader for the same package;
- creates a second runtime prototype for the same identity;
- owns a second Drake conversion route;
- patches a competing FPP/TPP/preview presentation path;
- silently falls back to local registration when framework admission fails;
- independently remaps the persistent custom GUID;
- unloads framework-owned resources;
- grants/stocks/serializes an identity that the shared registrar rejected.

Temporary migration adapters require an explicit owner, lifetime, negative controls, and deletion gate.

A fallback that can create two live truths is not a safe fallback.

---

# 4. Native definition owner — ItemTemplate

Weapons begin in the item domain.

A weapon `ItemTemplate` participates in:

- persistent GUID identity;
- template name;
- abstract-template lineage;
- item classification;
- attachment groups;
- icon/localization/economy fields;
- `ItemEquipSpec`;
- weapon-specific references inherited from the native archetype.

The weapon process therefore inherits the **identity/registration discipline** from the item process, but not the entire item integration process.

A weapon adds several owners that normal non-equippable items do not have.

---

# 5. Runtime owner — Item

The weapon in inventory/save state is the native `Item`.

It owns or participates in:

- quantity/state;
- attachment reconstruction;
- equip-slot state;
- equip/unequip events;
- serialization;
- inventory ownership.

The reusable architecture keeps this native object rather than constructing a mod-owned parallel weapon state machine.

---

# 6. Equip definition owner — ItemEquipSpec

`ItemEquipSpec` is the authoring attachment that makes the item equippable.

Static evidence shows it carries data including:

- equipment type;
- hero/NPC representation entries;
- gem slots;
- finisher type;
- hit-stop type;
- transmog behavior;
- quiver-hide behavior.

It spawns the runtime `ItemEquip` element.

A custom weapon template that loses the correct equip attachment is not semantically equivalent to the source archetype.

---

# 7. Runtime equip owner — ItemEquip

`ItemEquip` is an `Element<Item>`.

Its native responsibilities include:

- listening to Item equip/unequip events;
- selecting the appropriate representation;
- async asset loading;
- checking ownership/equipped state after load;
- instantiating the representation;
- requiring the correct native hand/view type;
- binding the view to the Item;
- attaching the weapon to the character;
- handling renderer-specific settings;
- releasing/discarding representation on unequip;
- restoring equip state after item restore.

This is why a “mesh swap” is not equivalent to native equip integration.

---

# 8. Equipped view owner — CharacterHandBase

`CharacterHandBase` is the native equipped weapon View.

Evidence establishes that the native item/equip path binds it to the Item and uses it as the real hand representation.

For melee weapons the concrete path may involve `CharacterWeapon`.

Important implication:

~~~text
the equipped GameObject is not just decoration
it participates in native View ownership
~~~

---

# 9. Combat owner — CharacterWeapon/native combat systems

For the bounded rigid melee path, the safe design is archetype-preserving.

The imported visual does **not** automatically define:

- melee sweep geometry;
- attack timings;
- damage;
- poise;
- hit-stop;
- finisher behavior;
- animation state;
- audio/trails;
- special attacks.

The first reusable profile therefore retains the native source archetype's combat profile.

Do not derive melee collision from custom render-mesh bounds and call that native-equivalent.

---

# 10. Presentation owner — native renderer / Drake

Some weapon presentation is Drake-backed.

Current evidence shows the visible native weapon can remain owned by the Drake path even when a custom ordinary Unity renderer is active.

This failure produced a critical rule:

~~~text
visible old weapon still present
+ custom Unity renderer exists
=> custom renderer did not replace the actual native presentation owner
~~~

A reusable rigid-weapon solution must integrate with the renderer that actually owns the native weapon presentation.

---

# 11. Important native methods and surfaces

Weapon work may depend on:

- `TemplatesProvider.AllLoaded`
- `TemplatesProvider.Get<ItemTemplate>(...)`
- private `TemplatesLoader.AddToMap(...)`
- `ItemTemplate`
- `Item`
- `World.Add(...)`
- `ItemEquipSpec`
- `ItemEquip`
- `Item.Events.Equipped`
- `Item.Events.Unequipped`
- representation selection inside `ItemEquip`
- `ARAssetReference.LoadAsset<GameObject>()`
- `ItemEquip.OnWeaponLoaded(...)`
- `CharacterHandBase`
- `CharacterWeapon`
- `World.BindView(...)`
- character attach/detach weapon lifecycle
- `View.Discard()`
- asset-reference release
- Drake renderer/resource owners where selected

Every one is version-sensitive until compatibility evidence says otherwise.

---

# 12. Native equip lifecycle

The current static architecture is:

~~~text
Item becomes equipped
→ Item.Events.Equipped
→ ItemEquip reacts
→ select correct representation for actor/gender/hand
→ load ARAssetReference<GameObject>
→ async completion
→ re-check item/owner/equipped state
→ instantiate under native socket
→ require CharacterHandBase
→ configure representation
→ World.BindView(Item, Hand)
→ character attaches weapon
→ native weapon/equipment event continues
~~~

Unequip:

~~~text
Item becomes unequipped
→ Item.Events.Unequipped
→ character detaches weapon
→ hand/view discarded
→ loaded asset reference released
→ renderer/resource cleanup
→ other equipment state restored
~~~

Restore:

~~~text
saved Item restores
→ template/attachments reconstructed
→ ItemEquip restored
→ parent fully initialized
→ equip restoration scheduled
→ native equipped View rebuilt
~~~

A custom weapon must fit this lifecycle.

---

# 13. Archetype-preserving rule

The strongest first reusable design is:

~~~text
known native rigid melee archetype
→ separate custom identity
→ clone complete native item/equip contract
→ native Item
→ native ItemEquip
→ native CharacterHandBase/CharacterWeapon
→ source combat/animation/audio/trail behavior
→ custom rigid presentation through the actual renderer owner
~~~

This is intentionally conservative.

It reduces the number of native systems changed at once.

---

# 14. First supported profile envelope

The proposed first release-capable profile is narrow.

Conceptually:

~~~text
profile: rigid-melee-weapon/v1
runtime: Mono
platform: Windows player
native identity: cloned ItemTemplate
native gameplay: source archetype preserved
presentation: rigid custom mesh/material
targets: FPP + TPP + inventory preview
template lifetime: process-session immutable
provider/bundle lifetime: explicitly owned
~~~

## In-scope candidate families

- swords;
- axes;
- maces;
- daggers;
- similar rigid melee weapons.

## Explicitly out of this first profile

- bows;
- crossbows;
- ammunition/projectiles;
- quivers;
- shields;
- dual-wield coordination;
- skinned weapon rigs;
- custom weapon Animator/controllers;
- new movesets;
- custom combat logic;
- custom damage/perks;
- weapon scripts in content bundles;
- hot reload/unload;
- IL2CPP;
- cross-platform bundles.

An unsupported profile is blocked, not “partially imported.”

---

# 15. Package admission is not import completion

The implemented package-admission layer can validate bounded package fields and paths.

Examples of implemented checks in the evidenced baseline include:

- required manifest values;
- locator length limits;
- supported clone profile;
- package-root existence;
- relative-path containment;
- rejection of rooted/UNC/NUL/traversal-like path shapes;
- rejection of executable/script payload extensions;
- bundle existence;
- non-empty/size limits;
- bundle SHA-256;
- basic equipped-prefab locator syntax.

Those checks mean:

~~~text
package input passed bounded admission checks
~~~

They do **not** mean:

- correct target platform;
- bundle CRC passed;
- dependencies closed;
- expected asset exists;
- asset is correct type;
- mesh/material/shader profile is valid;
- FPP/TPP/preview works;
- weapon equips;
- weapon attacks;
- weapon persists;
- package is compatible;
- package is release-ready.

Status names such as “accepted” or “imported” must not be interpreted as end-to-end completion unless the owning process explicitly defines that stronger meaning.

---

# 16. Target-platform bundle requirements

A production importer must validate the bundle as a platform artifact.

Required concerns include:

- explicit build target;
- expected platform;
- deterministic input/output;
- CRC;
- SHA-256;
- asset inventory;
- dependency closure;
- exact asset path;
- exact asset type;
- content-profile restrictions;
- unload/lifetime semantics.

A Windows weapon importer should not assume an arbitrary editor-built bundle is a Windows-player bundle.

---

# 17. Content package security boundary

A weapon content package should not silently become executable code.

The bounded package design rejects or excludes executable/script payloads.

The weapon content package is presentation/data.

Any gameplay plugin is a separately governed executable owner and cannot use its existence to bypass importer validation.

---

# 18. Identity policy

Each weapon needs stable identities for at least:

- package;
- weapon;
- registry key;
- custom `ItemTemplate` GUID;
- custom template name;
- source template GUID;
- runtime prototype address;
- mesh/material keys;
- definition hash;
- source-profile hash.

Identity normalization must be deterministic.

A runtime-random GUID is not a persistence policy.

---

# 19. Registration request contract

The implemented registrar accepts an explicit request bound to an already accepted matching presentation definition.

Important behaviors include:

- request identity must match framework definition identity;
- only supported clone profiles are accepted;
- changed definition under an existing registry key is rejected pending migration decision;
- requests can queue until the framework/importer lifecycle and template provider are ready.

This prevents the registrar from becoming a general “clone any template now” function.

---

# 20. Unity-thread requirement

The implemented registrar requires execution on the expected Unity thread.

A wrong-thread request is denied.

This is a concrete lifecycle/ownership rule.

Do not move native Unity/template mutation to arbitrary worker threads because package validation itself may be asynchronous or non-Unity.

---

# 21. Template readiness

The registrar queues when:

- framework lifecycle is not open;
- `TemplatesProvider` is unavailable;
- `TemplatesProvider.AllLoaded` is false.

It retries/drains pending work only when readiness is re-established.

This separates:

~~~text
request accepted
from
native registry mutation executed
~~~

---

# 22. Source-profile restriction

The bounded implemented profile is:

~~~text
weapon-item-template-clone/v1
~~~

The profile is intentionally not a universal item-template cloner.

It exists to restrict the registrar to a reviewed weapon template shape.

A source outside the profile is rejected.

---

# 23. Collision checks

Before native insertion the registrar checks the exact custom identity.

Required concerns include:

- custom GUID collision;
- custom template-name collision;
- changed definition under same registry key;
- source/custom identity confusion;
- foreign owner identity;
- request/presentation definition mismatch.

A collision is not solved by silently picking another identity.

Persistent content requires deterministic identity.

---

# 24. Clone strategy

The registrar resolves a known source `ItemTemplate`, clones the source GameObject, and applies bounded fields.

It preserves the source native structure rather than constructing a weapon template from scratch.

This supports the archetype-preserving model.

---

# 25. Clone validation

Current clone-profile evidence compares bounded structural data including:

- component topology;
- attachment-group topology;
- nested `TemplateReference` rows.

This is useful.

It is also explicitly incomplete.

It does **not** prove equality of every:

- scalar field;
- Unity object reference;
- collection;
- addressable reference;
- child GameObject;
- enabled state;
- combat value;
- animation reference;
- audio reference;
- runtime cache.

Therefore:

~~~text
weapon-item-template-clone/v1
proves bounded topology preservation
not complete semantic equivalence
~~~

A stronger release profile needs an approved field manifest:

~~~text
field path
source value/hash
clone value/hash
whether change is allowed
reason
~~~

Unapproved fields should remain equal.

---

# 26. Native insertion

The registrar invokes the private native `TemplatesLoader.AddToMap` once.

It then verifies provider lookup returns the inserted object.

The registration receipt tracks facts such as:

- registration ID;
- registry key;
- status;
- accepted/registered flags;
- reason code;
- definition hash;
- source profile;
- source/custom profile hash;
- source template GUID;
- custom template GUID;
- custom template name;
- AddToMap invocation count;
- whether native mutation occurred.

This is important because “method call returned” is weaker than a machine-readable statement about what stage actually occurred.

---

# 27. Non-transactional registration and terminal poison

Static native evidence shows `AddToMap` performs sequential mutations and has no transaction/rollback body.

The implemented registrar therefore treats a post-visibility failure as terminal for the process.

Conceptually:

~~~text
before AddToMap
    failures can fail cleanly
after native visibility
    uncertainty can mean partial native mutation
    registrar becomes terminally poisoned
    no further registrations
    process restart required
~~~

This is a major reliability rule.

Do not catch the exception and continue registering more weapons.

---

# 28. Process-session immutability

No researched native unregister path exists for the direct template map route.

Successful registered clones therefore remain process-session state.

The registrar intentionally retains successful templates.

Hot unload/reload/in-session replacement is outside the first supported profile.

---

# 29. Late-registration visibility boundary

Current evidence proves:

~~~text
TemplatesProvider map visibility
~~~

after insertion.

It does not prove every downstream cache notices the late template.

Potential cached consumers include:

- loot;
- vendors;
- crafting;
- search/UI indexes;
- abstract-template indexes;
- save reconstruction;
- common references;
- validation systems.

Therefore the correct claim is:

**Late insertion is proven for provider lookup only. System-wide late-registration observability remains unresolved.**

---

# 30. Create the native Item

After registration/round-trip lookup:

~~~text
World.Add(new Item(customTemplate, quantity))
~~~

creates the runtime gameplay object.

The weapon should then enter one controlled acquisition route.

Do not directly equip a loose custom prefab and call that a weapon import.

---

# 31. Acquisition is a separate gate

Possible controlled acquisition adapters include:

- debug/manager grant;
- known merchant insertion;
- other reviewed acquisition surfaces.

The first full importer proof should use one narrow adapter.

Acquisition must verify:

- custom template resolves;
- native Item creation succeeds;
- owner accepts Item;
- inventory/UI sees it;
- duplicates are controlled.

A registrar receipt is not an acquisition receipt.

---

# 32. Merchant acquisition inherits item timing rules

If a weapon is stocked in a merchant, use the proven decompressed-stock/UI timing from the item process.

Conceptually:

~~~text
Stock.ShopOpened()
→ decompressed RestockableStock
→ ShopUI.OnFullyInitialized Prefix
→ World.Add(new Item(customWeaponTemplate, ...))
→ stock.AddItem(...)
→ UI captures list
~~~

The weapon importer does not get to invent a second merchant lifecycle.

---

# 33. Native equip proof

After acquisition, prove the weapon enters the native equip chain.

Required observations include:

- Item equip event;
- `ItemEquip` response;
- correct representation selected;
- asset load completion;
- resulting object has expected `CharacterHandBase`;
- view binds to the same native Item;
- character attach completes;
- source combat profile remains intact.

A visible child GameObject alone is not sufficient.

---

# 34. Evil Greatsword failure — custom renderer visible, native weapon still visible

One consumer demonstrated a key presentation failure.

A custom Unity renderer could become active while the old native weapon remained visible.

That meant:

- the custom object existed;
- it could render;
- it had not replaced the native visible weapon owner.

The failure disproved a simple renderer-attachment approach as the reusable framework path.

---

# 35. Evil Greatsword failure — inactive native root

Another runtime observation found the converted native Drake weapon root inactive while a custom child was attached beneath it.

This forced investigation of active socket ancestry and, more importantly, exposed that the ordinary Unity hierarchy was not the full presentation authority.

The production direction therefore moved toward Drake prototype rebinding rather than continuing to patch symptoms in an inactive hierarchy.

---

# 36. Earliest narrow representation seam

Static evidence shows `ItemEquip` selects an `ARAssetReference` representation before loading and before `OnWeaponLoaded` instantiates it.

That creates an earlier, narrower representation seam than attaching a new renderer after the native weapon has already been built.

The shared framework direction is therefore:

~~~text
native ItemEquip selects representation
→ framework supplies/rebinds a weapon-compatible prototype
→ vanilla OnWeaponLoaded continues normal equip lifecycle
~~~

not:

~~~text
let native weapon fully mount
→ bolt a second visual onto it
→ hide pieces until it looks right
~~~

---

# 37. Drake prototype strategy

The current Tainted Weapons direction uses the native weapon prefab as a structural source.

Conceptually:

~~~text
capture reviewed native equipped prototype
→ clone native CharacterHandBase/Drake-authored structure
→ preserve native hand/view/combat integration
→ replace/rebind Drake mesh/material identity to importer-owned keys
→ serve assets through framework/provider ownership
→ return stable framework representation
→ let native ItemEquip finish equip
~~~

This is stronger than a direct renderer fallback because it keeps the weapon inside the native equipped-view contract.

---

# 38. Combat remains native-first

The first rigid weapon profile should retain:

- source animation/controller mapping;
- source melee sweep/hitbox;
- source attack event timing;
- source hit-stop;
- source finisher profile;
- source trail/audio behavior unless separately replaced;
- source equip semantics.

The visual asset should not silently redefine combat.

---

# 39. Animation boundary

A plain Unity `AnimatorController` in a custom bundle is not automatically the FoA hero weapon animation contract.

Evidence shows hero weapon animations flow through native animation mapping assets and native animation systems.

Therefore a custom moveset is a separate process.

For the first importer:

~~~text
custom weapon presentation
+ native archetype animation/combat
~~~

is the safe bounded goal.

---

# 40. Audio/trail/VFX boundary

Custom weapon audio, trails and VFX are separate owner systems.

A successful rigid visual import does not prove:

- custom swing audio;
- contact audio;
- trail lifecycle;
- impact VFX;
- spell effects.

Preserve native behavior first.

Add each replacement through its own researched route.

---

# 41. FPP, TPP, and inventory preview are separate presentation claims

A release-capable weapon must prove at least:

- first-person equipped view;
- third-person equipped view;
- inventory preview.

One visible perspective cannot imply the others.

Reasons include:

- different representation selection;
- different camera/visibility layers;
- different renderer ownership;
- different offsets;
- different lifecycle timing.

The importer should record independent receipts for these surfaces.

---

# 42. Asset provider lifetime

A reusable provider needs explicit ownership.

Target lifetime model:

~~~text
package mount owner
→ asset consumer lease
→ prototype consumer lease
→ equipped-instance consumer lease
→ final release
→ all counters/resources return to expected baseline
~~~

Without leases/refcounts, direct bundle caching can fail under:

- duplicate loads;
- multiple equipped instances;
- scene transition;
- remount;
- early unload;
- failure cleanup;
- dependency unload.

---

# 43. Why AssetBundle.Unload is not enough

Unity semantics make both unload modes dangerous if ownership is wrong.

`Unload(false)` can leave retained loaded objects detached from bundle ownership and allow duplicates after reload.

`Unload(true)` can destroy objects still referenced by live gameplay state.

The importer therefore needs a provider/lifetime contract, not a shutdown call selected by intuition.

---

# 44. Two simultaneous weapon instances

A reusable importer must support at least the intended multi-instance semantics.

For a rigid weapon this includes testing two simultaneous instances where the game permits them, for example inventory/equipment/preview or multiple consumers.

The provider must not:

- free shared resources when one instance releases;
- duplicate immutable resources unnecessarily;
- leak references after both release.

This is a provider proof, not a template proof.

---

# 45. Equip/unequip stress

A robust weapon path should eventually run repeated equip/unequip cycles.

The proposed process uses a high cycle count specifically to expose:

- leaked Views;
- stale Drake records;
- duplicate renderer entities;
- unreleased asset handles;
- retained provider leases;
- state drift.

One successful equip does not prove teardown symmetry.

---

# 46. Scene transition

Scene transition while equipped is another independent lifecycle test.

Questions include:

- does Item remain valid?
- does equipped View rebuild or persist correctly?
- do provider handles remain valid?
- do renderer resources survive/recreate correctly?
- is there duplicate presentation after transition?
- does cleanup occur at old-scene teardown?

Do not infer this from equip/unequip in one scene.

---

# 47. Persistence architecture

Native saves refer to templates by GUID.

The intended weapon restore chain is:

~~~text
game startup
→ framework/package startup
→ template system ready
→ custom weapon template registered
→ save reader resolves custom GUID
→ Item restores
→ attachments restore
→ equipped slots restore
→ ItemEquip.OnRestore
→ native equipped View rebuilt
→ custom presentation resolves
~~~

The unresolved critical question is ordering:

~~~text
is custom registration guaranteed before the first saved custom GUID is resolved?
~~~

Until that is runtime-proven, generic custom weapon persistence is not solved.

---

# 48. Package-present save/load

A positive persistence test should include:

- weapon in inventory;
- save;
- cold process restart;
- package/framework present;
- custom template registration ordering observed;
- item restores;
- quantity/state correct;
- equip works after load.

Also test an equipped weapon at save time.

---

# 49. Package-absent save/load

A missing-package test must be explicit.

Required questions:

- what happens when saved custom GUID cannot resolve?
- does load fail?
- does item disappear?
- is save overwritten destructively?
- can package restoration recover the campaign?
- can the framework prevent unsafe overwrite?
- can a placeholder be used without lying about identity?

No native lossless missing-template placeholder has been proven.

The conservative proposed posture is fail closed rather than substitute an unrelated vanilla weapon.

---

# 50. Migration

A durable importer must define migration for:

- compatible package update;
- changed presentation only;
- changed source archetype/profile;
- changed custom template fields;
- changed template GUID;
- changed package ID;
- framework version change;
- incompatible package update.

A changed definition under an existing registry key must not be silently accepted.

The current registrar explicitly rejects changed-definition reuse pending a migration decision.

---

# 51. Localisation boundary

The current registrar can assign provisional literal/implicit localization values.

That does not prove:

- multilingual Babel term registration;
- translation archives;
- runtime language switching;
- missing-translation fallback policy.

A release-grade importer needs a separate localisation contract.

---

# 52. Package identity and definition hashing

Definition hashing is useful for:

- idempotency;
- changed-definition detection;
- receipts;
- diagnostics.

But a hash proves only the inputs included in that hash.

If the hash omits source-template profile, game build, representation profile or external asset facts, equality can hide upstream drift.

A promoted receipt should bind all material fingerprints relevant to the stage it claims.

---

# 53. Environment fingerprint

A complete weapon compatibility claim needs an exact environment tuple including material facts such as:

- game version/build;
- Unity version;
- runtime family;
- platform/distribution;
- DLC/content set;
- `TG.Main.dll` hash/MVID;
- relevant `Awaken.ECS.dll` hash/MVID;
- BepInEx version;
- Harmony version;
- Tainted Weapons version/commit/binary hash;
- material configuration.

A historical environment observation is not a current compatibility claim.

---

# 54. Mono vs IL2CPP

The first importer profile is Mono-specific.

Do not state IL2CPP support because:

- a conceptual native architecture looks similar;
- generated interop types exist;
- a Mono reflection route exists.

IL2CPP requires its own:

- API binding;
- reflection/private-field strategy;
- Unity object semantics validation;
- runtime registration proof;
- equip/presentation proof;
- persistence proof;
- compatibility matrix.

---

# 55. Current capability-state discipline

Current framework code explicitly separates states such as:

- package;
- provider;
- template registration;
- presentation;
- equip;
- persistence;
- compatibility;
- release.

A lane may have presentation capability while template/equip/persistence/release remain blocked.

This is the correct model.

Do not collapse it into:

~~~text
framework ready = true
~~~

---

# 56. Complete importer state machine

The proposed owner process uses stages conceptually like:

~~~text
DISCOVERED
→ MANIFEST_VALIDATED
→ FILESYSTEM_VALIDATED
→ BUNDLE_VALIDATED
→ ADMITTED
→ ASSETS_READY
→ NATIVE_REGISTERED
→ PRESENTATION_PROVEN
→ ACQUISITION_PROVEN
→ PERSISTENCE_PROVEN
→ COMPATIBILITY_PROVEN
→ RELEASE_READY
~~~

Blocking/terminal outcomes include:

- rejected;
- environment blocked;
- unsupported profile;
- dependency blocked;
- migration required;
- missing package;
- dirty/restart required;
- withdrawn.

This state machine is a target process definition.

It must not be misreported as though every state transition is already implemented and validated.

---

# 57. Receipt discipline

Every stage should produce a receipt bound to the exact:

- package;
- weapon identity;
- implementation;
- environment;
- source profile;
- bundle;
- manifest;
- expected checks;
- negative controls.

Receipts must say what they prove and what they do not.

Example:

~~~text
NATIVE_REGISTERED
does not imply
PRESENTATION_PROVEN
~~~

A later receipt does not rewrite an earlier failure.

---

# 58. Failure history and resulting rules

## Failure: local consumer owns everything

Consumer had package load, registration retry/fallback, merchant, and presentation fallbacks.

**Rule:** migrate toward one owner; a consumer is a fixture, not proof of unified framework completion.

## Failure: direct renderer becomes visible but native weapon remains

**Rule:** integrate with actual presentation owner, not the nearest visible Unity renderer.

## Failure: custom child attached beneath inactive native root

**Rule:** hierarchy attachment does not prove native renderer ownership/lifecycle.

## Failure: plain AnimatorController assumed to be hero weapon animation integration

**Rule:** preserve native animation profile until FoA mapping route is separately proven.

## Failure: AddToMap treated as reversible

**Rule:** post-insertion uncertainty poisons process; restart rather than continue.

## Failure: structural clone treated as semantic equivalence

**Rule:** topology hash/profile is not full field equivalence.

## Failure: provider map visibility treated as global late-registration visibility

**Rule:** each downstream cache/owner needs its own evidence.

## Failure: bundle loaded treated as provider-lifetime proof

**Rule:** prove leases/refcounts, two instances, failure cleanup, remount, final release.

## Failure: visible equip treated as persistence proof

**Rule:** cold save/load, equipped restore and missing-package tests remain separate.

---

# 59. Minimum bounded weapon implementation sequence

1. Select one exact current-build native rigid melee source archetype.
2. Capture source template GUID/name.
3. Capture full relevant source profile.
4. Allocate stable custom weapon identity.
5. Prepare deterministic package input.
6. Build target-platform presentation bundle.
7. Validate bundle hash/CRC/inventory/dependencies.
8. Admit package through bounded importer contract.
9. Mount assets under provider ownership.
10. Wait for native template readiness.
11. Submit explicit native registration request.
12. Validate request/definition/source-profile identity.
13. Check GUID/name collisions.
14. Clone source template.
15. Apply only approved changes.
16. Compare bounded source/custom profiles.
17. Invoke native map insertion once.
18. Verify provider round-trip.
19. Fail-stop on post-visibility uncertainty.
20. Create native `Item`.
21. Add through one bounded acquisition adapter.
22. Verify inventory UI.
23. Equip through native `ItemEquip`.
24. Verify resulting `CharacterHandBase`.
25. Verify View binding/character attach.
26. Resolve custom presentation through actual renderer owner.
27. Verify FPP.
28. Verify TPP.
29. Verify inventory preview.
30. Verify source native combat behavior.
31. Verify audio/trail/animation preservation required by profile.
32. Unequip and verify cleanup.
33. Repeat lifecycle/stress tests.
34. Test scene transition.
35. Run cold save/load unequipped.
36. Run cold save/load equipped.
37. Run missing-package negative control.
38. Run reinstall recovery.
39. Run compatible/incompatible upgrade/migration tests.
40. Run multi-package collision/lifetime matrix.
41. Only then consider release readiness.

---

# 60. Minimum native-registration proof

A weapon registrar proof should show:

- expected Unity thread;
- framework/presentation identity match;
- supported source profile;
- template provider ready;
- exact source resolves;
- custom identity unoccupied or idempotently owned;
- source clone succeeds;
- bounded profile checks pass;
- one `AddToMap` invocation;
- native mutation recorded;
- provider lookup returns exact clone;
- clone retained;
- receipt recorded.

If failure happens before mutation, native maps should remain unchanged.

If failure happens after visibility, the process must not claim clean rollback without evidence.

---

# 61. Minimum equip proof

- custom Item is in native inventory;
- equip request is accepted;
- `ItemEquip` runs;
- selected representation corresponds to the custom weapon route;
- async load succeeds;
- item is still equipped at completion;
- native hand View exists;
- View binds to custom Item;
- character attach succeeds;
- expected renderer backend becomes active;
- no second old weapon remains visible;
- attack behavior uses intended native archetype;
- unequip removes presentation cleanly.

---

# 62. Minimum presentation proof

Treat these independently:

## FPP

- correct custom weapon visible;
- native duplicate absent;
- correct transform/scale;
- expected materials;
- attack animations still align;
- no camera/near-plane artifact that invalidates use.

## TPP

- correct custom weapon visible to third person;
- correct socket/orientation;
- no duplicate native presentation;
- locomotion/attack compatibility.

## Inventory preview

- expected preview owner displays custom weapon;
- preview does not consume/leak live equip resources;
- close/reopen cycles remain stable.

---

# 63. Minimum provider-lifetime proof

- first mount/load;
- second consumer shares/reuses as designed;
- first consumer release does not destroy second;
- final release returns counters to baseline;
- failure halfway through load cleans up;
- remount after full release works;
- scene transition while equipped works;
- repeated cycles do not increase retained state;
- two instances remain valid simultaneously.

---

# 64. Minimum combat-preservation proof

For first rigid profile:

- light attack;
- heavy attack;
- native hit detection;
- source sweep/hitbox behavior;
- block/parry interaction where source archetype supports it;
- equip/unequip does not change combat state unexpectedly;
- no imported render bounds used as unreviewed hitbox;
- no custom moveset claim unless separately validated.

---

# 65. Minimum persistence proof

## Present package

- save with weapon in inventory;
- cold restart;
- custom registration before resolution;
- item restores;
- equip restores if saved equipped;
- presentation resolves.

## Missing package

- package removed/disabled;
- load behavior captured;
- no silent destructive overwrite;
- recovery procedure proven.

## Upgrade

- same identity, compatible version;
- changed presentation if allowed;
- no duplicate item;
- no loss.

## Incompatible change

- blocked or explicit migration;
- no silent same-GUID semantic replacement.

---

# 66. Current proven boundary

## Strongly established

- native `ItemTemplate → Item → ItemEquipSpec → ItemEquip → CharacterHandBase/CharacterWeapon` ownership;
- native equip/unequip/restore lifecycle shape;
- archetype-preserving design rationale;
- private-Mono native registration seam;
- current Tainted Weapons registration request/receipt architecture;
- readiness queueing;
- identity/collision checks;
- bounded clone-profile checks;
- one-shot native insertion and provider round-trip;
- terminal-poison/restart posture after unsafe post-insertion failure;
- process-session template lifetime;
- substantial Drake-oriented presentation implementation direction;
- consumer evidence that direct renderer fallback is not equivalent to native Drake replacement.

## Implemented but not sufficient for a generic public importer

- bounded package admission;
- bounded native registrar;
- identity receipts;
- runtime prototype/presentation framework pieces;
- consumer migration work;
- merchant/debug acquisition adapters in consumers;
- selected runtime presentation observations.

## Still blocked / incomplete as a generic release process

- complete approved source-field equivalence profile;
- exact current-environment compatibility tuple;
- complete target-platform package validation;
- system-wide late-registration visibility;
- generic acquisition proof owned by importer;
- complete FPP/TPP/preview matrix;
- provider lifetime/refcount closure;
- combat equivalence across supported archetypes;
- repeated lifecycle stability;
- scene transition;
- cold save/load ordering;
- equipped save restore;
- missing-package behavior;
- migration;
- hot unload/reload;
- multilingual localisation;
- multi-package compatibility;
- IL2CPP;
- release/public SDK promotion.

---

# 67. The rule to carry forward

The correct weapon model is:

~~~text
weapon content package
→ validated identity + assets
→ native item registration
→ normal native Item
→ native acquisition
→ native ItemEquip ownership
→ native hand/combat View
→ renderer-owner-compatible presentation
→ cleanup
→ persistence
→ compatibility
~~~

not:

~~~text
mesh
→ attach to hand
→ weapon done
~~~

The item process supplies identity/registration fundamentals. The weapon process adds equip, combat, presentation, provider lifetime and persistence obligations that must each be proved in their own evidence lane.
