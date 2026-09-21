# Design Notes

## Scope

- Requested behavior: build a heavier BepInEx/AssetBundle weapon texture mod and reject the 3DMigoto hash/DDS/ini route.
- Current slice: reusable `Tainted Weapons` framework mapping, canonical identity reservation, a first-class explicit native item registrar service, guarded read-only weapon material probe, runtime Drake prototype rebinding, a DrakeRendererLoadingManager-specific key bridge for registered custom weapons, registered post-conversion Drake ECS readiness receipts/completion and retention for existing linked renderer entities, focused visual-proof receipt fields, and a default-off lifecycle validation receipt harness.
- Files expected to change: `mods/tainted-weapons/**`.
- Risk level: Low for the probe, higher for equipped visual redirection and Drake authoring rebinding.

## Target

- Assembly: BepInEx plugin plus Unity runtime objects.
- Runtime object: loaded scene objects matching configured weapon filters, especially `Equipable_Weapon` paths.
- Evidence: local Template Diagnostics dump proves candidate weapon object paths but not material slots; the `20260803-233146` Tainted Weapons report proves gameplay weapon presentation objects and Drake-backed sword components, but not the exact Longsword/Dull Longsword material target.

## Framework Direction

- Public content contract: custom weapon packages submit immutable definitions and cooked assets; package code must not call Drake, ECS, template-loader internals, or vanilla weapon fields directly.
- Asset resolver: load framework/package bundles, resolve named prefab/mesh/material/texture/animation assets, and hand concrete assets to internal adapters.
- Native item registrar: accept only explicit weapon registration requests bound to an already accepted presentation definition; queue until `TemplatesProvider` readiness; clone an `ItemTemplate`; validate bounded component, attachment, nested-reference, identity, definition-hash, GUID/name collision, idempotency, changed-definition, and post-insertion lookup rules; keep acquisition and persistence outside this service.
- Drake integration route: author or clone a known-good `CharacterHandBase` weapon prefab with `DrakeLodGroup` and `DrakeMeshRenderer`, then rebind mesh/material references in the Drake authoring data before the vanilla equip path converts it.
- Equip lifecycle: keep `ItemEquip.OnWeaponLoaded` and whole-root `SetUnityRepresentation(linkedLifetime=true, movable=true)` as the owner of instantiate, socket placement, `World.BindView`, character attachment, Drake entity creation, hide/show, and teardown. The framework may observe registered spawned hand views, activate their Drake presentation chain, ask the native Drake manager to update completed loading arrays, complete their existing linked renderer entity with native `MaterialMeshInfo`, mipmap, UV, and spawned-tag components once valid IDs are available, mark that existing registered renderer with `DrakeRendererManualTag`, retain the exact registered Drake mesh/material keys used by that renderer, skip unload calls only for those retained registered keys, and append read-only camera/frustum, ECS visibility/culling, material/shader, and world-position evidence to the runtime ECS receipt; it must not bypass equip with a socket-level Unity renderer fallback.
- Disabled paths: no direct Unity `MeshRenderer` fallback for equipped weapons, and no synthetic Drake entity construction until the complete entity schema is proven.

## Implemented Framework Slice

- `TaintedWeaponsApi.RegisterWeaponDefinition(...)` is the public package entry point.
- `TaintedWeaponsIdentityPolicy` normalises public pack/item IDs, hashes the canonical definition with SHA-256, and reserves custom template GUID/name, registry key, runtime prototype address, mesh key, and material-key namespace before framework records are stored.
- Identity receipts expose the legacy 64-character `definitionHash` and the algorithm-labelled `canonicalDefinitionHash=sha256:<hash>`; changed-definition public-ID collisions report the SHA-256 algorithm plus existing and candidate canonical definition hashes.
- Capability receipts now separate the bounded presentation hook aggregate from importer-stage claims: `PresentationReady` gates the current Tainted Weapons presentation lane, while `ImporterReady` remains fail-closed until package, provider, native-template, equip, persistence, compatibility, and release receipts are bound.
- Startup logs emit machine-readable capability state rows for package, provider, template, presentation, equip, persistence, compatibility, and release using the G4/G5 vocabulary.
- `TaintedWeaponDefinition` carries immutable package id, weapon id, custom/source template GUIDs, display text, package root, bundle name, and equipped prefab asset path.
- `TaintedWeaponNativeItemRegistrar` is a framework-owned service in the main `src` folder. Its public request/receipt/status contracts are reached through `TaintedWeaponsApi`; presentation registration never calls it automatically. The exact `TemplatesLoader.FinishedLoading` postfix only drains explicitly queued requests, and a throttled pending-only fallback covers a missed readiness event.
- The registrar implements one weapon-only profile, `weapon-item-template-clone/v1`. It resolves all native template-name candidates through `TemplatesProvider.AllTemplates`, denies GUID/name and changed-definition collisions before insertion, validates clone component/attachment/`TemplateReference` profiles under fixed limits, invokes the exact private `AddToMap(string, ITemplate)` at most once, and verifies the same custom object through provider lookup afterward.
- There is no researched native unregister route. Successfully inserted clones therefore remain session-persistent until process exit; plugin teardown does not destroy a clone still owned by the native maps.
- `TaintedWeaponAssetResolver` loads the cooked package bundle and resolves the equipped prefab by exact asset path.
- `TaintedWeaponDrakePrototypeAdapter` captures the native equipped prefab reference selected by `ItemEquip.GetHeroItem`, clones that native prefab under a framework-owned inactive storage root, and rebinds the single native `DrakeMeshRenderer` through `DrakeMeshRenderer.Setup(...)` using framework-owned completed `AssetReference` objects for the package mesh/materials. `TaintedWeaponDrakeLoadingManagerBridge` then serves only those registered framework mesh/material keys from `DrakeRendererLoadingManager.StartLoadingMesh/StartLoadingMaterial`, records `UnloadMesh/UnloadMaterial` registered-key counter receipts, and blocks unload only for retained registered keys after the runtime renderer has completed. The runtime ECS receipt calls the native `DrakeRendererManager.ComponentsManager.UpdateLoadings()`/`TryGetMaterialMesh(...)` path and adds the same ready components plus `DrakeRendererManualTag` to the already-linked registered renderer entity only after native IDs exist, then marks that renderer's exact registered mesh/material keys as retained and appends read-only visual proof for camera/frustum, ECS culling flags, material/shader validity, and world/bounds positions. The raw generic `AssetReference.LoadAssetAsync<Mesh/Material>` and `Addressables.LoadAssetAsync<Mesh/Material>` hook route is quarantined after live UI logs proved closed generic hooks can bleed into unrelated TextAsset/Sprite/GameObject loads on Mono.
- `LifecycleValidation` config writes default-off TSV receipts with summary snapshots, per-record counters, Drake loading-manager rows, and lifecycle events. Because the native title-screen `Awaken.TG.Main.UI.TitleScreen.TitleScreenUI.Exit()` path kills the process before normal Unity/BepInEx teardown, the framework patches that exact parameterless exit with a non-skipping prefix that writes the configured before/after teardown receipt first; `Plugin.OnDestroy` remains the fallback and the write is idempotent. The harness observes manual or native fixture actions; it does not equip, unequip, grant items, travel scenes, open UI screens, control the game process, mutate saves, or invoke native registration.
- Registered definitions log `assetResolved`, `drakeReady`, `reason`, and custom mesh/material source details. Runtime equip logs the source redirect, prototype handle route, and built prototype Drake authoring details.
- Failure behavior for equipped visuals is fail-closed: if custom prototype build fails after redirect, the framework returns the original native source prefab handle instead of creating a direct Unity renderer fallback.
- Failure behavior for native registration is fail-closed: missing provider/source/profile/reflection evidence, wrong thread, collisions, changed definitions, or profile mismatch return a denial receipt with `addToMapInvocationCount=0` and `nativeMutationOccurred=false`. An exception after a partial native map mutation is reported distinctly and the mapped clone is not destroyed.

## Approach

- Patch type or plugin behavior: non-skipping Harmony postfix on the exact `TemplatesLoader.FinishedLoading` setter for explicit registrar queue readiness; Harmony postfix on `ItemEquip.GetHeroItem`, prefix on `ARAssetReference.LoadAsset<GameObject>` for registered framework weapon prototypes, and prefixes on `DrakeRendererLoadingManager.StartLoadingMesh/StartLoadingMaterial` for registered framework mesh/material keys only; optional manual fallback plus automatic scene-aware probe. Auto-run skips `BuildInitialScene` and `TitleScreen`, schedules its delay from the current active scene, retries after empty gameplay reports, and only marks itself complete after finding candidate rows or reaching the bounded retry cap.
- Target priority behavior: `TargetPriorityFilters` are scanned before broad `TargetFilters`. When priority filters are configured, auto-run writes broad evidence but continues retrying until priority rows are captured or the retry cap is reached. This prevents arrows, VFX, or NPC Gladius rows from ending the evidence pass before Longsword/Dull Longsword appears.
- Drake probe route: when a matching `DrakeMeshRenderer` is present, read its mesh reference, material runtime keys, already-loaded material, shader, and texture properties. Do not start Addressables loads, register runtime materials, or replace Drake materials in this evidence slice.
- Config entries: probe enablement, trigger key, auto-run, delay, target filters, target-priority filters, row limits, member limits.
- Save impact: none.
- Compatibility considerations: read-only scene scan remains bounded; equipped visual redirection is limited to registered custom template GUIDs; no shared material mutation, inventory writes, combat edits, animation edits, or synthetic Drake ECS construction.
- Failure behavior: if no matching renderer/material rows are found, write bounded empty reports and keep retrying; after the retry cap, log that no replacement target is proven.
- UI route: none.

## Later AssetBundle Slice

After the probe proves a target, the implementation should:

- load a cooked Windows AssetBundle from the plugin directory;
- validate the expected texture asset exists;
- match the exact renderer/material slot/profile from the probe report;
- clone the renderer material locally with `HideFlags.DontSave`;
- set only the proven texture properties, expected candidates being base/diffuse, metallic/mask, and normal after runtime slot evidence confirms the game shader names;
- restore original renderer materials on disable/unload.

## Review Requirements

- Code review required: yes.
- Second review required: yes before enabling any texture override.
- Reason: visual replacement touches renderer/material state and can corrupt shared assets if scoped incorrectly.
