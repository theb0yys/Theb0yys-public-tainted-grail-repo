# Tainted Weapons

`0.3.20` continues the reusable Tainted Weapons framework for custom assets that must integrate through FoA's native item-template and Drake weapon presentation paths. It does not use 3DMigoto hashes, DDS files, `.ini` overrides, or hand-socket Unity renderer fallbacks.

The framework direction is template cloning plus Drake prototype rebinding: package definitions provide identity and cooked assets, the framework resolves assets, authors or clones a `CharacterHandBase` weapon prefab with `DrakeLodGroup`/`DrakeMeshRenderer`, and the vanilla `ItemEquip` plus `SetUnityRepresentation` lifecycle owns placement, rendering, hide/show, and teardown.

Current framework API:

- `TaintedWeaponsApi.RegisterWeaponDefinition(...)` accepts immutable weapon identity and cooked bundle/prefab references, then returns a registration result backed by a `TaintedWeaponsIdentityReceipt`.
- `TaintedWeaponsApi.RegisterNativeWeaponTemplate(...)` is the explicit provisional native-item entry point. It requires a matching accepted weapon definition, supports only `weapon-item-template-clone/v1`, queues before `TemplatesProvider` readiness, and returns a machine-readable `TaintedWeaponNativeItemRegistrationReceipt`.
- `TaintedWeaponsApi.TryGetNativeItemRegistrarStatus(...)` and `TryGetNativeItemRegistrationReceipt(...)` expose readiness and the latest per-weapon receipt without performing registration.
- `TaintedWeaponsIdentityPolicy` canonicalises pack/item identity, hashes the canonical definition with SHA-256, and atomically reserves the custom template GUID/name, registry key, runtime prototype address, mesh key, and material-key namespace.
- `TaintedWeaponNativeItemRegistrar` owns the current native clone path: exact provider readiness, source resolution, source/custom profile hashing, GUID/name collision denial, idempotency, changed-definition denial, one private `TemplatesLoader.AddToMap` insertion, provider lookup verification, and session-persistent teardown policy. The service is not auto-invoked by presentation definition registration.
- The asset resolver loads the package bundle and resolves the equipped prefab asset path.
- The equipped prototype adapter captures the vanilla `ItemEquip.GetHeroItem` prefab reference for registered custom templates, returns a stable framework `mod://` prototype address, clones the native `CharacterHandBase` prefab, activates the cloned `CharacterHandBase`/`DrakeLodGroup`/`DrakeMeshRenderer` presentation hierarchy, and rebinds the native `DrakeMeshRenderer` mesh/material references to framework-owned completed `AssetReference` objects. Runtime hand lifecycle hooks observe registered spawned `CharacterHandBase` instances at `ItemEquip.OnWeaponLoaded` and `CharacterHandBase.OnMount`, activate only that spawned hand's Drake presentation chain, then schedule a registered-only post-conversion Drake ECS readiness receipt. If Drake has already created the linked renderer entity but native loading has not published render IDs, the receipt path asks `DrakeRendererManager` to update its loading arrays and completes that existing registered renderer with the same native ready components once valid IDs are available, marks that renderer with `DrakeRendererManualTag`, marks the exact registered Drake mesh/material keys as retained, blocks unload calls for those retained registered keys so Drake does not release the framework resources immediately after readiness, and records a focused `visualProof` receipt with camera/frustum, ECS culling-flag, material/shader, world-position, native `Hero.TppActive`, FPP/TPP path classification, and camera culling-mask/layer evidence. A DrakeRendererLoadingManager-specific bridge serves only registered framework mesh/material keys and records native unload calls for lifecycle receipts while preserving Drake's counter and handle lifecycle for non-retained keys. Raw generic `Addressables.LoadAssetAsync<Mesh/Material>` hooks are disabled because they broke unrelated UI asset loads on Mono.
- The inventory/equipment preview adapter observes FoA's native `CharacterHandBase.AttachToCustomHeroClothes(CustomHeroClothes, ItemEquip)` clone boundary, records the preview owner path, camera mask, render layer visibility, Drake transform/bounds, ECS render-filter layer state, and whether the route is the separate inventory hero-renderer clone. It leaves visible vanilla two-handed preview clones read-only and only normalizes a registered custom preview clone when its Unity/ECS layer state mismatches the native `CustomHeroClothes` owner layer/mask contract.
- If prototype rebinding fails, the framework returns the original native source prefab instead of falling through to a broken framework address.

The plugin also contains a guarded read-only weapon material probe. Reports include ordinary Unity renderer texture rows plus Drake-backed weapon material rows resolved through already-loaded material keys. Use this only to gather exact renderer, material slot, shader, and texture-property evidence.

## Configuration

- `WeaponMaterialProbe.Enabled`: default `false`.
- `WeaponMaterialProbe.TriggerKey`: default `F9`; manual evidence capture.
- `WeaponMaterialProbe.AutoRunOnce`: default `false`.
- `WeaponMaterialProbe.InitialDelaySeconds`: default `8`.
- `WeaponMaterialProbe.TargetFilters`: default `Equipable_Weapon|Weapon_`.
- `WeaponMaterialProbe.TargetPriorityFilters`: default Longsword/Dull Longsword name variants; priority matches are written before broad weapon rows.
- `WeaponMaterialProbe.MaxRows`: default `160`; applies separately to renderer, Drake material, and component rows.
- `WeaponMaterialProbe.MaxMemberValues`: default `24`.
- `LifecycleValidation.Enabled`: default `false`; enables the W7-W10/W14-W17 receipt harness.
- `LifecycleValidation.RequestId`: default `0`; increment while enabled to write one `lifecycle-validation-*.tsv` report. Version `0.3.8+` reloads the plugin config when the file changes so live file edits can trigger this request gate.
- `LifecycleValidation.WriteOnSceneChange`: default `false`; writes a lifecycle report when the active scene changes.
- `LifecycleValidation.WriteOnShutdown`: default `false`; writes a before/after cleanup report when the plugin is destroyed.

Reports are written under:

```text
BepInEx/config/kane.tgfoa.tainted-weapons/
```

## Current Boundary

This version maps the framework, adds a runtime Drake prototype-rebinding path for registered weapons, records/completes post-conversion ECS readiness plus registered renderer retention and visual-proof evidence for the spawned registered weapon view through the existing Drake manager, and captures/repairs the registered inventory/equipment preview clone only at FoA's native preview-owner boundary. It does not:

- mutate inventory, combat, animation, or save state;
- edit shared materials;
- change shared renderer materials or texture properties.
- spawn direct Unity renderer weapon visuals.
- construct synthetic Drake ECS entities.
- patch global generic Addressables mesh/material loads.
- drive equip, unequip, scene travel, or UI screens for lifecycle validation.
- automatically invoke native item registration or migrate an existing consumer-local registrar.

Gate 7 is not passed by the `0.3.20` source/build alone. `template_registered` remains unavailable until an explicitly authorized live request produces an accepted native receipt, and T0-T14—including copied-save timing and missing-registrar behavior—remain required before registrar promotion. The inventory/equipment preview repair also remains pending live user-controlled visual confirmation after restart.
