# Mechanics Catalogue

Quick reference for reusable FoA modding techniques and how strong the supporting evidence currently is.

| Technique | Evidence | What it does **not** prove |
| --- | --- | --- |
| Resolve `ItemTemplate` after `TemplatesProvider.AllLoaded` | Source inspected | A template ID is valid on every game build |
| Grant an `Item` to Hero inventory | Source inspected and used by real consumers | Save/reload safety |
| Clone a template and call `TemplatesLoader.AddToMap` | Seen in multiple source implementations | Long-term compatibility; it uses private internals |
| Restore a saved template by GUID | Exact Mono binary contract inspected | Custom registration will always happen early enough at runtime |
| Use `ItemTemplate` helper properties for classification | Source inspected | Those helpers form one complete universal item taxonomy |
| Suppress `LockpickingInteraction.ConsumePickHP` | Source inspected | Every custom lockpick flow has been runtime-tested |
| Use the native clothes/Kandra equip lifecycle | Decompiled static contract | Custom armour has passed a complete live validation |
| Register a custom weapon and serve a Drake prototype | Static evidence plus limited runtime evidence | Full persistence and lifecycle safety |
| Append a runtime alchemy recipe | Source inspected | Persistent recipe learning |
| Call `HeroRecipes.LearnRecipe` for an existing recipe | Source inspected | Save/reload safety without a throwaway-save test |
| Restock merchants on `Shop.OpenShop` | Source inspected | Full runtime ownership/compatibility validation |
| Add a spell-cast VFX overlay | Source inspected | Template-name family matching is an exact native spell identity |
| Run a one-session companion lifecycle | Source plus representative runtime evidence | Persistence; the demonstrated route is intentionally not saved |
| Use the Avalon Awakened resolver | Project API contract | Native FoA API behavior |
| Build a completed Addressables handle bridge | Source inspected | Arbitrary Addressables substitution is safe |
| Use the documented ModService catalogue layout | Repeated offline proof | In-game actor/content integration |
| Observe concrete native save completion callbacks | Source plus decompiled targets | A general-purpose custom save serializer |
| Register an arbitrary new native save domain | **No supported route found in the inspected Mono build** | That no future build or different mechanism could provide one |
| Store mod-owned state in a sidecar | Research in progress | A general save-safe public implementation yet |

Use the linked system/mechanic pages for the actual implementation details and version limits.
