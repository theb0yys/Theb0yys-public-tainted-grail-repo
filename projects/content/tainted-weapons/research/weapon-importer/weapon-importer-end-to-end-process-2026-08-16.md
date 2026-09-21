# Weapon Importer End-to-End Owner Process — 2026-08-16

Document control:

- Status: `PROPOSED_CANONICAL_OWNER_PROCESS`
- Authority state: `REVIEW_REQUIRED`
- Owner: Tainted Weapons
- Repository owner path: `mods/tainted-weapons/`
- Architecture owner path: `documents/frameworks/weapon-importer/`
- Canonical project branch: `tainted-weapons`
- Repository baseline: `main@7247f5c06520953572ee2f695ecd07e582faf725`
- Tainted Weapons source baseline: `0.3.34`
- Change class: documentation and process definition only
- Implementation authority: `NOT_GRANTED`
- Runtime mutation authority: `NOT_GRANTED`
- Save/persistence authority: `NOT_GRANTED`
- Compatibility/release authority: `NOT_GRANTED`
- Independent review: `NOT_RUN`
- Human promotion: `NOT_RUN`

This document is the consolidated process proposed to become the single owner-level lifecycle for importing weapon content into Tainted Grail: The Fall of Avalon. It is deliberately concrete, but it does not claim that the described target pipeline is already implemented or runtime-proven. Existing source remains the authority for current executable behaviour until reviewed implementation changes are separately authorised, completed, validated, and promoted.

## 1. Required outcome

A weapon package is not considered imported because a bundle exists, because a definition API returned `true`, because an `ItemTemplate` was cloned, or because one equipped model appeared once.

The importer is complete only when one controlled system can take a declared weapon package through all of these stages:

```text
source intake
  -> deterministic authoring inputs
  -> target-platform bundle build
  -> filesystem and manifest validation
  -> isolated bundle inspection
  -> runtime package admission
  -> asset-provider ownership
  -> native template registration
  -> first-person presentation proof
  -> third-person presentation proof
  -> inventory-preview proof
  -> acquisition proof
  -> save/load/reload proof
  -> missing-package and migration proof
  -> multi-package compatibility proof
  -> release package proof
```

Every transition is fail-closed and produces a machine-readable receipt. No receipt may imply a later state.

## 2. One system, one truth

### 2.1 Canonical ownership

Tainted Weapons owns exactly one authoritative implementation for each of the following:

- weapon-package schema and version negotiation;
- package discovery and identity normalisation;
- content-path safety and payload admission;
- bundle fingerprint, CRC, dependency, and asset validation;
- loaded-bundle and loaded-asset lifetime;
- definition registration and collision policy;
- native `ItemTemplate` clone and registration;
- presentation-profile selection;
- first-person, third-person, and inventory-preview integration;
- importer receipts and diagnostic markers;
- custom-template persistence identity and migration aliases;
- missing-package detection and fail-closed save handling;
- importer compatibility matrix and release gate.

A consumer weapon project owns only its source content and package declaration unless a separately owned gameplay system is explicitly required. It must not independently own a second implementation of any importer function above.

### 2.2 Prohibited duplicate paths after migration

After a consumer is migrated to the canonical importer, that consumer must not retain an active parallel path that:

- calls or patches `TemplatesLoader.AddToMap`;
- clones or registers its own native `ItemTemplate`;
- loads the same weapon bundle directly;
- constructs a second runtime prototype for the same weapon identity;
- owns a second Drake conversion/lifetime route;
- patches first-person, third-person, or preview presentation for the same imported identity;
- silently falls back from rejected importer admission to local registration;
- independently remaps the same persistent GUID;
- unloads an importer-owned bundle;
- grants, stocks, or serialises an unregistered custom identity.

Temporary migration adapters are permitted only when their exact lifetime, owner, negative controls, and deletion gate are recorded. A fallback that can produce a second live truth is prohibited.

### 2.3 Truth surfaces

The owner surfaces are separated as follows:

| Truth | Owning surface |
|---|---|
| Current executable behaviour | `mods/tainted-weapons/src/` at an exact commit |
| Package contract | Reviewed schema and contract implementation owned by Tainted Weapons |
| End-to-end lifecycle | This process after independent review and human promotion |
| Native-game mechanism | Reviewed current-build static/decompilation evidence |
| Runtime ordering/behaviour | Current-environment runtime evidence packet |
| Persistence behaviour | Save/load/reload/migration evidence packet |
| Compatibility | Exact environment/package matrix evidence |
| Release state | Owner-local gate and release receipts |

No prose summary, older design, generated report, successful build, or single runtime observation may replace these owner surfaces.

## 3. Current reality at the repository baseline

### 3.1 Implemented package admission

At `main@7247f5c06520953572ee2f695ecd07e582faf725`, Tainted Weapons `0.3.34` implements an in-process package contract identified as:

```text
tainted-weapons.weapon-package/1
```

The current admission path accepts a caller-constructed manifest, validates selected strings and one local bundle path, calculates the bundle SHA-256, registers a presentation definition, and optionally requests native-template registration.

Current v1 validation includes:

- required manifest values;
- a 240-character locator ceiling;
- one supported native clone profile, `weapon-item-template-clone/v1`;
- package-root existence;
- relative-path containment checks;
- rejection of rooted, UNC, NUL, empty, `.` and `..` path segments;
- rejection of bundle payloads ending in `.dll`, `.exe`, `.bat`, `.cmd`, or `.ps1`;
- bundle existence, non-empty size, and a 128 MiB ceiling;
- bundle SHA-256 calculation;
- syntax validation of the declared equipped-prefab path.

Current v1 admission does **not** establish:

- a disk manifest format or schema validation;
- package-version semantics;
- current game/Unity/assembly compatibility;
- bundle target platform;
- bundle CRC verification;
- bundle dependency closure;
- bundle asset inventory;
- existence or exact type of the declared prefab;
- component, renderer, mesh, material, shader, pivot, scale, or bounds conformance;
- bundle lifetime ownership across all consumers;
- guaranteed registration before save-item reconstruction;
- FPP, TPP, and preview proof;
- acquisition proof;
- persistence, missing-package, or migration proof;
- multi-package compatibility;
- release readiness.

`package_accepted`, `package_imported`, and `package_import_partially_queued` therefore remain current API statuses, not end-to-end completion states.

### 3.2 Implemented native registrar

The current registrar:

- requires the Unity thread;
- requires a matching accepted presentation definition;
- queues requests until `TemplatesProvider` exists and reports `AllLoaded`;
- rejects unsupported clone profiles;
- rejects changed definitions under an already observed registry key;
- checks custom GUID and template-name collisions;
- resolves and screens the source `ItemTemplate`;
- clones the source GameObject;
- applies a bounded set of identity, display, icon, description, and flavour fields;
- compares bounded component, attachment, and nested `TemplateReference` profiles;
- invokes `TemplatesLoader.AddToMap` once;
- verifies that provider lookup returns the inserted object;
- retains successful clones for the process session;
- marks the registrar terminally poisoned when failure occurs after native visibility.

This is a useful native-registration core. It is not, by itself, the complete importer.

### 3.3 Current consumer duplication

The Evil Greatsword consumer currently demonstrates the integration gap. It calls `RegisterWeaponPackage`, but it also retains local bundle loading, registration retry, native-registration fallback, merchant acquisition, and presentation-related fallback surfaces. Its bundle also contains an animation controller and preview/moveset content beyond the bounded rigid-weapon presentation envelope defined below.

The consumer is therefore a migration fixture, not proof that one-system/one-truth consolidation is complete.

## 4. Evidence boundary and current unknowns

### 4.1 Repository source evidence

Repository statements in this process are anchored to:

- `mods/tainted-weapons/src/Plugin.cs`;
- `mods/tainted-weapons/src/TaintedWeapons.csproj`;
- `mods/tainted-weapons/src/TaintedWeaponPackageImporter.cs`;
- `mods/tainted-weapons/src/TaintedWeaponNativeItemRegistrar.cs`;
- `mods/tainted-weapons/src/TaintedWeaponFramework.cs`;
- `mods/evil-greatsword-moveset/src/Plugin.cs`;
- `mods/evil-greatsword-moveset/tools/Build-EvilGreatswordMovesetBundle.ps1`;
- `mods/evil-greatsword-moveset/tools/unity/Assets/Editor/EvilGreatswordMovesetBundleBuilder.cs`;
- repository commit `7247f5c06520953572ee2f695ecd07e582faf725`.

### 4.2 Current native static evidence

A user-supplied `TG.Main.dll` candidate was inspected as static Mono/.NET evidence with this identity:

```text
file label: TG.Main(3).dll
sha256: 749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982
module: TG.Main.dll
MVID: 68528841-991c-481e-bd94-7f1776fc3579
assembly version: 0.0.0.0
metadata runtime: v4.0.30319
```

Static findings scoped only to that binary:

- `TemplatesProvider.AllLoaded` reflects its current loader's `FinishedLoading` state;
- title/loading flow starts template loading and waits for `AllLoaded` before later hero/equipment reconstruction paths continue;
- `TemplatesLoader` maintains a GUID map and a runtime-type multimap;
- `TemplatesLoader.AddToMap(string, ITemplate)` performs three sequential actions: GUID-map add, type-map add, then `ITemplate.GUID` assignment;
- that method has no transaction or rollback path in the inspected method body;
- `TemplateReference` persists a GUID and resolves through `TemplatesProvider`;
- Drake manager, renderer, authoring, and component types referenced by the inspected assemblies are owned by `Awaken.ECS`, not by `TG.Main`, `HLOD`, or `MeshToTerrain`.

These findings support static design constraints only. They do not prove runtime ordering, successful registration, presentation, save reconstruction, or compatibility.

### 4.3 Official Unity constraints

The process adopts these Unity 6 constraints:

- AssetBundles are target-platform specific;
- batch-mode bundle builds must set the target platform explicitly rather than relying on an editor platform switch during the build;
- bundle build failure can return `null` or throw;
- a bundle CRC can be extracted at build time;
- `AssetBundle.LoadFromFile(path, crc)` validates a non-zero CRC before loading and returns `null` on failure;
- `GetAllAssetNames()` exposes project-relative asset paths for normal, non-scene bundles;
- exact asset loading should use the complete relative asset path, including extension, and an exact expected type;
- `Unload(false)` detaches retained objects and can create duplicates after reload;
- `Unload(true)` destroys loaded objects and can invalidate live references;
- unloading a dependency while dependent content remains live can produce undefined behaviour.

Primary sources:

- <https://docs.unity3d.com/6000.0/Documentation/Manual/assetbundles-platforms.html>
- <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.BuildAssetBundles.html>
- <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/BuildPipeline.GetCRCForAssetBundle.html>
- <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetBundle.LoadFromFile.html>
- <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetBundle.GetAllAssetNames.html>
- <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetBundle.LoadAssetAsync.html>
- <https://docs.unity3d.com/6000.0/Documentation/ScriptReference/AssetBundle.Unload.html>

### 4.4 Hard blocker register

| Blocker | State | Exact consequence | Required evidence |
|---|---|---|---|
| Fresh current installed-game fingerprint | `NOT_RUN` | No current game-build support claim | Runtime fingerprint from the exact target installation |
| Matching `Awaken.ECS.dll` | `BLOCKED` | No exact Drake manager/components/lifetime contract | Matching binary hash, MVID, static inspection, then runtime proof |
| Central readiness hook before save reconstruction | `NOT_RUN` | No persistence-safe startup claim | Current-build startup trace with ordered markers |
| Rigid-weapon Drake conversion profile | `BLOCKED` | Presentation implementation cannot be promoted | Reviewed `Awaken.ECS` contract plus FPP/TPP/preview runtime evidence |
| Save/load/missing-package behaviour | `NOT_RUN` | No persistence or missing-content safety claim | Save matrix with preserved originals and ordered receipts |
| Multi-package operation | `NOT_RUN` | No compatibility claim | Exact package-order/collision/reload matrix |
| IL2CPP support | `NOT_APPLICABLE` for initial profile | No IL2CPP release | A separate reviewed IL2CPP design and evidence lane |

A blocked lane must not be replaced by a nearby static, runtime, or repository observation.

## 5. Initial supported product envelope

The first release-capable importer profile is intentionally narrow:

```text
profile: rigid-melee-weapon/v1
platform: WindowsPlayer
runtime: Mono
bundle target: StandaloneWindows64
native identity: cloned ItemTemplate
presentation source: rigid GameObject mesh/material asset
presentation targets: FPP + TPP + inventory preview
bundle lifetime: process-session immutable
native-template lifetime: process-session immutable
```

### 5.1 Included

- swords, axes, maces, daggers, and similar rigid melee weapons;
- one native source-template clone per declared weapon;
- one custom immutable 32-hex template GUID per weapon identity;
- one pure presentation prefab per weapon;
- static meshes and materials;
- bounded renderer hierarchies;
- package-declared placement offsets for each proven perspective;
- one importer-owned acquisition adapter selected from a reviewed set;
- explicit persistence and migration identity.

### 5.2 Excluded until separate profiles exist

- bows, crossbows, projectiles, quivers, and ammunition logic;
- shields or off-hand equipment;
- dual-wield pair coordination;
- skinned weapon rigs;
- weapon-local `Animator`, animation controllers, or movesets;
- combat-state, damage, perk, spell, VFX, SFX, or behaviour code;
- arbitrary `MonoBehaviour` components in content bundles;
- scenes or streamed-scene AssetBundles;
- runtime compilation or executable package payloads;
- hot unload, hot reload, or in-session package replacement;
- IL2CPP builds;
- cross-platform bundles;
- automatic conversion of unreviewed content profiles.

A package outside this envelope is `BLOCKED_PROFILE_UNSUPPORTED`; it is not partially imported.

## 6. Canonical lifecycle state machine

### 6.1 States

```text
DISCOVERED
  -> MANIFEST_VALIDATED
  -> FILESYSTEM_VALIDATED
  -> BUNDLE_VALIDATED
  -> ADMITTED
  -> ASSETS_READY
  -> NATIVE_REGISTERED
  -> PRESENTATION_PROVEN
  -> ACQUISITION_PROVEN
  -> PERSISTENCE_PROVEN
  -> COMPATIBILITY_PROVEN
  -> RELEASE_READY
```

Terminal or blocking states:

```text
REJECTED
BLOCKED_ENVIRONMENT
BLOCKED_PROFILE_UNSUPPORTED
BLOCKED_DEPENDENCY
MIGRATION_REQUIRED
MISSING_PACKAGE
TERMINAL_DIRTY_RESTART_REQUIRED
WITHDRAWN
```

### 6.2 Transition rule

A transition is valid only when:

1. the immediately preceding state has a passing receipt;
2. the receipt is bound to the exact environment, package, bundle, source, and implementation fingerprints;
3. every required negative control for that transition passed;
4. no conflicting receipt or blocker remains open;
5. the transition was performed by the owning implementation;
6. the receipt names its allowed and forbidden downstream uses.

A successful build cannot create `BUNDLE_VALIDATED`. `ADMITTED` cannot create `NATIVE_REGISTERED`. A visible FPP model cannot create `PRESENTATION_PROVEN`. A single save reload cannot create `PERSISTENCE_PROVEN`.

### 6.3 Receipt envelope

Every receipt uses this common envelope:

```json
{
  "receiptSchema": "tainted-weapons.receipt/1",
  "receiptId": "<stable-unique-id>",
  "stage": "<state-transition>",
  "status": "PASSED|FAILED|PARTIAL|BLOCKED|NOT_RUN|NOT_APPLICABLE",
  "recordedAtUtc": "<ISO-8601 UTC>",
  "repositoryCommit": "<40-hex>",
  "pluginVersion": "<version>",
  "environmentFingerprint": "sha256:<hex>",
  "packageIdentity": "<canonical package-id>@<version>",
  "weaponIdentity": "<canonical package-id>/<weapon-id>",
  "manifestSha256": "sha256:<hex>",
  "bundleSha256": "sha256:<hex>",
  "inputs": {},
  "checks": [],
  "failures": [],
  "allowedUsage": [],
  "forbiddenUsage": [],
  "nextGate": "<state-or-blocker>"
}
```

Receipts are append-only evidence records. A later run creates a new receipt; it does not rewrite a failed or obsolete receipt.

## 7. Environment fingerprint contract

Before package validation, the importer records one exact environment tuple:

```json
{
  "schema": "tainted-weapons.environment/1",
  "capturedAtUtc": "<ISO-8601 UTC>",
  "productName": "Fall of Avalon",
  "gameVersion": "<Application.version>",
  "unityVersion": "<Application.unityVersion>",
  "platform": "WindowsPlayer",
  "runtime": "Mono",
  "distribution": "<steam-or-other-reviewed-id>",
  "dlc": ["<stable-id>"],
  "tgMain": {
    "sha256": "sha256:<hex>",
    "mvid": "<uuid>"
  },
  "awakenEcs": {
    "sha256": "sha256:<hex>",
    "mvid": "<uuid>"
  },
  "bepInExVersion": "<version>",
  "harmonyVersion": "<version>",
  "taintedWeapons": {
    "repositoryCommit": "<40-hex>",
    "pluginVersion": "<version>",
    "dllSha256": "sha256:<hex>"
  },
  "configurationSha256": "sha256:<hex>"
}
```

Rules:

- Unknown required fields produce `BLOCKED_ENVIRONMENT`.
- A package compatibility declaration must match this tuple exactly unless a reviewed compatibility rule explicitly permits a range.
- A receipt from another tuple is historical only.
- Any change to game version, Unity version, relevant assembly hash/MVID, loader, Harmony, plugin binary, package manifest, bundle, DLC set, or material configuration reopens affected gates.

The historical repository observation `gameVersion=1.23.401`, `unityVersion=6000.0.64f1`, `WindowsPlayer` is not a current fingerprint. The existing Evil Greatsword build tool also pins Unity `6000.0.64f1`; this alignment is useful historical context but still requires a fresh target-installation capture.

## 8. Target package contract: `tainted-weapons.weapon-package/2`

Contract v2 is the required target contract. It is `NOT_IMPLEMENTED` at the repository baseline.

### 8.1 Package layout

```text
<package-root>/
  weapon-package.json
  bundles/
    windows-x64/
      <bundle-file>
  licenses/
    <license-and-attribution-files>
```

The content package contains no DLL or script. Any separately authorised gameplay plugin remains outside the content package and cannot bypass importer ownership.

### 8.2 Identity syntax

- `packageId`: lowercase NFKC-normalised ASCII, 3–128 characters, regex `^[a-z0-9]+(?:[.-][a-z0-9]+)*$`.
- `weaponId`: lowercase NFKC-normalised ASCII, 1–64 characters, same regex.
- canonical registry key: `packageId + "/" + weaponId`.
- `packageVersion`: SemVer 2.0.0.
- custom and source template GUIDs: exactly 32 lowercase hexadecimal characters, regex `^[0-9a-f]{32}$`.
- `customTemplateGuid` is immutable after first persistence-capable release.
- `customTemplateName` is deterministic and unique; changing it after release requires an explicit migration record.
- display text is not identity and cannot resolve collisions.

### 8.3 Required manifest

```json
{
  "schema": "tainted-weapons.weapon-package/2",
  "packageId": "kane.tgfoa.example-weapons",
  "packageVersion": "1.0.0",
  "weaponId": "example-longsword",
  "identity": {
    "customTemplateGuid": "11111111111111111111111111111111",
    "customTemplateName": "ItemTemplate_Mod_ExampleLongsword",
    "displayName": "Example Longsword",
    "description": "Example package used by the validation fixture.",
    "semanticIconAddress": "mod://kane.tgfoa.example-weapons/icon/example-longsword",
    "flavorText": ""
  },
  "sourceTemplate": {
    "guid": "22222222222222222222222222222222",
    "cloneProfile": "weapon-item-template-clone/v1"
  },
  "compatibility": {
    "platform": "WindowsPlayer",
    "runtime": "Mono",
    "gameVersion": "<exact-version>",
    "unityVersion": "<exact-version>",
    "tgMainSha256": "sha256:<64-lower-hex>",
    "awakenEcsSha256": "sha256:<64-lower-hex>"
  },
  "bundle": {
    "id": "presentation-windows-x64",
    "path": "bundles/windows-x64/example-weapons",
    "buildTarget": "StandaloneWindows64",
    "compression": "LZ4",
    "bytes": 123456,
    "sha256": "sha256:<64-lower-hex>",
    "crc32": "0x12345678",
    "dependencies": []
  },
  "assets": {
    "equippedPrefab": {
      "path": "assets/exampleweapons/generated/examplelongsword_weapon.prefab",
      "type": "UnityEngine.GameObject",
      "profile": "rigid-presentation-source/v1"
    }
  },
  "presentation": {
    "profile": "rigid-melee-weapon/v1",
    "firstPerson": {
      "position": [0.0, 0.0, 0.0],
      "rotationEuler": [0.0, 0.0, 0.0],
      "scale": [1.0, 1.0, 1.0]
    },
    "thirdPerson": {
      "position": [0.0, 0.0, 0.0],
      "rotationEuler": [0.0, 0.0, 0.0],
      "scale": [1.0, 1.0, 1.0]
    },
    "inventoryPreview": {
      "position": [0.0, 0.0, 0.0],
      "rotationEuler": [0.0, 0.0, 0.0],
      "scale": [1.0, 1.0, 1.0]
    }
  },
  "acquisition": {
    "profile": "debug-only/v1"
  },
  "persistence": {
    "identityVersion": 1,
    "aliases": []
  }
}
```

### 8.4 Schema rules

- UTF-8 without BOM.
- JSON object only; duplicate keys rejected before deserialisation.
- Unknown top-level and nested properties rejected for contract v2.
- Numbers must be finite; `NaN`, infinity, and exponent overflows rejected.
- Vector arrays contain exactly three finite decimal numbers.
- Path separators canonicalised to `/` in the manifest.
- Asset paths canonicalised to lowercase project-relative paths before comparison because Unity bundle asset names are project-relative and current Windows delivery is case-insensitive.
- Bundle `bytes`, SHA-256, and CRC must match the actual file.
- Empty dependency lists are explicit; omitted dependencies are invalid.
- Each manifest describes exactly one weapon for v2. Multi-weapon packs contain multiple manifests or a later reviewed contract version; v2 does not introduce an unreviewed nested-weapon identity model.
- The schema validator returns all deterministic errors in stable field/path order.

## 9. Source and authoring process

### Gate A0 — Source intake and rights

Required inputs:

- original asset locator;
- license and redistribution terms;
- source-file SHA-256 inventory;
- authoring owner;
- intended weapon profile;
- target source-template rationale;
- known modifications.

Pass conditions:

- every shipped asset has a traceable source and permitted redistribution state;
- source hashes are captured before conversion;
- unsupported or executable source payloads are excluded;
- the declared weapon profile is inside the supported envelope.

Failure state: `REJECTED_SOURCE_INTAKE`.

### Gate A1 — Authoring workspace

The build workspace must be disposable and generated from controlled inputs. It must contain:

- an exact Unity editor version matching the target environment;
- a committed builder source revision;
- a generated `ProjectVersion.txt` with exact editor version/revision;
- an explicit `-buildTarget StandaloneWindows64` command-line argument;
- no unpinned package-manager resolution;
- no user-specific absolute paths in the resulting manifest or release package;
- a clean generated asset root for each build.

The existing Evil Greatsword script is a useful starting fixture because it creates a temporary project and pins Unity `6000.0.64f1`, but it must be corrected to pass `-buildTarget StandaloneWindows64` explicitly and to emit the target v2 manifest/receipts before it can satisfy this gate.

### Gate A2 — Rigid presentation-source profile

For `rigid-presentation-source/v1`, the declared prefab must satisfy all of the following before bundling:

- root object local position `[0,0,0]`;
- root object local rotation identity;
- root object local scale `[1,1,1]`;
- at least one enabled `MeshRenderer` with a corresponding `MeshFilter`;
- no `SkinnedMeshRenderer`;
- no `Animator` or animation controller;
- no `Rigidbody`, collider, joint, camera, light, audio, particle, trail, or line-renderer component;
- no custom or missing `MonoBehaviour`;
- no embedded `ItemTemplate`, game template, addressable handle, or native-game state object;
- every mesh has finite vertices and non-zero finite bounds;
- each renderer's material count matches its mesh submesh requirements;
- no null mesh, material, texture, or shader reference;
- renderer, material, texture, triangle, and vertex totals remain under reviewed profile budgets;
- hierarchy depth and transform count remain under reviewed profile budgets;
- no scene asset is included;
- no animation/moveset asset is included;
- the content preview confirms the intended forward axis, grip origin, and physical scale.

Exact numeric budgets and shader allowlists remain blocked until the reviewed native/Drake fixture profile is established. Their absence blocks profile promotion; it does not permit unlimited content.

### Gate A3 — Bundle build

The build command must be equivalent to:

```powershell
Unity.exe `
  -batchmode `
  -noUpm `
  -quit `
  -buildTarget StandaloneWindows64 `
  -projectPath <temporary-project> `
  -executeMethod <reviewed-builder-entrypoint> `
  -bundleOutput <output-directory> `
  -bundleManifest <receipt-path> `
  -logFile <log-path>
```

The builder must:

1. fail on any Unity console error;
2. use `BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.StrictMode`;
3. target `StandaloneWindows64`;
4. reject a `null` build manifest;
5. confirm the exact expected bundle file exists;
6. extract CRC with `BuildPipeline.GetCRCForAssetBundle`;
7. enumerate exact built asset names;
8. record Unity version and builder source hash;
9. record bundle bytes and SHA-256;
10. write one build receipt atomically;
11. return a non-zero process exit code on failure.

### Gate A4 — Reproducibility check

Run two builds from independently cleared workspaces using identical inputs.

Pass requires:

- both builds pass A0–A3;
- source inventories match;
- editor and builder fingerprints match;
- bundle asset-name sets match exactly;
- declared dependency sets match exactly;
- component/profile inspection outputs match exactly;
- both bundle hashes and CRCs are recorded.

Byte-identical bundles are recorded when achieved but are not assumed. Different bytes or CRCs require an explained semantic comparison; unexplained divergence is `BLOCKED_NON_REPRODUCIBLE`.

## 10. Offline validation process

Offline validation is divided into a pure filesystem/schema validator and an isolated Unity bundle inspector. Neither mutates game state.

### Gate V0 — Manifest parse and canonicalisation

The pure validator:

1. reads the manifest with a fixed maximum byte size;
2. rejects invalid UTF-8, BOM, duplicate keys, comments, trailing data, and unknown properties;
3. validates every field against contract v2;
4. normalises IDs and paths once;
5. rejects values that change under required canonicalisation rather than silently rewriting release identity;
6. builds the canonical registry key;
7. serialises a canonical manifest representation;
8. computes `manifestSha256` over canonical UTF-8 bytes.

### Gate V1 — Filesystem boundary

For every path from the manifest:

- reject NUL, rooted, UNC, device, drive-relative, empty, `.` and `..` segments;
- calculate the full path and require containment under the package root;
- reject symbolic links, junctions, mount points, and other reparse points in the path chain;
- reject case-insensitive duplicate paths;
- reject alternate data streams;
- reject executable or script extensions anywhere in the content package;
- require the exact declared file set;
- reject undeclared files except reviewed license/attribution files;
- verify bytes and SHA-256 before any bundle load.

A validation failure never registers a definition and never loads the bundle in the game process.

### Gate V2 — Isolated Unity bundle inspection

The bundle inspector runs in a disposable process using the exact target Unity version and platform.

Ordered checks:

1. compute and compare the actual CRC;
2. call `AssetBundle.LoadFromFile(path, crc)`;
3. require a non-null bundle;
4. require a normal non-scene AssetBundle;
5. enumerate `GetAllAssetNames()`;
6. compare the exact canonical asset-name set with the manifest allowlist;
7. reject undeclared or duplicate canonical asset paths;
8. load the declared prefab by complete relative path and exact `UnityEngine.GameObject` type;
9. require one and only one compatible main asset;
10. traverse every object, component, renderer, mesh, material, texture, and shader;
11. execute the `rigid-presentation-source/v1` checks;
12. record deterministic counts, type names, paths, bounds, and dependency identities;
13. destroy loaded instances;
14. call `Unload(true)` only because the process is isolated and no live game references exist;
15. exit the process.

The runtime game process must not use validation unload behaviour as its production lifetime policy.

### Gate V3 — Negative fixtures

The validator fixture set must include at least:

- malformed JSON;
- duplicate JSON key;
- unknown property;
- invalid ID and GUID forms;
- rooted/UNC/traversal path;
- symlink/junction escape;
- forbidden executable payload;
- empty, oversized, wrong-hash, and wrong-CRC bundle;
- wrong platform bundle;
- scene bundle;
- undeclared asset;
- missing declared prefab;
- ambiguous short-name asset collision;
- wrong asset type;
- missing script;
- prohibited component;
- animated/skinned weapon under rigid profile;
- null mesh/material/shader;
- non-finite transform/bounds;
- over-budget asset;
- changed package identity under the same persistent GUID.

Each fixture must fail with one stable primary reason code and no game mutation.

## 11. Runtime admission process

### 11.1 Discovery window

Tainted Weapons discovers content manifests during plugin startup from reviewed package roots only. Discovery:

- is deterministic and sorted by canonical package ID, package version, then weapon ID;
- never executes package code;
- never follows reparse points;
- never loads bundles before environment, manifest, and filesystem validation pass;
- records rejected packages without attempting fallback registration.

### 11.2 Admission sequence

For each package, the importer performs this exact sequence:

```text
capture environment fingerprint
  -> V0 manifest validation
  -> V1 filesystem validation
  -> compatibility exact-match check
  -> V2-equivalent production bundle open with CRC
  -> exact asset-name and typed-prefab verification
  -> importer-owned bundle/asset registration
  -> presentation-definition registration
  -> native-registration request queue
  -> ADMITTED receipt
```

No native mutation occurs in this sequence.

### 11.3 Bundle/provider ownership

The importer owns one bundle record per `(bundleSha256, crc32, platform)` and one asset record per exact bundle asset path.

Production rules:

- load each admitted bundle once per process;
- load dependencies before dependants in a deterministic topological order;
- reject dependency cycles and missing dependencies;
- use complete asset paths and exact types;
- never expose raw `AssetBundle` ownership to consumers;
- expose immutable importer handles or resolved presentation records;
- retain admitted bundles and loaded presentation assets until FoA process exit;
- do not call `Unload(false)` or `Unload(true)` while any imported definition, prototype, inventory preview, equipped view, or dependency may reference the content;
- do not support hot replacement in the initial profile;
- a changed on-disk bundle after admission sets `MIGRATION_REQUIRED` or restart-required state; it is not reloaded in session.

The process-session retention policy is deliberate: current native template registration has no researched unregister path, and Unity bundle unload modes can detach, duplicate, destroy, or invalidate live content.

## 12. Native registration process

### 12.1 Single registration window

Release-capable native registration uses one central Tainted Weapons readiness hook. Polling from multiple consumer `Update` loops is not an accepted release path.

Target ordering:

1. Tainted Weapons installs one Harmony postfix on the exact `TemplatesLoader.FinishedLoading` setter identified for the fingerprinted build.
2. Packages are discovered, validated, admitted, and queued before the setter reports `true`.
3. When the setter receives `true`, the postfix processes the complete sorted native queue synchronously on the Unity thread before the setter returns.
4. The queue closes when processing ends.
5. Requests arriving after closure are rejected with `registration-window-closed-restart-required`.
6. Update-loop retry is disabled for release packages.

Required trace order:

```text
TW_IMPORT_DISCOVERY_BEGIN
TW_IMPORT_PACKAGE_ADMITTED
TW_IMPORT_NATIVE_QUEUE_SEALED
TW_IMPORT_TEMPLATES_READY_ENTER
TW_IMPORT_NATIVE_PREFLIGHT_PASS
TW_IMPORT_NATIVE_REGISTERED
TW_IMPORT_NATIVE_QUEUE_CLOSED
TW_IMPORT_TEMPLATES_READY_EXIT
<observed hero/save reconstruction markers>
```

The exact relation to game reconstruction remains `NOT_RUN` until captured on the current environment. If the hook is missing, altered, late, invoked more than once unexpectedly, or cannot prove queue closure before reconstruction, persistence-capable admission is blocked.

### 12.2 Preflight before `AddToMap`

Because the inspected `AddToMap` is non-transactional, every check that can be performed before mutation must be complete before invocation:

- correct Unity thread;
- registrar not disposed or terminally poisoned;
- queue window open;
- exact environment fingerprint still matches;
- definition and request identities match;
- manifest, bundle, and loaded asset handles remain valid;
- supported clone and presentation profiles;
- provider and loader resolved;
- provider reports all templates loaded;
- exact `AddToMap` method and required fields resolved;
- source template exists and passes the reviewed source-profile contract;
- custom GUID absent;
- custom template name absent;
- registry/public/mesh/material/prototype keys reserved without collision;
- definition hash has not changed;
- source component, attachment, and nested-reference captures are complete within budget;
- clone succeeds;
- every custom field can be applied;
- clone component, attachment, nested-reference, identity, and type checks pass;
- source object remains unchanged;
- no pending operation exists that could fail after map insertion.

Any failure here is mutation-free and returns `REJECTED_NATIVE_PREFLIGHT`.

### 12.3 Irreversible mutation boundary

The mutation boundary begins immediately before invoking:

```text
TemplatesLoader.AddToMap(customTemplateGuid, customTemplate)
```

The inspected binary performs sequential map/type/GUID mutation without rollback. Therefore:

- invoke exactly once;
- do not catch and continue as if the registrar were clean;
- do not destroy a clone that may have become visible through either map;
- do not attempt in-session unregister;
- do not process another package after uncertain or partial native visibility;
- require process restart after any post-invocation failure or uncertainty.

Outcomes:

| Observation | State |
|---|---|
| Invocation not reached | safe rejection; no native mutation |
| Invocation throws and exhaustive lookup proves no visibility | `REJECTED_NATIVE_INSERTION`; clone may be destroyed |
| Any map/provider lookup sees the custom identity | native mutation occurred |
| Provider lookup returns the exact clone and all postconditions pass | `NATIVE_REGISTERED` |
| Visibility exists but postcondition fails, or visibility cannot be proven absent | `TERMINAL_DIRTY_RESTART_REQUIRED` |

A terminal-dirty process blocks all further imported native registrations, acquisition, save writes, compatibility claims, and release proof.

### 12.4 Session immutability

After `NATIVE_REGISTERED`:

- identity, source template, definition hash, bundle hash, profile, and placement data are immutable for the process;
- the clone is retained until process exit;
- the bundle and required assets are retained until process exit;
- plugin disable/unload in the same process cannot remove the native identity safely and is reported as restart-required;
- definition changes require process restart and, after a persistence-capable release, a migration decision.

## 13. Source-template semantic profile

Component-list equality is necessary but not sufficient to establish semantic safety. Each supported source template requires a reviewed source-profile record containing:

- exact source template GUID, name, and runtime type;
- source game/assembly fingerprint;
- abstract/hidden/drop/stack flags;
- weapon category and hand/equip semantics;
- attachment type inventory and critical field values;
- nested `TemplateReference` inventory and meaning;
- item stats and combat-facing references that are inherited;
- FPP/TPP/preview presentation references that will be replaced or retained;
- acquisition/loot/economy implications;
- save-relevant fields;
- fields permitted to differ on the clone;
- fields required to remain identical;
- runtime fixture and negative control.

A source GUID is not accepted merely because it resolves to a visible, droppable `ItemTemplate`. Until the semantic profile is reviewed, the package is planning-only and native registration is blocked for release.

## 14. Presentation process

Presentation proof is three independent lanes. Passing one does not imply another.

### 14.1 Drake blocker

The exact Drake loading, conversion, components-manager, entity ownership, teardown, and rebuild contract resides in the matching `Awaken.ECS.dll`. The supplied `TG.Main`, HLOD, and MeshToTerrain assemblies contain references but do not define that contract.

Therefore the `rigid-melee-weapon/v1` Drake implementation remains `BLOCKED` until:

1. matching `Awaken.ECS.dll` identity is captured;
2. static owner methods, fields, and lifecycle are inspected;
3. the minimum supported conversion path is recorded;
4. a read-only runtime trace confirms manager availability and ordering;
5. a bounded prototype fixture passes FPP, TPP, preview, scene, rebuild, and cleanup tests;
6. independent review approves the exact contract.

No HLOD or archive-file observation may substitute for this evidence.

### 14.2 FPP gate

Required test matrix:

- draw from inventory;
- idle, walk, sprint, crouch, jump, block, light attack, heavy attack, hit reaction, sheath, and re-equip where applicable;
- weapon switch between imported and vanilla items;
- camera/FOV changes supported by the game;
- controller/body rebuild;
- scene transition;
- save reload with the weapon equipped.

Pass evidence:

- correct identity and prototype selected;
- exactly one visible imported weapon;
- vanilla source geometry not simultaneously visible;
- finite placement transform and expected hand/socket ownership;
- expected layer/culling behaviour;
- no missing renderer, material, mesh, shader, or ECS entity;
- no duplicate after repeated equip cycles;
- no leaked prototype or orphaned entity after unequip;
- captured visual receipt and ordered logs.

### 14.3 TPP gate

Run the equivalent third-person/body-visible matrix. FPP offsets cannot be reused without separate proof. The receipt must identify the owning body/hand view, render path, hierarchy/entity, transform, and rebuild behaviour.

### 14.4 Inventory-preview gate

Required checks:

- inventory list icon resolves;
- detail/preview model resolves where the game exposes one;
- preview camera layer and bounds are correct;
- repeated open/close and item switching do not duplicate or leak objects;
- preview does not consume or mutate the equipped instance;
- fallback icon/model behaviour is explicit and validated.

### 14.5 Presentation completion

`PRESENTATION_PROVEN` requires passing receipts for FPP, TPP, and inventory preview on the same environment, package, bundle, native identity, and implementation commit.

## 15. Acquisition process

A weapon that can only be created through an internal developer call is not release-complete.

Acquisition profiles are importer-owned adapters with separate receipts. Initial profiles may include:

- `debug-only/v1` — validation only; cannot satisfy release;
- `merchant-stock/v1` — exact reviewed shop identity and stock insertion lifecycle;
- `loot-table/v1` — exact reviewed loot owner and duplicate policy;
- `recipe/v1` — exact reviewed recipe/crafting identity;
- `quest-reward/v1` — requires separate quest/system ownership and is not implied by importer authority.

Rules:

- acquisition runs only after `NATIVE_REGISTERED`;
- it resolves the exact custom GUID through `TemplatesProvider`;
- it never clones or registers another template;
- it is idempotent under repeated UI/scene initialisation;
- it has a stable duplicate/stack/quantity policy;
- failure cannot fall back to an unregistered or source-template item;
- release requires at least one non-debug acquisition profile with runtime proof;
- quest or story integration requires the owning system's separate authority and validation.

The current Evil Greatsword merchant patch is evidence of a candidate acquisition route, not canonical importer ownership.

## 16. Persistence and missing-package process

### 16.1 Persistence identity

- the custom 32-hex template GUID is the persistent identity;
- package ID, weapon ID, custom template name, and display name do not replace it;
- GUID ownership is recorded before first release;
- one GUID has one active owner and one canonical definition per compatibility tuple;
- changing the source template without changing persistent identity requires reviewed migration proof;
- reusing a released GUID for a different weapon is prohibited.

### 16.2 Persistence ordering requirement

Custom native identities must be registered before any save item containing that GUID is resolved. Static ordering plausibility is insufficient. A release-capable startup trace must show:

```text
package admitted
native queue sealed
TemplatesLoader finished-loading hook entered
custom template registered
registration window closed
save/hero item reconstruction begins
custom GUID lookup succeeds
```

If this order cannot be proven, persistence remains `BLOCKED`.

### 16.3 Save matrix

Use disposable copies of named save fixtures. Preserve and hash originals before every test.

Required cases:

1. new game -> acquire -> inventory -> save -> exit -> reload;
2. acquire -> equip FPP -> save -> exit -> reload;
3. acquire -> equip TPP/body-visible -> save -> exit -> reload;
4. acquire multiple quantities where profile permits -> save/reload;
5. move between inventory/equipment/container where supported -> save/reload;
6. scene transition before save and after load;
7. game restart with identical package;
8. package patch with unchanged persistent identity and reviewed compatible definition;
9. package version requiring explicit migration;
10. package removed;
11. bundle corrupt or mismatched;
12. duplicate/colliding package installed;
13. Tainted Weapons disabled or missing;
14. load, then save again, then reload the second-generation save.

Each test records save hashes before and after, exact GUID resolution, item count/state, equipped presentation, logs, and any game-generated recovery behaviour.

### 16.4 Missing-package policy

Until a validated interception and recovery path exists, the fail-closed policy is:

- do not substitute the source template;
- do not silently drop the item;
- do not map the GUID to another weapon;
- do not fabricate a placeholder that can be saved as the original identity;
- do not overwrite the affected save as proof of recovery;
- preserve the original save unchanged;
- report the missing package ID, weapon ID, custom GUID, and required package version when determinable;
- block importer-owned acquisition and save-write claims for the affected session;
- require package restoration or an explicitly reviewed migration/recovery procedure.

The exact game-level mechanism for detecting and blocking unsafe load/save remains `NOT_RUN`; this policy defines the required outcome, not an unsupported claim that the current plugin already enforces it.

### 16.5 Migration contract

A migration record contains:

```json
{
  "schema": "tainted-weapons.identity-migration/1",
  "migrationId": "<stable-id>",
  "packageId": "<canonical-id>",
  "weaponId": "<canonical-id>",
  "fromPackageVersion": "<semver-range>",
  "toPackageVersion": "<semver>",
  "persistentGuid": "<32-lower-hex>",
  "fromDefinitionHash": "sha256:<hex>",
  "toDefinitionHash": "sha256:<hex>",
  "aliases": [],
  "requiredGameFingerprint": "sha256:<hex>",
  "preconditions": [],
  "actions": [],
  "rollback": [],
  "validationReceipts": []
}
```

No automatic migration runs without an exact record and passing save fixtures. A changed definition without a matching migration is `MIGRATION_REQUIRED`.

## 17. Compatibility process

Compatibility is tested after persistence for one package.

### 17.1 Package matrix

At minimum:

- one package;
- two independent packages;
- maximum supported package count and total asset budget;
- reversed discovery order;
- identical package installed twice;
- same package ID with different versions;
- same registry key with different definition hash;
- duplicate custom GUID;
- duplicate custom template name;
- duplicate bundle path with different hash;
- duplicate asset path in different bundles;
- dependency chain and dependency diamond;
- missing dependency;
- dependency cycle;
- one valid and one malformed package;
- one valid and one terminal-dirty registration fixture;
- package removal after a save contains both imported items;
- package update for one item while another remains unchanged.

### 17.2 Compatibility invariants

- discovery order cannot change canonical ownership;
- one rejected package cannot mutate or deregister an accepted package;
- preflight failures are isolated before native mutation;
- a terminal-dirty native mutation blocks all subsequent registrations and release proof for the process;
- every bundle and asset has one owner record;
- every custom GUID has one owner record;
- every runtime prototype/public/mesh/material key is collision-free;
- acquisition adapters cannot stock or grant another package's identity;
- saving one package's item cannot rewrite another package's identity;
- removal/missing-package diagnostics identify every affected identity.

### 17.3 External mod compatibility

Any mod that patches template loading, item equipment, `CharacterHandBase`, `CharacterWeapon`, Drake managers/components, inventory preview, acquisition surfaces, or save reconstruction requires a separate exact-version compatibility row. Absence of a known conflict is not compatibility proof.

## 18. Performance and resource budgets

Before release, define and enforce profile budgets for:

- manifest and file count;
- bundle bytes and dependency bytes;
- meshes, vertices, triangles, submeshes;
- renderers and materials;
- texture count, dimensions, formats, and estimated resident memory;
- hierarchy depth and transforms;
- bundle load time;
- asset validation time;
- native-registration time;
- prototype creation time;
- equipped-frame CPU/GPU cost;
- process-session retained memory;
- package-count scaling.

Until reviewed numeric budgets exist and are measured on the target environment, performance state is `NOT_RUN` and release remains blocked.

## 19. Security and containment rules

The importer is a content boundary, not a general plugin loader.

It must:

- reject executable and script payloads;
- reject path escapes and reparse points;
- reject unknown schema fields;
- cap manifest, string, file, bundle, asset, hierarchy, and resource sizes;
- avoid deserialising arbitrary CLR types from package data;
- load only exact declared assets by full path and expected Unity type;
- reject custom `MonoBehaviour` content for the rigid profile;
- never invoke package-provided methods;
- never allow packages to name arbitrary reflection targets;
- never expose native loader or raw bundle mutation APIs to consumers;
- sanitise receipt/log fields against line and delimiter injection;
- preserve hashes for all admitted files;
- fail closed on validation exceptions.

## 20. Required implementation work packages

This document does not authorise these work packages. They are the only ordered implementation route after separate planning and authorisation.

### W0 — Process review and promotion

Scope:

- independent domain review of this process;
- independent validation review;
- blocker verification;
- human promotion decision.

Exit: process becomes reviewed current owner authority for the exact approved scope.

### W1 — Contract v2 and pure validator

Scope:

- JSON schema/contract types;
- canonicalisation;
- filesystem containment;
- hash/size/identity checks;
- machine receipts;
- complete negative fixtures.

No Unity or native mutation.

### W2 — Unity builder and isolated bundle inspector

Scope:

- exact build target;
- CRC output;
- semantic build receipt;
- exact asset inventory;
- rigid source-profile inspector;
- isolated unload.

No game runtime registration.

### W3 — Importer-owned bundle/asset provider

Scope:

- deterministic load/dependency order;
- CRC load;
- typed exact-path assets;
- immutable handles;
- session-retention policy;
- collision receipts.

No native mutation.

### W4 — Central native registration window

Scope:

- one readiness hook;
- sealed sorted queue;
- exhaustive preflight;
- exact irreversible mutation policy;
- terminal-dirty state;
- startup trace markers.

No Drake promotion, acquisition, or persistence claim.

### W5 — Drake contract and presentation adapter

State: `BLOCKED` pending matching `Awaken.ECS.dll` and reviewed evidence.

Scope after unblock:

- exact manager/components lifecycle;
- rigid source conversion;
- FPP/TPP/preview adapters;
- process-session lifetime;
- rebuild/scene/equip tests.

### W6 — Acquisition adapters

Scope:

- importer-owned debug fixture;
- one production acquisition profile;
- idempotence and duplicate policy;
- receipts.

### W7 — Persistence, missing-package, and migration

Scope:

- startup ordering proof;
- save matrix;
- missing-package interception/recovery policy implementation;
- migration records and fixtures;
- original-save preservation.

### W8 — Multi-package compatibility and performance

Scope:

- package matrix;
- collision and dependency tests;
- resource budgets;
- performance receipts.

### W9 — Consumer migration

First fixture: Evil Greatsword.

Required result:

- split rigid weapon presentation content from animation/moveset scope;
- emit contract-v2 package;
- use only the canonical importer path;
- remove or permanently disable duplicate native registration, bundle lifetime, and presentation fallbacks;
- route acquisition through the selected importer adapter or separately authorised owner;
- preserve persistent identity only after save proof;
- retain negative tests proving the old path cannot activate.

Consumer migration occurs on the consumer's own canonical branch and PR after the central importer work is promoted. This Tainted Weapons process change does not authorise that mutation.

### W10 — Release gate

Scope:

- final source, binary, package, environment, and receipt fingerprints;
- build/plugin-load/runtime/presentation/acquisition/save/compatibility/performance/package checks;
- documentation and known limitations;
- independent release review;
- human release decision.

## 21. Current v1 migration policy

- Contract v1 remains an executable historical/current-development interface until replaced through reviewed implementation.
- No v1 `accepted`, `imported`, or `queued` result is re-labelled as contract-v2 or end-to-end proof.
- Feature expansion on v1 is frozen except bounded corrections required to protect current users or obtain evidence.
- Contract v2 is built in parallel behind explicit versioned entry points.
- A development-only v1-to-v2 adapter may be designed, but it cannot invent missing compatibility, CRC, component, persistence, or migration data.
- Release packages must pass the complete v2 lifecycle.
- Consumer fallback removal occurs only after its v2 replacement passes the same required lanes.

## 22. Review checklist for this process

The independent reviewer must verify:

- current repository baseline and version statements;
- current v1 capability and gap inventory against source;
- static binary fingerprint and method findings;
- separation of static, runtime, save, compatibility, and release evidence;
- exact ownership and no duplicate truth;
- supported/excluded profile boundary;
- contract-v2 field and identity rules;
- filesystem, bundle, and lifetime rules against official Unity behaviour;
- non-transactional native mutation handling;
- central startup-window design and its unproved status;
- Drake blocker and required `Awaken.ECS` evidence;
- persistence and missing-package fail-closed policy;
- implementation work-package ordering;
- consumer migration boundary;
- absence of implementation, runtime, compatibility, persistence, or release overclaim.

Review outcomes:

- `APPROVED_FOR_HUMAN_PROMOTION`;
- `CHANGES_REQUESTED`;
- `BLOCKED_EVIDENCE`;
- `REJECTED`.

Only a named human promotion owner may set this process to current authority.

## 23. Definition of done

### 23.1 Process-document completion

This documentation task is `PASSED` only when:

- the process is present on the exact `tainted-weapons` branch;
- README routing points to it without leaving older documents as competing current authority;
- the actual diff is documentation-only and owner-contained;
- baseline/version/fingerprint statements are rechecked;
- independent review is requested;
- a draft PR targets `main` from `tainted-weapons`;
- no merge, code, package, runtime, save, compatibility, promotion, or release claim is made.

The process authority remains `REVIEW_REQUIRED` until independent review and human promotion occur.

### 23.2 Importer implementation completion

The weapon importer itself is complete only when the final exact source, binary, environment, and package tuple has passing receipts for:

```text
MANIFEST_VALIDATED
FILESYSTEM_VALIDATED
BUNDLE_VALIDATED
ADMITTED
ASSETS_READY
NATIVE_REGISTERED
PRESENTATION_PROVEN
ACQUISITION_PROVEN
PERSISTENCE_PROVEN
COMPATIBILITY_PROVEN
RELEASE_READY
```

Any missing, failed, partial, stale, blocked, wrong-environment, or wrong-version lane prevents an importer-complete or release-ready statement.

## 24. Immediate next evidence task

```text
Next researched task: obtain the matching current-install Awaken.ECS.dll and a fresh runtime environment fingerprint, preserve their SHA-256/MVID identities, then inspect and independently review the exact Drake manager/components lifecycle before authorising W5 or any release-capable presentation profile.
```
