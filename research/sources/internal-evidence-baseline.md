# Internal Evidence Intake Baseline

> **Snapshot:** 2026-09-20  
> **Private evidence snapshot inspected:** repository commit `a0c1fbc69c5154baf8f6d2dc0744351d748e9a28`  
> **Purpose:** publish useful FoA technical facts that were stronger or unavailable in the public-source baseline without publishing private paths, proprietary binaries, bulk decompiled source, private logs or private repository content.

This page records **provenance and evidence strength**, not an additional truth surface. Canonical facts belong in `knowledge/`.

## Evidence lanes used

### Preserved developer lifecycle documentation

The private corpus preserves developer-supplied documentation for FoA's MVC model/element/event lifecycle. Public-safe derived facts include:

- `World.Add(model)` lifecycle ordering;
- deferred Element initialization when the parent is not fully initialized;
- Model discard ordering and owner-based listener cleanup;
- `World.All<T>()`, `model.Element<T>()`, `model.Elements<T>()`, `element.ParentModel`, and `World.View<T>(model)`;
- `Model.Events` lifecycle events;
- target-scoped, limited and any-source event-listening patterns.

The preserved document explicitly leaves saved-Model restoration undocumented.

### Exact Mono decompilation/static evidence

A current Mono installation was fingerprinted and selected managed contracts were inspected with ILSpy.

| Field | Recorded value |
| --- | --- |
| Steam build | `24270691` |
| Runtime lane | Mono |
| `TG.Main.dll` SHA-256 | `749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982` |
| `TG.Main.dll` MVID | `68528841-991C-481E-BD94-7F1776FC3579` |
| `TG.Main.dll` size | `9,058,304` bytes |
| BepInEx | `5.4.23.3` |
| Harmony | `2.9.0.0` |
| Unity | `6000.0.41.4645959` |
| Decompiler | `ilspycmd 10.1.0.8386` |

This lane establishes static contracts only. It does not by itself prove runtime ordering, save round trips, mutation safety or IL2CPP equivalence.

## Stronger-than-public findings

The following public-baseline facts gained stronger private evidence:

| Subject | Stronger evidence |
| --- | --- |
| `HeroRPGStats.AfterHeroFullyInitialized` | Exact decompilation shows `HeroRPGStats.OnInitialize()` initializes its wrapper and registers `AfterHeroFullyInitialized` on the parent Hero's fully-initialized callback |
| `Stat` / `LimitedStat` | Exact value/save semantics: saved base value is distinct from tweak-derived modified value |
| `StatTweak` / `TweakSystem` | Exact constructors, operations, priorities, recalculation and discard cleanup were inspected |
| `TemplatesLoader.FinishedLoading` | Exact template-load sequence shows the flag is set after both `template` and `templateSO` passes |
| `TemplatesProvider` | Exact `AllLoaded`, typed GUID lookup and enumeration surfaces were inspected |
| `TemplateReference` | Exact GUID-based resolution chain through `TemplatesUtil.Load<T>()` and `TemplatesProvider` |
| `SceneService` | Main/additive/active scene relationships and additional readiness/lifecycle surfaces were mapped |
| assembly ownership | Current Mono DLL hashes/MVIDs establish which managed assembly owns major FoA systems |

## Project-inspected general hook surfaces

The private engineering corpus also provides source-inspected implementations for several generally useful patch points:

| Target | Evidence | Public-safe conclusion |
| --- | --- | --- |
| `LockpickingInteraction.ConsumePickHP(float)` | exact Harmony target in project source | narrow lockpick-durability consumption seam |
| `Shop.OpenShop` | exact Harmony Prefix in project source | merchant-open seam used before normal shop flow continues; runtime owner validation remains incomplete |
| `VCCharacterMagicVFX.CastingBegun` | exact Harmony Postfix in project source | character magic-cast presentation seam, filtered to player-owned casts by the implementation |
| `TemplatesLoader.set_FinishedLoading(bool)` | multi-consumer source inspection plus exact loader decompilation | template-readiness retry seam after native template loading |
| concrete `SteamCloudService` / `SteamNoCloudService` / `DebugCloudService` / `GogCloudService.EndSave(string)` | exact Harmony target set plus decompiled save-service surface | completed native slot-write observation seam; not a custom serializer contract |

These entries are reference targets, not blanket compatibility or runtime-safety guarantees.

## Additional private-only general reference findings

These are useful enough for public intake because they save repeated reverse engineering:

- `World.All<T>()` for broad model enumeration;
- `model.Element<T>()` and `model.Elements<T>()`;
- `element.ParentModel`;
- `World.View<T>(model)`;
- `Model.Events.BeforeFullyInitialized`, `AfterFullyInitialized`, `AfterChanged`, `BeforeDiscarded`, `BeingDiscarded`, `AfterDiscarded`, `AfterElementsCollectionModified`;
- `ListenToLimited(...)`;
- `TemplatesProvider.GetAllOfType<T>()`;
- `Stat.BaseValue`, `ModifiedValue`, `ValueForSave`, `SetTo`, `IncreaseBy`;
- `Model.MarkedNotSaved` / `IsNotSaved` and save-preparation exclusion;
- `SaveWriter.WriteTemplate<T>` → saved template GUID;
- `SaveReader.ReadTemplate<T>` → `TemplatesUtil.Load<T>()` → `TemplatesProvider.Get<T>(guid)`;
- `LoadSave.CanAutoSave()`, `LoadSave.Save(...)`, `LoadSave.QuickSave()`;
- concrete `CloudService.EndSave(string)` implementations as completed-slot-write observation seams;
- later scene-readiness surfaces beyond domain identity, including `EverythingInitialized`, `AfterSceneFullyInitialized`, `AfterSceneStoriesExecuted`, and `SafeAfterSceneChanged`.

## Public-safe publication rule

Derived facts from internal evidence may be published when they are useful to modders and can be expressed without redistributing proprietary code or private material.

When publishing:

1. state the runtime/build scope;
2. distinguish developer documentation, static/decompiled evidence, source-inspected mod evidence and runtime observation;
3. publish names, signatures, lifecycle relationships and bounded behavior—not bulk decompiled bodies;
4. omit private filesystem paths, private logs, credentials, saves and proprietary binaries;
5. do not upgrade static evidence into runtime/save/compatibility claims.

See [Evidence Standards](../../contributing/evidence-standards.md).
