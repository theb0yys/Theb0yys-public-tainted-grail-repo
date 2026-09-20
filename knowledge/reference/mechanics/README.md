# Mechanics Catalogue

Use this catalogue to see which reusable modding techniques have been investigated and how strong the evidence is for each one. The table's status and boundary columns are the authoritative part of this page.

Curated public view of reusable private mechanics. Status is claim-specific.

| Mechanic | Public state | Important boundary |
| --- | --- | --- |
| Resolve `ItemTemplate` after `TemplatesProvider.AllLoaded` | Static/source inspected | Exact identity/runtime scope matter |
| Grant `Item` to hero inventory | Static/source inspected; consumer-used | Does not prove save safety |
| Direct clone + `TemplatesLoader.AddToMap` | Multi-consumer static/source evidence | Private API; patch-sensitive; shared registrar preferred |
| Template GUID restoration dependency | Current-binary static contract | Registration/restoration order still runtime-sensitive |
| Item helper classification | Source inspected | Helpers are not a universal taxonomy |
| Lockpick `ConsumePickHP` guard | Static/source inspected | Live custom-item route separately validated |
| Native clothes/Kandra equip lifecycle | Decompiled static contract | No custom-armour runtime pass implied |
| Custom weapon registration + Drake prototype | Static + bounded runtime evidence | Persistence/full lifecycle separate |
| Runtime alchemy recipe append | Source inspected | Not persistent learning |
| Existing recipe `HeroRecipes.LearnRecipe` | Source inspected | Throwaway save/reload still required |
| Merchant restock | Source inspected | Owner review recorded runtime gap |
| Spell cast VFX overlay | Source inspected | Family mapping is name heuristic |
| One-session companion lifecycle | Source + representative runtime lineage | Explicitly not saved |
| Avalon Awakened resolver | Project API boundary | Not native FoA API |
| Completed Addressables handle bridge | Source inspected | Specific bridge, not global substitution |
| ModService catalogue layout | Offline two-cycle proof | No game deployment/actor proof |
| Native save completion observation | Source/decompiled candidate | Not generic durable-success semantics |
| Arbitrary native mod save domain | **Blocked by static verdict** | No supported mutable registrar found |
| Sidecar persistence | Under evaluation | Static lifecycle candidates; runtime/save matrix incomplete |
