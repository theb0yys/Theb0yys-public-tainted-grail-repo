# Packet A — Native Rigid-Melee Source-Archetype Identification and Comparative Selection

**Date:** 2026-08-10  
**Domain:** Weapon Importer / Tainted Weapons  
**Research packet:** A  
**Status:** `BLOCKED BY ADMISSION`  
**Prerequisites:** passing `repository-integrity/1` and `foa-environment-fingerprint-v2`  
**Implementation authority:** none; research only

## Objective

Identify the exact current-build native rigid-melee weapon candidates suitable as the first canonical cloning source, compare at least two ordinary native archetypes, and select one source based on semantic suitability rather than visual similarity.

The packet must output the chosen template GUID/identity, rejected alternative(s), lineage, representation identities, initial source-profile boundaries, granular claims, uncertainties, falsification criteria, and revalidation triggers.

## Admission rule

Do not start current-build archetype selection until both admission receipts pass:

```text
repository-integrity/1 = PASS
foa-environment-fingerprint-v2 = PASS
```

The environment receipt must bind the clean repository commit, local Steam branch/build/depot manifest, FoA displayed version, Unity version, runtime track, BepInEx/Harmony versions, `TG.Main.dll`, `Awaken.ECS.dll`, relevant Unity Entities assemblies, `ScriptingAssemblies.json`, game executable, and decompiler identity.

Historical project research and Questline source may guide candidate discovery but may not silently substitute for current-build evidence.

## Candidate requirements

Prefer ordinary rigid melee weapons with:

- current-build ItemTemplate identity and GUID;
- ordinary native equip/inventory integration;
- normal ItemEquipSpec / ItemEquip route;
- inspectable equipped representation;
- CharacterHandBase and, where applicable, CharacterWeapon;
- normal melee attack path;
- FPP and TPP representation;
- inventory/equipment preview;
- no unnecessary bespoke scripting or exotic VFX dependency;
- enough similarity to the first importer consumer to be useful without making visual similarity the selection criterion.

At least two candidates must be evaluated.

## Comparative evidence matrix

For each candidate capture or establish, where available:

### Template identity
- ItemTemplate GUID;
- template name;
- concrete template type;
- abstract-template lineage;
- weapon-category predicates/tags;
- source asset locator/profile identity.

### Attachments
- ItemEquipSpec presence and serialized configuration;
- attachment component types;
- attachment groups;
- TemplateReference relationships;
- known combat/UI/economy/presentation attachments;
- unresolved attachment semantics.

### Equipped representation
- representation records by actor/gender/hand where applicable;
- equipped prefab/reference identity;
- root component type;
- CharacterHandBase/CharacterWeapon presence;
- renderer backend/topology, including Drake where used;
- hierarchy/layer/transform profile boundaries.

### Combat
- melee category;
- collider/sweep owner;
- hit detection owner;
- damage/hit-stop/finisher ownership boundaries;
- whether representation geometry appears coupled to gameplay geometry;
- bespoke combat scripts or unusual exceptions.

### Animation
- animator/animation mapping ownership;
- attack/equip/unequip event path;
- special bespoke animation dependencies;
- FPP/TPP distinctions relevant to source suitability.

### Presentation lanes
Independently identify current-build native ownership for:
- FPP;
- TPP;
- inventory/equipment preview.

Do not implement or fix any presentation behaviour in this packet.

### Persistence identity
- ItemTemplate GUID significance;
- Item model/template linkage;
- equipped-slot identity where relevant;
- any source identity that future save restoration will require.

## Selection criteria

Rank candidates against:

1. semantic completeness and inspectability;
2. ordinary/native lifecycle usage;
3. minimal bespoke behaviour;
4. combat inheritance suitability;
5. animation inheritance suitability;
6. FPP/TPP/preview coverage;
7. persistence identity clarity;
8. source-profile stability and detectability;
9. suitability for a rigid-melee v1 lane;
10. ability to falsify clone-equivalence assumptions cleanly.

Visual similarity is secondary and cannot decide selection by itself.

## Mandatory alternatives

Evaluate at least:

```text
Candidate A — preferred source hypothesis
Candidate B — credible ordinary rigid-melee alternative
```

If neither is sufficiently ordinary/inspectable, return `RESEARCH MORE` and identify additional candidates rather than forcing a canonical source.

## Claim Registry outputs

Create bounded claims such as:

```text
CLM-ARCHETYPE-001
Candidate A is a current-build ordinary rigid-melee ItemTemplate with the stated GUID and lineage.

CLM-ARCHETYPE-002
Candidate A uses the normal ItemEquip/CharacterHandBase lifecycle rather than a bespoke parallel equip path.

CLM-ARCHETYPE-003
Candidate A provides independently identifiable FPP, TPP and preview representation routes.

CLM-ARCHETYPE-004
Candidate A's combat owner is sufficiently archetype-preserving for v1 clone research.

CLM-ARCHETYPE-005
Candidate B is rejected for the explicitly evidenced reason(s).
```

Every claim must carry scope tuple, supporting/contradicting evidence, confidence, maturity, dependencies, and revalidation triggers.

## Falsification

The preferred candidate is rejected if evidence shows any of the following:

- bespoke item/equip lifecycle that bypasses the ordinary native route;
- presentation-specific geometry materially controls gameplay semantics in a way incompatible with bounded visual replacement;
- hidden or untraceable attachment dependencies dominate behaviour;
- source representation is unusually scripted/exotic;
- persistence identity is ambiguous;
- FPP/TPP/preview routes cannot be distinguished sufficiently for later profiling;
- an alternative candidate is materially simpler and more representative.

## Required output

The final Packet A report must contain:

1. exact admitted current-build provenance reference;
2. candidate discovery method;
3. candidate table;
4. immutable/current-build locators where available;
5. GUID/template identities;
6. abstract lineage summary;
7. attachment summary;
8. equipped representation identities;
9. combat ownership comparison;
10. animation ownership comparison;
11. FPP/TPP/preview comparison;
12. persistence-identity comparison;
13. candidate risk/complexity comparison;
14. selected canonical source or explicit `RESEARCH MORE`;
15. rejected alternative(s) with evidence;
16. initial `sourceTemplateProfile` boundary;
17. initial `sourceEquippedPrefabProfile` boundary;
18. granular Claim Registry entries;
19. uncertainties/new child Research Units;
20. revalidation triggers.

## Promotion boundary

Packet A may select a **research canonical source archetype**. It does not prove clone semantic equivalence and does not authorise native registration, package-facing field overrides, presentation fixes, inventory grants, or save mutation.

Successful Packet A unlocks Packet B:

**Native Template Inheritance and Attachment Graph.**
