# Proprietary System Documentation Standard

> **Mandatory handbook authoring contract.** Every public page that documents a Questline / Awaken Realms proprietary system must follow this structure unless a later explicit maintainer decision supersedes it.

The purpose of these pages is not merely to list types, gates, or research findings. They must teach a technically capable reader who has **not** reverse engineered Tainted Grail: The Fall of Avalon enough of the system to understand:

- what the system is for;
- where it sits in the game;
- which native objects own it;
- what data and identities it expects;
- how its lifecycle works;
- how native FoA enters it;
- how a mod can enter it without bypassing native ownership;
- what the system accepts, rejects, delays, or fails on;
- why the proven process has its current shape;
- how to verify every stage;
- what remains unproven.

A reader should finish a proprietary-system page able to say:

> “I understand what this system owns, how FoA reaches it, what contract I must satisfy, where I can integrate, what failure means, and how to prove I did it correctly.”

The documentation must therefore translate reverse-engineered evidence into a **system model**, not dump raw decompilation or present a sequence of unexplained gates.

---

# 1. Mandatory page structure

Every proprietary-system page must contain the following sections in this order.

## 1. What this system is

Start in plain technical language.

Explain:

- the system's purpose;
- what problem it solves for FoA;
- what it is **not**;
- the simplest useful mental model;
- where it sits in the broader game stack.

Do not begin with a wall of class names.

A reader must understand the purpose before the implementation vocabulary.

---

## 2. Where it sits in FoA

Show the upstream and downstream ownership chain.

Use a compact flow such as:

~~~text
upstream definition/owner
→ proprietary system entry
→ internal lifecycle
→ downstream presentation/runtime owner
→ cleanup/persistence
~~~

Explain which adjacent systems feed it and which consume its results.

When ownership crosses systems, name the boundary explicitly.

---

## 3. Native owners, identities, types, and data contracts

Document the exact native surface.

Include, where applicable:

- assemblies;
- namespaces;
- core types;
- managers;
- components;
- data structures;
- GUIDs;
- addresses;
- template identities;
- package/file identities;
- registry keys;
- relevant native methods;
- serialized fields;
- runtime IDs.

For every important identity, explain **what kind of identity it is** and what it must not be confused with.

For every important type, explain its responsibility rather than merely listing it.

---

## 4. What the system expects

Describe the input contract.

Answer:

- What must exist before entry?
- What exact objects/data/files are required?
- Which fields/counts/references must agree?
- Which runtime state must already be ready?
- Which thread/scene/service/provider conditions matter?
- What is mutable and what must remain stable?
- Which inputs are version-sensitive?

Where the system has framework-level policy in addition to native requirements, label them separately:

~~~text
native requirement
vs
framework safety constraint
~~~

Do not present a framework restriction as though Questline requires it unless the evidence proves that.

---

## 5. Lifecycle

Document the complete lifecycle from creation or discovery through terminal cleanup.

At minimum cover, when applicable:

~~~text
creation / discovery
→ admission / registration request
→ queued / pending state
→ validation
→ native registration
→ deferred finalisation
→ active runtime use
→ update path
→ disable / unregister
→ teardown / release
→ disposal
~~~

If registration is deferred, asynchronous, multi-stage, or manager-owned, show that explicitly.

A method call returning is not automatically lifecycle completion.

Every proprietary-system page must identify the **terminal success state**, not only the entry call.

---

## 6. How native FoA enters the system

Trace the normal game-owned route.

Answer:

- Which native owner initiates entry?
- Which lifecycle event causes it?
- Which data/reference does it pass?
- Which manager receives it?
- How is the result connected back to the native object?
- Who owns the resulting lifetime?

This section is essential because the safest custom-content route normally preserves that native chain.

Do not skip directly from “system exists” to “call this private method.”

---

## 7. How a mod enters the system safely

Only after the native route is understood, document the proven intervention seam.

Explain:

- where the mod enters;
- why that seam was selected;
- what native ownership is preserved;
- what the mod owns;
- what the native system continues to own;
- readiness requirements;
- identity requirements;
- failure behavior;
- cleanup responsibility;
- version sensitivity.

When a shared provider/framework exists, document the consumer boundary.

A consumer must not silently create a second source of truth for identities, resources, registration, or lifetime.

---

## 8. What it accepts, rejects, queues, and fails on

This section is mandatory.

Document the actual contract behavior discovered through source, static inspection, runtime evidence, or controlled validation.

Separate:

### Accepted

Conditions known to enter the intended path.

### Rejected

Conditions explicitly refused or known to fail validation.

### Queued / deferred

Conditions that are valid but cannot complete yet because the system is not ready.

### Partial / dirty failure

Cases where mutation may already have occurred and clean rollback is not proven.

### Unsupported

Profiles or operations outside the proven system contract.

Explain **why** each condition exists where evidence supports the explanation.

A list of error strings without system context is insufficient.

---

## 9. Failure history and what each failure taught us

Preserve the important failed paths.

For each major failure record:

~~~text
attempt
→ observed result
→ actual owner/cause
→ wrong assumption
→ corrected rule
~~~

This is part of the technical documentation, not optional project history.

The failed path often explains the architecture more clearly than the successful one.

Examples of the kind of lesson this section should preserve:

- visible object != correctly owned object;
- registration call != fully registered state;
- asset load != gameplay definition;
- native map visibility != every downstream cache refreshed;
- UI mutation after snapshot != UI ownership;
- successful current session != persistence safety.

Do not remove failure evidence merely to make the final process look simpler.

---

## 10. How to verify the system

Verification must be stage-specific.

Separate evidence lanes:

### Source / static

What source or decompilation can prove.

### Runtime

What must be observed in the running game.

### Controlled validation

What requires a predefined repeatable test and negative controls.

### Save / persistence

What requires cold save/load, missing-provider/package, migration, or uninstall tests.

### Compatibility

What requires exact build/runtime/assembly/package matrices.

Define success markers for each lifecycle stage.

Do not use one successful stage as proof of an adjacent stage.

---

## 11. Proven custom-content process through this system

The practical pipeline belongs **inside the related proprietary-system documentation**.

It must be presented as:

> “Now that you understand how this native system works, here is how a new custom entity enters it correctly.”

Do not present unexplained gate names as the process.

For every process stage explain:

- what you do;
- which native/system contract that satisfies;
- why the stage exists;
- what failure looks like;
- how to verify it;
- what the stage still does not prove.

A useful format is:

~~~text
Stage
  What you do
  Native contract being satisfied
  Why
  Failure modes
  Verification
  Proof boundary
~~~

The process should read as the logical consequence of the architecture described earlier in the page.

---

## 12. Current proof boundary and unknowns

End every proprietary-system page with explicit evidence boundaries.

Separate:

### Proven

Exact behavior supported for the recorded scope.

### Partially proven

Implemented or observed behavior whose full lifecycle or generalization remains incomplete.

### Static only

Architecture/ownership known from source or decompilation but not runtime-proven.

### Runtime bounded

Observed in a specific environment/session only.

### Not proven / blocked

Missing evidence.

### Version-sensitive

Claims that require revalidation when game/runtime/assembly/package state changes.

Do not end with vague language such as “more testing is needed.”

Name the exact missing proof.

---

# 2. Mandatory teaching rules

The following rules apply to every proprietary-system page.

## Explain before naming gates

Never assume a reader knows why a gate exists.

Bad:

~~~text
Run Kandra registration preflight, then host proof, then target gate.
~~~

Required:

~~~text
Kandra registration needs mesh metadata and packed stream counts to agree before the native manager sees the renderer. The preflight exists to catch that mismatch before runtime mutation. Once that static contract passes, the host proof can test the native registration lifecycle without yet claiming armour equip.
~~~

Then name the gate.

## Define jargon on first use

Terms such as:

- Kandra;
- Drake;
- BRG;
- MVC Model;
- Element;
- View;
- ModService;
- TemplateReference;
- provider;
- registrar;
- source profile;
- presentation profile;

must be explained before being relied upon.

## Use diagrams aggressively

Where a lifecycle has more than a few owners, include an ASCII flow diagram.

Prefer:

~~~text
Item
→ ItemEquip
→ CharacterHandBase
→ Drake
→ renderer resources
~~~

over several paragraphs that force the reader to reconstruct the chain.

## Explain the reason behind private/reflection calls

A private method name alone is not useful documentation.

Explain:

- what native state it changes;
- why there is no public equivalent;
- what readiness it assumes;
- what can go wrong;
- whether rollback exists.

## Preserve native ownership

The public documentation should teach readers to enter the native system rather than construct parallel systems unless the evidence specifically proves a replacement design.

---

# 3. Mandatory evidence labelling

Every material claim must make its proof level clear.

Use the public evidence vocabulary in [Testing and Evidence Status](../EVIDENCE.md) and preserve distinctions such as:

- `STATIC_CONFIRMED`;
- `SOURCE_CONFIRMED`;
- `SOURCE_BUILD_EVIDENCED`;
- `LOAD_EVIDENCED`;
- `RUNTIME_EVIDENCED`;
- `RUNTIME_PASSED`;
- `NOT_RUN`;
- `NOT_PROVEN`.

Where the private evidence uses stronger research-governance distinctions, translate them without inflating the claim.

A public teaching rewrite does not inherit `RUNTIME_PASSED` automatically from the private implementation that inspired it.

---

# 4. Mandatory Golden Rules integration

Every proprietary-system page must explicitly connect its system-specific findings to the repository Golden Rules.

At minimum consider:

- exact identity matters;
- native owner matters;
- lifecycle timing matters;
- registration is not runtime object creation;
- asset loading is not registration;
- visibility is not correct ownership;
- method return is not always lifecycle completion;
- cleanup is part of the feature;
- runtime success is not persistence proof;
- one archetype does not prove all archetypes;
- static evidence does not prove runtime behavior;
- one runtime observation does not prove compatibility;
- fail closed when ownership or identity is uncertain.

Do not merely link to Golden Rules. Show how the system demonstrates them.

---

# 5. Mandatory proprietary-system process integration

A domain process must be located with the proprietary system that owns the hard part of that domain.

Examples of intended relationships:

~~~text
Drake
  → rigid weapon presentation process
  → provider/resource lifetime
  → FPP/TPP/preview verification

Kandra
  → armour geometry/Kandra conversion and registration
  → native clothes/stitch lifecycle
  → body-cover/deformation/cleanup verification

Templates / registry ownership
  → custom definition registration
  → identity/collision/readiness

native actor/location lifecycle
  → custom creature template/actor process
  → controlled construction
  → combat/death/corpse/cleanup
~~~

A separate end-to-end domain page may still exist as a consolidated walkthrough, but it must link back to the proprietary-system pages that explain the underlying mechanism.

The consolidated page must not become a second independent truth.

---

# 6. Mandatory rejection of “three gates and good luck” documentation

The following documentation style is prohibited for proprietary systems:

~~~text
Prerequisites
Gate 1
Gate 2
Gate 3
Done
~~~

unless every gate is already explained by the system architecture on the same page or directly linked owning documentation.

A process page must teach:

- what state exists before the gate;
- what changes at the gate;
- which owner performs the transition;
- what invariant is checked;
- what successful state exists afterward;
- what remains outside scope.

The reader should never be required to reverse engineer the documentation in order to understand the reverse-engineered system.

---

# 7. Required reader outcome

Before a proprietary-system page is considered ready for the handbook, a technically competent reader who did not perform the original research should be able to answer:

1. What problem does this system solve?
2. Which native objects own it?
3. What identities does it use?
4. What data does it consume?
5. What is its complete lifecycle?
6. How does vanilla FoA enter it?
7. Where can a mod enter it?
8. What must be ready first?
9. What will it reject or defer?
10. What is the terminal success state?
11. What failure modes matter?
12. How is cleanup performed?
13. How is each stage verified?
14. Which proof is static, runtime, save, or compatibility?
15. How does a genuinely new custom entity reach this system?
16. Which parts of that process are actually proven today?
17. What remains unknown, blocked, or version-sensitive?

If the page cannot answer those questions, it is not complete enough for the public proprietary-system handbook.

---

# 8. Authoring source model

Public proprietary-system documentation should be built from the evidence layers in this order:

~~~text
first-party / Merlin terminology and identities
→ current native source/decompilation architecture
→ private working implementation evidence
→ runtime observations
→ controlled validation
→ failure history
→ reviewed proof boundaries
→ user-friendly public explanation
~~~

Merlin and other first-party surfaces provide important authoring terminology, IDs, GUIDs, addresses, and native structure.

They do not replace the reverse-engineered runtime/system lifecycle where that lifecycle is not publicly documented.

Private research supplies the missing mechanism.

The public handbook must translate that mechanism without copying private implementation source or proprietary game code.

---

# 9. Public clean-room boundary

A proprietary-system page may publish:

- type and method names needed to explain integration;
- system architecture;
- identities required by public examples;
- lifecycle diagrams;
- bounded pseudocode;
- evidence-derived rules;
- failures and diagnostics;
- independently authored teaching examples.

It must not publish:

- proprietary game assemblies;
- bulk decompiled source;
- extracted commercial assets;
- private-project code copied verbatim merely because it works;
- user-specific paths;
- secrets/tokens;
- private diagnostic data that should not be public.

The goal is **complete technical understanding without redistributing proprietary material**.

---

# 10. Relationship to the domain handbook

The existing domain pages remain consolidated end-to-end references:

- [Items](ITEMS.md)
- [Weapons](WEAPONS.md)
- [Armour](ARMOUR.md)
- [Creatures](CREATURES.md)

As the proprietary-system pages are expanded, those domain pages should become navigational end-to-end walkthroughs over the same evidence rather than competing architecture owners.

System truth belongs with the system.

Domain workflow belongs with the domain.

The two must cross-link and must not contradict each other.
