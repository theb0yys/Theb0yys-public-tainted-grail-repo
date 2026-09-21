# Mysterious Merchant at Bonfires — cleaned report derivative

Intake date: 2026-09-11. Source: user-provided Deep Research report, attachment `befa6252-8941-4d8b-a9a8-f7b68fa10bb5`. [Original report text](original-report.md), [source ledger](sources.json), [intake receipt](intake.json), [intake findings](README.md).

The original attachment bytes are preserved as Base64 in `intake.json` at `source.raw_bytes_base64`, with the original size and SHA-256. The readable original and derivative use LF line endings for Git; normalization changes no report wording.

**Authority remains E1 research context.** This document retains the returned report's narrative and stated gate labels; those labels are the report author's assessments, not repository gate approvals. No implementation, installed-build equivalence, new static gate, runtime validation or promotion is established by intake.

Thirty first-party source files at toolkit commit `073bdab3e09d6adad5003339fc49b021738d71e6` were opened for source comparison. Conversation citations were replaced where their claims match those inspected files. These are newly established replacement citations, not a recovered original citation map. Unresolved markers remain explicit and every original token is retained in `intake.json`; do not use unresolved claims as authority. GitHub release metadata confirms the stated tag/date and reports the archive digest; the archive itself was not downloaded or hashed.

Intake qualification: the toolkit's `Shop.RestockCurrency` sets wealth to `float.MaxValue`; the presence of max-wealth/restock fields does not establish a finite merchant budget. One persistent merchant per save remains a hypothesis. Stock policy, prices, selling/buyback and replacing startup grants remain owner decisions, not missing APIs. The local repository commit is present, while the connector returned GitHub 422 for that same SHA; neither fact replaces the other.

The report narrative follows, with citation normalization only.


## Executive answer

**Overall result: PARTIAL. Implementation remains BLOCKED. Static and runtime gates remain NOT_RUN.**

The public evidence does establish a coherent, first-party **native ownership path** for a dedicated merchant. The strongest source-backed design to take into later static review is **not** a detached `Shop`, a cloned existing merchant, or a manually driven `ShopUI`. It is a dedicated native `Location` whose `LocationSpec` owns a `ShopAttachment`, whose attachment references a dedicated `ShopTemplate`; the resulting `Shop` is then opened exclusively through `Shop.OpenShop()`. AR-Questline's source explicitly models `Shop` as `Element<Location>` and `IRefreshedByAttachment<ShopAttachment>`, and `ShopAttachment` is the object that spawns `new Shop()` from a `LocationSpec`. [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) [ShopAttachment](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/ShopAttachment.cs)

There is also a first-party route for making that owner a **runtime-created, saveable Location** rather than modifying a physical bonfire. `LocationTemplate.SpawnLocation()` creates a `RuntimeLocationInitializer`; that initializer is explicitly serializable, has `ShouldBeSaved => true`, stores its `LocationTemplate`, transform and optional overridden location name, and `LocationCreator` gives the runtime location its own generated model ID. This matters because `ShopUI.MerchantName` is derived from the Shop's owning Location, so a dedicated location can own the text **“Mysterious Merchant”** without changing a bonfire's own identity. [LocationTemplate](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Setup/LocationTemplate.cs) [RuntimeLocationInitializer](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/RuntimeLocationInitializer.cs) [LocationCreator](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/LocationCreator.cs) [ShopUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/UI/ShopUI.cs)

Crucially, AR-Questline's official mod-content machinery supplies a plausible native template-registration path rather than requiring an invented runtime template store. The toolkit's editor code converts templates into Addressables with the `template` or `templateSO` labels; its template mapping explicitly assigns `LocationTemplate` to the location template group and `ShopTemplate` to a template group. At runtime, `ModManager` discovers mod content catalogs, `Mod` loads them with `Addressables.LoadContentCatalogAsync`, and `ModService` is registered before `TemplatesProvider`. `TemplatesLoader` then enumerates the labelled Addressable locations and builds the GUID/type maps from them. [Authoring](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Editor/Assets/Templates/AddressableTemplatesCreator.cs) [Mapping](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesToAddressablesMapping.cs) [Mod](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Utility/Assets/Modding/Mod.cs) [ModManager](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Utility/Assets/Modding/ModManager.cs) [ApplicationScene](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Scenes/SceneConstructors/ApplicationScene.cs) [Loader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesLoader.cs)

That is substantially stronger evidence than constructing an arbitrary runtime `ShopTemplate`. Template save data is GUID-based: `SaveWriter.WriteTemplate` writes `value?.GUID`, while `SaveReader.ReadTemplate` resolves that GUID through `TemplatesUtil.Load<T>()`; `TemplateReference` follows the same GUID → `TemplatesProvider` route. [SaveWriter](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/MVC/Serialization/SaveWriter.Special.cs) [SaveReader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/MVC/Serialization/SaveReader.Special.cs) [unresolved citation: turn48file0] [TemplatesUtil](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesUtil.cs)

However, this **does not yet prove compatibility with the brief's pinned installed build**. The first-party source examined is the official `merlin-workshop` v1.1.0 source snapshot at commit `073bdab3e09d6adad5003339fc49b021738d71e6`, published on 6 February 2026. Its release archive is `merlin-workshop-v1.1.0.7z`, SHA-256 `dca167843cc22d720e7ec62a1eaef1d6c9d07656f0584b48d03c5ee18ba6aacb`. AR-Questline identifies that repository as the game's modding toolkit. [unresolved citation: turn71file0] [unresolved citation: turn76file0] [unresolved citation: turn77file0] The brief's installed `TG.Main.dll` SHA-256 `749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982` remains a **brief-supplied, separate artifact identity**; no public mapping establishing byte/API equivalence between that DLL and toolkit v1.1.0 was found.

Accordingly, the answer to the immediate feasibility question is:

> **A supported information path exists:** dedicated native Location ownership → native Shop attachment → registered ShopTemplate → native Shop.OpenShop → native pricing/trading/stock compression → native ShopUI close.  
> **A deployable implementation path is not yet proven:** exact pinned-build API parity, custom template/catalog resolution in that installation, cross-scene ownership, save restoration, bonfire/shop forced-close coordination, C0 build ownership and actual six-weapon transaction persistence still require the brief's static and runtime evidence lanes.

The research did **not** identify a durable independent third-party FoA source implementation that creates a dedicated native `Shop`; the inspectable code-search results were overwhelmingly AR-Questline's first-party source plus material from this repository. That means the first-party code is a strong source group, but it should not be misrepresented as multiple independent implementations. [unresolved citation: turn68file0] [unresolved citation: turn68file8] [unresolved citation: turn68file11]

## Source authority and applicability

| Source | Exact identity | What it establishes | Applicability limit |
|---|---|---|---|
| Brief-supplied installed game assembly | `TG.Main.dll`, SHA-256 `749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982` | The installation against which later static/runtime proof must bind | Not inspected or rehashed in this Deep Research execution; no claim that public toolkit source is identical |
| AR-Questline `merlin-workshop` | Commit `073bdab3e09d6adad5003339fc49b021738d71e6`, message “Update with new game content” | First-party source for `Shop`, `Location`, templates, transactions, bonfire UI, persistence and mod Addressables | Architectural/API evidence, not a substitute for decompilation of the pinned DLL [unresolved citation: turn77file0] |
| AR-Questline toolkit release | v1.1.0, published 6 February 2026; archive SHA-256 `dca167843cc22d720e7ec62a1eaef1d6c9d07656f0584b48d03c5ee18ba6aacb` | Versioned public toolkit artifact associated with the examined snapshot | No public evidence binds this archive to Steam build `24270691` or the brief's DLL hash [unresolved citation: turn76file0] |
| AR-Questline shop source | `Shop`, `ShopAttachment`, `ShopTemplate`, stock classes, `ShopUI`, trade classes | Native ownership, stock, transaction, price and close semantics | Must be confirmed against pinned DLL before implementation [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) [ShopTemplate](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/ShopTemplate.cs) |
| AR-Questline location source | `Location`, `RuntimeLocationInitializer`, `LocationCreator`, `LocationTemplate` | Native persistent runtime-location construction and display-name ownership | Does not by itself prove a merchant should be spawned once, per scene or per save in the pinned build [Location](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Location.cs) [RuntimeLocationInitializer](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/RuntimeLocationInitializer.cs) |
| AR-Questline template/mod source | `TemplatesProvider`, `TemplatesLoader`, `AddressableTemplatesCreator`, `Mod`, `ModManager` | First-party content-catalog route for GUID-addressed templates | Whether this route can be introduced without violating the Tainted Weapons C0 binary lock is a repository/build-ownership question, not resolved here [Provider](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesProvider.cs) [Loader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesLoader.cs) |
| Brief's repository pin | `theb0yys/Tainted-Grail-The-Fall-of-Avalon-mods` at `1a30867675ceef1ddfdb5e86cbb41ffb43ccb3d2` | Intended process/source boundary supplied by the owner | **External verification PARTIAL:** direct connected-GitHub lookup returned `No commit found for the ref`; the currently searchable public repository is newer, so I did not silently substitute current HEAD for the supplied pin |
| Current public repository material | Search results at newer public commit `1cc85a1b3cd05839c81c72372b1c8cd12679676f` | Useful non-authoritative corroboration that this repository has already encountered template-registration and decompressed-stock boundaries | Not claim-fit evidence for the supplied `1a308…` checkout; kept separate from pin-bound claims [unresolved citation: turn66file0] [unresolved citation: turn67file0] |

The inability to resolve `1a308…` matters. It means the brief's P1/P2/S1/S2/S3/C1 links could not all be independently reopened at their exact commit during this execution. The narrow facts supplied in the brief remain useful as **owner-provided E1/static context**, but I have not converted current public HEAD into evidence for that historical checkout.

### First-party template registration is more complete than a runtime GUID alone

This is an important finding because it narrows the persistence problem.

`TemplateReference` serializes a GUID and resolves it through `TemplatesUtil.Load<T>()`; the runtime branch of `TemplatesUtil.Load<T>` calls `World.Services.Get<TemplatesProvider>().Get<T>(guid)`. `TemplatesProvider` has lookup/enumeration APIs but no public runtime `Add` method. [unresolved citation: turn48file0] [TemplatesUtil](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesUtil.cs) [Provider](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesProvider.cs)

`TemplatesLoader`, in turn, fills its GUID and type maps by enumerating Addressables carrying `template` and `templateSO`, loading each one, and assigning the resulting template's GUID. [Loader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesLoader.cs) The toolkit editor contains the corresponding authoring side: `AddressableTemplatesCreator` assigns those labels and a GUID-bearing address, while `TemplatesToAddressablesMapping` explicitly recognises both `LocationTemplate` and `ShopTemplate`. [Authoring](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Editor/Assets/Templates/AddressableTemplatesCreator.cs) [Mapping](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesToAddressablesMapping.cs)

The mod loader is designed to make extra Addressable catalogs visible: `ModManager` scans the game's Mods directory, each `Mod` loads its JSON content catalog, active mod resource locators are added to Addressables, and application setup registers `ModService` before `TemplatesProvider`. [ModManager](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Utility/Assets/Modding/ModManager.cs) [Mod](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Utility/Assets/Modding/Mod.cs) [ApplicationScene](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Scenes/SceneConstructors/ApplicationScene.cs)

So **public first-party source does contain a native content-registration model capable in principle of giving a custom LocationTemplate and ShopTemplate durable GUID identities**. That is materially different from a BepInEx plugin creating an anonymous `ScriptableObject` after template loading.

The repository's newer public research independently records the same practical boundary: its later food/drink work reports that `TemplatesProvider` exposes lookup only and that its experimental runtime-registration route invoked the loader's private map insertion. That is useful corroborating context, but because it is from newer repository state and local decompilation it is not promoted here as proof for the present pin. [unresolved citation: turn66file0] [unresolved citation: turn66file1]

## Native shop ownership, stock and persistence contract

### The supported owner is a Location

The native object graph is unambiguous in first-party source:

`Location`  
→ attachment tracking from its `LocationSpec`  
→ `ShopAttachment`  
→ `Shop : Element<Location>`  
→ `ShopTemplate`  
→ `UniqueStock`, `RestockableStock`, `BoughtFromHeroStock`  
→ `ShopUI`

`Location.OnInitialize()` creates and initialises its attachment tracker from `Spec.GetAttachmentGroups()`, and restoration reconciles that tracker from the same spec. [Location](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Location.cs) `ShopAttachment` is declared as an attachment for `LocationSpec`, carries a `TemplateReference` to `ShopTemplate`, and its `SpawnElement()` returns `new Shop()`. [ShopAttachment](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/ShopAttachment.cs)

That is especially significant because `Shop` implements `IRefreshedByAttachment<ShopAttachment>`. The first-party interface itself carries an explicit caution that this mechanism **“shouldn't be used on elements that are dynamically added”** and says attachment initialisation occurs before the element's `OnInitialize`/`OnRestore`, after the Location is already initialised. [AttachmentContract](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Attachments/IRefreshedByAttachment.cs)

Therefore:

| Candidate ownership path | E1 assessment |
|---|---|
| Open an existing loaded vanilla `Shop` | **Supported native reuse**, already compatible with the known Better Bonfire pattern, but it cannot independently own the six custom weapons/title/save state |
| Dedicated `LocationTemplate` + `LocationSpec` + `ShopAttachment` + dedicated `ShopTemplate` | **Strongest first-party supported ownership candidate** |
| `new Shop()` followed by arbitrary dynamic `AddElement` | **Not supported by the published attachment contract**; later pinned-DLL proof would be needed before considering it |
| Construct `ShopUI` directly around custom data | **Contradicts the native lifecycle** because it bypasses `Shop.OpenShop`, stocks, hero-storage acquisition, native close and merchant transactions |
| Reuse a vanilla merchant Location and replace/clone its saved shop state | **Does not meet the requested ownership isolation** and risks sharing title, stock and persistence with the original merchant |

This does not authorise the second row for implementation. It identifies the path whose semantics are supported well enough to justify the later pin-bound inspection.

### A separate runtime Location solves the merchant-name problem cleanly

`ShopUI` is an unsaved UI element of `Shop`; its merchant name comes from the Shop's parent Location's display name. [ShopUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/UI/ShopUI.cs) A `RuntimeLocationInitializer` has a saved `OverridenLocationName`, and Location's display-name code honours that override. [RuntimeLocationInitializer](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/RuntimeLocationInitializer.cs) [Location](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Location.cs)

That gives the research question a source-backed answer: **“Mysterious Merchant” does not require renaming the current bonfire or a shared vanilla merchant Location.** A dedicated location can own that name.

There is nevertheless an unresolved lifetime decision. Runtime locations are given generated IDs of the form represented by `LocationCreator` and the runtime initializer is saveable. Repeatedly spawning one every time the player clicks the bonfire button would therefore create distinct model identities unless some higher-level deduplication exists; no such merchant-specific deduplication mechanism was found in the inspected public source. [LocationCreator](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/LocationCreator.cs) This makes **“one persistent merchant owner per save, reachable from every bonfire”** the safer lifecycle hypothesis to test later than **“spawn a new saved shop on every click”**. That is an inference from the identity/persistence contracts, not an implementation decision.

Cross-scene lifetime is not settled by `ShouldBeSaved` alone. Ordinary Locations default to scene-domain ownership, while AR-Questline separately supplies `GameplayUniqueLocation.InitializeForLocation`, which moves a location to `Domain.Gameplay` and contains explicit scene-change hide/restore handling. That demonstrates that first-party code treats cross-scene persistent location lifetime as a distinct concern. It does **not** prove that `GameplayUniqueLocation` is appropriate for a non-world Mysterious Merchant. [GameplayUniqueLocation](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Attachments/Elements/GameplayUniqueLocation.cs)

**Missing static proof:** the exact pinned build must establish what happens to the proposed merchant Location when its originating scene unloads, whether a non-visual/hidden Location is safe, and what domain owns it across save/load and scene changes.

### Stock policy is native, but the owner must choose its semantics

A `ShopTemplate` carries the sell/buy price modifiers, fence settings, restockable definitions, unique items, locked unique items, restock wealth, maximum wealth and blocked-sale tags. [ShopTemplate](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/ShopTemplate.cs) `Shop.OnInitialize()` creates the native stock objects from that template, and `Shop.OpenShop()` invokes the native restock/decompression sequence before the UI is created. [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs)

The available native stock classes embody materially different acquisition policies:

**`UniqueStock`** is documented in source as stock that does not change on restock and is intended for important items that can be bought only once. It serializes its compressed item state. [UniqueStock](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Stocks/UniqueStock.cs) This is a potentially natural representation for six unique weapons **only if** the owner later chooses one-time purchases. This research does not make that choice.

**`RestockableStock`** serializes its loot table, capacity, remove policy and compressed items. On restock it removes the configured items and then pulls new entries from its `LootTableAsset` if under capacity. [RestockableStock](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Stocks/RestockableStock.cs) That is the native replenishing route, but using it would require an explicit replenishment decision and a defined loot-table contract; it must not be selected merely because the merchant “needs stock”.

**`BoughtFromHeroStock`** is the native holding area for goods the shop buys from the player; it has its own cached-price and compression/reconstruction behaviour. [BoughtFromHeroStock](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Stocks/BoughtFromHeroStock.cs) This becomes relevant because the default native shop interface is not intrinsically “buy six weapons only”.

Indeed, `ShopUITabs` declares **Buy, Sell, Sell From Stash and Buyback**, and all four are defined with an always-visible predicate in the examined first-party snapshot. [ShopUITabs](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Tabs/ShopUITabs.cs) Therefore a request for “a custom weapons shop” does not, by itself, prove a buy-only UI. The owner has not specified whether players should be allowed to sell ordinary inventory to the Mysterious Merchant or use buyback/stash selling. Silently accepting those tabs would itself be an unreviewed behavioural decision.

### Stock compression forbids arbitrary insertion timing

Native `Stock` compresses item instances when the shop closes and reconstructs them when it opens. Its add operation rejects additions while the stock is compressed. [Stock](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Stocks/Stock.cs) This explains why a route that merely finds a `Shop` model at an arbitrary time and pushes six `Item` instances into it is lifecycle-sensitive.

A properly authored dedicated `UniqueStock` or restockable template avoids needing to improvise around that boundary, because native Shop initialisation and opening own creation/decompression. A runtime-injection alternative would require exact current-binary timing proof. The newer public repository's separate experiments also record a need to wait for `Shop.OpenShop()` to decompress stock before insertion, but that evidence is newer than the supplied repository pin and is therefore corroborative only. [unresolved citation: turn67file0]

### Template persistence is GUID persistence

There is a particularly clear save contract:

`SaveWriter.WriteTemplate` → writes template GUID.  
`SaveReader.ReadTemplate` → reads GUID and calls `TemplatesUtil.Load<T>`.  
`TemplatesUtil.Load<T>` → asks `TemplatesProvider` in a running build.  
`TemplatesProvider` → resolves from the maps built by `TemplatesLoader`. [SaveWriter](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/MVC/Serialization/SaveWriter.Special.cs) [SaveReader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/MVC/Serialization/SaveReader.Special.cs) [TemplatesUtil](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesUtil.cs) [Provider](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesProvider.cs)

The implication is direct: **a custom ShopTemplate that exists only as an unregistered in-memory object at save time is not proven restorable merely because it has a string that looks like a GUID**. It must be resolvable through the template provider when the save is loaded. Conversely, the first-party Addressables/mod-catalog route is structurally capable of supplying such GUID-addressed assets before template loading. [Loader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesLoader.cs) [Authoring](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Editor/Assets/Templates/AddressableTemplatesCreator.cs) [ModManager](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Utility/Assets/Modding/ModManager.cs)

This is one of the central facts the later C0/build-ownership decision must address.

## Bonfire-to-shop lifecycle and transaction comparison

The target experience can be mapped onto native responsibilities without replacing the native transaction engine.

| Player/lifecycle stage | Native contract established by first-party source | Responsibility still belonging to the proposed feature |
|---|---|---|
| Bonfire UI initialises | `VFireplaceUI` owns native `ButtonWithDescription` controls; `RegisterButton` initialises a button callback and hover/selection description behaviour. The view is an autofocus/focus source. [VFireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/VFireplaceUI.cs) | Attach exactly one Mysterious Merchant entry without duplicating inherited handlers and prove mouse/controller focus in the pinned build |
| Player activates entry | Built-in fireplace actions hide their current UI state while opening subordinate UI and restore it after subordinate close. [VFireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/VFireplaceUI.cs) [FireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/FireplaceUI.cs) | Resolve the one persistent merchant Shop and call its native open path; define failure behaviour if owner/template/stock is unavailable |
| Merchant opens | `Shop.OpenShop()` requests hero storage, emits the shop-open event, restocks, opens/decompresses each stock, creates `ShopUI` and registers close handling. [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) | Do not bypass this by manually adding `ShopUI` |
| Shop receives title | `ShopUI.MerchantName` uses the owning Location's display name. [ShopUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/UI/ShopUI.cs) | Ensure the dedicated owner says “Mysterious Merchant”; do not rename the bonfire |
| Shop displays inventory | Buy UI treats the `Shop` as seller and Hero as buyer; normal buy inventory excludes `BoughtFromHeroStock`, which has its own buyback presentation. [unresolved citation: turn16file0] | Prove all six exact weapon identities are present once and preserve their existing GUID/template/assets |
| Price is displayed | Native pricing resolves the stock's price provider. `DefaultPriceProvider` derives a price from the item's native price and merchant modifiers, rounds and enforces a minimum. [Price](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Prices/DefaultPriceProvider.cs) | Owner must define ShopTemplate modifiers/wealth rather than this research inventing prices |
| Player confirms purchase | Native vendor UI performs affordability handling and delegates the transaction to `TradeUtils.TryTrade`. [unresolved citation: turn18file0] | No guessed “subtract gold then grant weapon” path is needed |
| Transaction executes | `TradeUtils.TryTrade` computes native price, checks buyer wealth, moves the item into buyer inventory, changes seller/buyer wealth and emits the trade events. [TradeUtils](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/TradeUtils.cs) | Runtime proof must bind displayed price to actual wealth delta and item/stock delta |
| Player presses Back | `ShopUI` has a cancel/back prompt; close discards the ShopUI. [ShopUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/UI/ShopUI.cs) | Restore the existing bonfire UI exactly once, with correct focus |
| ShopUI is discarded | `Shop`'s close path tells stocks to close/compress, emits ShopClosed and releases Hero storage. [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) | Preserve this callback even on forced exits; do not merely reactivate the fireplace visual |
| Bonfire itself is forcibly closed | `FireplaceUI` listens for combat entry and fast travel; its `Close(bool)` discards popup/rest UI and the Fireplace model, and may invoke an autosave. [FireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/FireplaceUI.cs) | Coordinate a simultaneously open ShopUI so no orphaned modal, retained storage request or invalid “return to discarded fireplace” callback remains |
| Save/load | Shop/Location/stock/template components have distinct persistence; ShopUI itself is unsaved. [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) [ShopUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/UI/ShopUI.cs) | Prove the dedicated owner and remaining stock restore from the exact save with the same template GUIDs |

This confirms the requested **stock → price → transaction → input → close** chain can remain native rather than reimplementing trading.

It also exposes several behavioural details that must not be conflated:

**Opening and persistence are separate.** A visible `ShopUI` establishes neither a successful purchase nor persistence. ShopUI is expressly unsaved; the durable objects are underneath it. [ShopUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/UI/ShopUI.cs)

**Closing the shop and saving the game are separate.** The Shop close path performs stock and storage cleanup, while the fireplace's own `Close(true)` is the code path that may initiate autosaving. [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) [FireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/FireplaceUI.cs)

**A purchase and acquisition policy are separate.** Nothing in this research authorises removal of the existing startup `ensure-present` grants. Those remain the brief's current policy until the owner chooses shop-only versus shop-plus-testing-grants after successful merchant proof.

**Unique versus replenishing stock is a game-design decision, not an API convenience.** `UniqueStock` and `RestockableStock` explicitly encode different behaviours. [UniqueStock](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Stocks/UniqueStock.cs) [RestockableStock](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Stocks/RestockableStock.cs)

## Contradictions, unsupported shortcuts and unresolved evidence

### Native-shop reuse is not the same as dedicated ownership

The Better Bonfire pattern described in the brief—enumerate currently loaded `Shop` objects and call an existing shop's `OpenShop()`—is useful evidence for **bonfire → native merchant UI**. It does not solve dedicated stock or persistence. The newer public repository still describes its merchant functionality as a profile over `World.All<Shop>()` and `Shop.OpenShop()`, not a spawned merchant inventory, which corroborates the distinction but cannot substitute for the supplied historical pin. [unresolved citation: turn29file1] [unresolved citation: turn29file3]

The two models should therefore remain explicitly separate:

**Native-shop reuse:**  
bonfire button → select an already-existing loaded Location's Shop → `Shop.OpenShop()`.

**Dedicated ownership candidate:**  
registered dedicated LocationTemplate → LocationSpec → ShopAttachment → dedicated ShopTemplate → one merchant-owned native Shop → bonfire button resolves that Shop → `Shop.OpenShop()`.

The second does **not** require inventing a replacement shop engine. It creates another owner using the game's existing ownership engine.

### “Temporary Shop per bonfire” is not established

The public source supports runtime Locations, but those Locations have saveable initializers and generated model identities. Nothing inspected establishes that repeatedly spawning a merchant owner, opening it, and discarding it is safe for stock persistence. [RuntimeLocationInitializer](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/RuntimeLocationInitializer.cs) [LocationCreator](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/LocationCreator.cs)

For the same reason, cloning an existing Shop's state to create a temporary merchant is not source-backed. It would introduce questions about copied compressed stock, merchant stats, template identity and purchases which the native attachment route already avoids.

### Direct dynamic Shop creation conflicts with a published caution

The only direct `new Shop()` construction found in first-party shop source is `ShopAttachment.SpawnElement()`. [ShopAttachment](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/ShopAttachment.cs) And `IRefreshedByAttachment` explicitly warns against using that pattern for dynamically added elements. [AttachmentContract](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Attachments/IRefreshedByAttachment.cs)

This does not mathematically prove a reflected/dynamic `AddElement(new Shop())` can never run. It does mean it would be a **non-proven deviation from the documented lifecycle** and cannot be promoted simply because it compiles.

### Runtime template injection and first-party mod content are different authority classes

The official content path loads template-labelled Addressables into the template maps. [Loader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesLoader.cs) The newer public repository contains separate experimental work that manipulated private template-loader state for runtime clones. [unresolved citation: turn66file1]

Those approaches are not interchangeable:

- a native mod catalog has stable built content, a GUID and the game's loader path;
- private-map runtime registration depends on implementation details and registration timing;
- a plain runtime object with an assigned GUID is not enough to prove save restoration.

The working Tainted Weapons DLL/C0 constraint determines whether any content-catalog or registration addition can be integrated without replacing its proven binary. **This research does not resolve that repository ownership decision.**

### Cross-scene persistence remains an exact gap

A saved runtime Location is not automatically proof of the desired cross-scene lifetime. First-party code has a separate `GameplayUniqueLocation` facility precisely for moving a Location to the gameplay domain and managing scene visibility. [GameplayUniqueLocation](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Attachments/Elements/GameplayUniqueLocation.cs)

The later pinned static inspection therefore needs to determine:

- whether the merchant can remain in `Domain.Gameplay` without world presence;
- whether moving a Shop-owning Location between domains correctly moves all owned stock/items;
- whether the Shop is still discoverable/openable from a bonfire in another scene;
- whether an invisible/headless Location is valid;
- whether its view host or prefab references can be omitted safely;
- whether save restoration occurs before the bonfire entry can resolve it.

Public source answers none of those merchant-specific questions completely.

### Default native shop capability is broader than the requested experience

The examined `ShopUITabs` makes Buy, Sell, Sell From Stash and Buyback available. [ShopUITabs](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Tabs/ShopUITabs.cs) If the intended experience is truly “a shop showing the six custom weapons” but not a general merchant that buys the player's items, that difference must be decided explicitly. Removing tabs or changing sell behaviour would be another behavioural/UI contract and needs its own proof; it cannot be assumed from the word “custom”.

### Prices are still unspecified

Native arithmetic is known, but the desired values are not. `ShopTemplate` supplies price modifiers and the item's native values feed `DefaultPriceProvider`; native transactions then charge the resulting price. [ShopTemplate](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/ShopTemplate.cs) [Price](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Prices/DefaultPriceProvider.cs)

Therefore Deep Research can say **how** prices should be produced natively, but it cannot supply fair prices for Baleful Eye, Wayfarer's Oath, Graveward, Penitent's Burden, Thornwake or Widow's Lament without inventing balance. The later ShopTemplate must either use explicitly approved/native-derived values or await an owner decision.

### Public independent examples are insufficient

Searches for inspectable FoA custom-shop implementations did not yield an independent third-party source that demonstrates dedicated `ShopAttachment`/`ShopTemplate` ownership end-to-end. The relevant GitHub material found was mainly the AR-Questline source and this repository's own experiments. [unresolved citation: turn68file0] [unresolved citation: turn67file0]

Accordingly, no claim such as “other mods prove this works on the current Steam build” is supported by this report.

## Exact remaining proof and test contract

The public research has reduced the later internal investigation from an open-ended “how do shops work?” question to a comparatively narrow set of claim-fit checks.

### Decompilation-static lane

**Status: NOT_RUN by this Deep Research execution.**

Against exactly:

`S:/SteamLibrary/steamapps/common/Tainted Grail FoA/Fall of Avalon_Data/Managed/TG.Main.dll`  
SHA-256 `749aabbfbec121bb69bda0ae226223154406d2c990df3312ad12365d513fa982`

the later authorised static review should answer the following without relying on toolkit-version similarity.

| Required proof | Expected comparison target from first-party source | Blocking consequence if different |
|---|---|---|
| `Shop : Element<Location>` and `IRefreshedByAttachment<ShopAttachment>` still hold | `Shop.cs` / `IRefreshedByAttachment.cs` [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) [AttachmentContract](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Attachments/IRefreshedByAttachment.cs) | Dedicated attachment design cannot be promoted |
| `ShopAttachment.SpawnElement` and ShopTemplate assignment sequence are unchanged enough | `ShopAttachment` [ShopAttachment](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/ShopAttachment.cs) | Must re-derive exact native ownership |
| `LocationTemplate.SpawnLocation`, `RuntimeLocationInitializer.ShouldBeSaved`, Location attachment initialisation survive in pinned DLL | Runtime-location files [RuntimeLocationInitializer](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/RuntimeLocationInitializer.cs) [LocationTemplate](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Setup/LocationTemplate.cs) | Runtime dedicated owner remains unproven |
| exact domain/drop behaviour of a runtime Location across scene unload | `Location`, domain machinery, `GameplayUniqueLocation` [GameplayUniqueLocation](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Attachments/Elements/GameplayUniqueLocation.cs) | Save/reopen/scene-transition policy cannot be selected |
| ShopTemplate serialization writes stable template identity and restore resolves it | save reader/writer + TemplatesProvider [SaveWriter](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/MVC/Serialization/SaveWriter.Special.cs) [SaveReader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/MVC/Serialization/SaveReader.Special.cs) | Existing-save support is blocked |
| installed build's `ModService`/mod catalog/TemplateLoader order matches the first-party model | `ModManager`, `Mod`, `TemplatesLoader` [ModManager](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Utility/Assets/Modding/ModManager.cs) [Mod](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Utility/Assets/Modding/Mod.cs) [Loader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesLoader.cs) | Custom native templates cannot yet be claimed resolvable |
| `OpenShop()` still requests storage, restocks, opens stock and closes through ShopUI discard | `Shop.cs` [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) | Must not open ShopUI directly as a substitute |
| current `Stock` compression boundaries | `Stock.cs` [Stock](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Stocks/Stock.cs) | Any stock-add strategy remains unsafe |
| current default tabs and exact tab visibility | `ShopUITabs.cs` [ShopUITabs](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Tabs/ShopUITabs.cs) | Buy-only versus full-merchant behaviour cannot be stated |
| current `TradeUtils` and default price path | `TradeUtils`, `DefaultPriceProvider` [TradeUtils](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/TradeUtils.cs) [Price](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Prices/DefaultPriceProvider.cs) | Displayed/charged-price test expectations must be revised |
| `VFireplaceUI` focus/button ownership and `FireplaceUI.Close(bool)` | fireplace source [VFireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/VFireplaceUI.cs) [FireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/FireplaceUI.cs) | Return/focus/forced-close integration remains blocked |
| exact owner lookup after save/load | Location/model ID and any intended marker | Without deterministic rediscovery, a persisted merchant can exist yet be unreachable or duplicated |
| C0-compatible integration boundary | working Tainted Weapons binary/source/artifact relationship | No build or packaging mutation until closed |

A particularly important inspection target is **template load timing**. The first-party source's clean native path depends on mod catalogs being present before `TemplatesLoader` enumerates the `template` labels. [ApplicationScene](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Scenes/SceneConstructors/ApplicationScene.cs) [Loader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Templates/TemplatesLoader.cs) That ordering must be checked in the pinned game and against whatever packaging mechanism Tainted Weapons currently uses; it should not be inferred from v1.1.0 source.

### Runtime/internal-game lane

**Status: NOT_RUN by this Deep Research execution.**

The runtime gate should use a disposable named save and record the exact game DLL, merchant-content artifact, weapon DLL and configuration hashes. A minimally sufficient proof matrix is:

| Scenario | Required observation |
|---|---|
| First bonfire entry | Exactly one **Mysterious Merchant** button; existing Services and vanilla entries unchanged |
| Mouse activation | One callback invocation, one merchant owner, one ShopUI |
| Controller activation | Focus reaches the new entry; activation opens the same Shop; Back returns focus to bonfire |
| Shop identity | Title is Mysterious Merchant and the bonfire's own Location/display name remains unchanged |
| Inventory | Exactly the six expected GUID-backed weapons appear with expected names/icons; no accidental vanilla inventory unless explicitly approved |
| Sufficient funds | Displayed native price equals actual player wealth decrement; weapon inventory increments once; merchant stock decrements once |
| Insufficient funds | Native rejection occurs; no weapon, stock or currency delta |
| Purchased weapon | Existing mesh/icon/grip/animation/combat behaviour remains the working weapon behaviour |
| Reopen immediately | Purchased-state semantics match the approved Unique/Restockable policy; no duplicate restoration |
| Sell/buyback | Either deliberately supported and proven, or explicitly removed through a separately reviewed native UI policy |
| Shop Back | Stock closes/compresses, storage request releases and bonfire returns exactly once |
| Repeated entry | No duplicate Location, Shop, stock or event handler |
| Combat/forced close | Both modal layers clean up; no orphaned ShopUI, hidden fireplace, storage hold or stale restore callback |
| Scene transition | Merchant owner survives or is deliberately reconstructed exactly once according to the approved ownership model |
| Save/reload | Remaining stock and purchased weapon state restore from the same template GUIDs; merchant owner is rediscovered rather than duplicated |
| Existing save | Defined result for a save made before the merchant feature existed; no data loss or duplicate six-weapon injection |
| Disable feature/mod | Explicit policy is observed; no save corruption and existing purchased weapons remain intact |
| Failure: template absent | Button/open attempt fails visibly/safely rather than creating a partial Shop |
| Failure: stock absent | No empty “success” claim; ownership/stock error is observable |
| Failure: late template registration | Save/load/open path refuses or reports unresolved template rather than silently using another template |
| Vanilla negative controls | An ordinary merchant and an ordinary bonfire still open, trade, close and save normally |

The user-supplied requirement to capture inventory/currency deltas, cleanup markers, screenshots and errors is well matched to the native transaction contract: `TradeUtils` changes item ownership and both parties' wealth in one native operation, so all three dimensions can be verified independently. [TradeUtils](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/TradeUtils.cs)

### New-campaign and existing-save policy

The research supports a policy shape but not final promotion:

**New campaign:** a dedicated merchant owner can conceptually be created once after its registered templates are available, then persist as a normal native model graph. The exact creation trigger, domain and stable lookup must be pinned-build proven first.

**Existing save:** the feature must tolerate “merchant model not yet present” and create at most one owner only after template resolution is available. A later load must find that existing owner rather than manufacture another. Because runtime Location IDs are allocated dynamically, the feature needs an inspected stable discovery/marker contract; the public source examined here does not supply a Mysterious-Merchant-specific one. [LocationCreator](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/LocationCreator.cs)

**Feature removal/disable:** the report cannot yet claim that a save containing a custom LocationTemplate/ShopTemplate can safely load with those template assets absent. In fact, the GUID-based template load path makes missing-template behaviour an explicit risk: save restoration calls `TemplatesUtil.Load<T>(guid)`. [SaveReader](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/MVC/Serialization/SaveReader.Special.cs) Therefore “disabling the merchant feature” must be tested against both saves with and without a created merchant before any compatibility promise.

Existing player-owned copies of the six weapons should remain untouched under all policies described in the brief. No source found provides a justification for deleting inventory merely because acquisition later becomes shop-based.

## Review path and research disposition

The returned report should remain **E1 research context**. It substantially narrows the candidate contract but does not promote an implementation.

**RH1 — claim evaluation: REQUIRED.** Review each first-party source claim against its exact narrow meaning. In particular, keep “toolkit v1.1.0 supports this structure” separate from “installed `TG.Main.dll` supports it”; keep “native mod catalogs can carry templates” separate from “the current Tainted Weapons packaging may add such content without C0 impact”; and keep “runtime Location is serializable” separate from “this particular shop survives every scene transition”.

**RH2 — native UI/lifecycle domain review: REQUIRED.** Review the complete native chain `VFireplaceUI → callback → Shop.OpenShop → stocks → ShopUI → TradeUtils → ShopUI discard → Shop.OnShopClosed → bonfire restoration`, including focus, combat close, fast-travel close, save timing, all default merchant tabs and cross-scene model ownership. The first-party sources demonstrate why partial UI proof is insufficient. [VFireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/VFireplaceUI.cs) [Shop](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Locations/Shops/Shop.cs) [unresolved citation: turn18file0] [FireplaceUI](https://github.com/AR-Questline/merlin-workshop/blob/073bdab3e09d6adad5003339fc49b021738d71e6/Assets/Code/Main/Crafting/Fireplace/FireplaceUI.cs)

**RH3 — independent implementation review: REQUIRED after the static gate, before mutation.** The reviewer should specifically challenge duplicate-owner prevention, GUID/template registration timing, scene-domain selection, existing-save creation, feature-disable behaviour, native sell/buyback exposure and C0 compliance. A shortcut based on dynamically adding `Shop`, directly constructing `ShopUI`, or manually moving gold should be rejected unless new claim-fit evidence overturns the first-party lifecycle findings.

**RH4 — validation review: NOT_READY.** It becomes meaningful only after an authorised candidate exists and the runtime matrix above has actually executed against exact artifact hashes. Visible UI alone, successful compilation, or source similarity cannot satisfy it.

The research-state summary is therefore:

| Gate | State | Result |
|---|---|---|
| Deep Research execution | **PASSED** | Information gathering performed from inspected public sources |
| Durable first-party ownership pattern | **PASSED at E1** | `Location → ShopAttachment → ShopTemplate → Shop` is directly supported |
| Dedicated runtime Location mechanism | **PASSED at E1** | Native saveable runtime Location construction exists |
| Native mod-template content path | **PASSED at E1** | First-party mod catalogs and template-labelled Addressables provide a GUID-resolvable content mechanism |
| Independent third-party dedicated-shop example | **PARTIAL** | No durable inspectable example located; first-party source remains the principal evidence group |
| Exact supplied repository pin reopening | **BLOCKED** | Connected GitHub could not resolve `1a30867675ceef1ddfdb5e86cbb41ffb43ccb3d2`; newer HEAD was not substituted |
| Pinned installed-DLL static equivalence | **NOT_RUN** | Explicitly excluded from this research execution |
| Tainted Weapons C0/build ownership | **BLOCKED** | Brief states the working DLL is ahead of source; no workaround architecture was selected |
| Price/balance policy | **BLOCKED** | Native calculation path known; desired modifiers/prices are not owner-specified |
| One-time versus replenishing stock | **BLOCKED** | Both native mechanisms exist; choosing between them would invent policy |
| Buy-only versus full merchant tabs | **BLOCKED** | Native snapshot exposes Buy/Sell/Stash/Buyback; requested policy is unspecified |
| Cross-scene/save ownership | **PARTIAL** | Components are serializable, but merchant-specific domain/lifetime and deterministic rediscovery are unproven |
| Runtime transaction/save proof | **NOT_RUN** | Requires later authorised game execution |
| Implementation/deployment/release | **NOT_RUN** | No mutation, build, test, deployment or release action was performed |

The central research conclusion is consequently narrower than “build a merchant”: **the game already has a complete native shop ownership and transaction system, and its first-party mod pipeline has a plausible durable content route for dedicated Location/Shop templates. The remaining blockers are now exact artifact-equivalence, owner-policy, lifecycle and integration evidence—not absence of a native merchant mechanism.**