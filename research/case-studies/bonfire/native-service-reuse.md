# Better Bonfire Menu: Reusing Native Services Without Rebuilding Them

This case study is useful when you want to add a **convenience menu around existing FoA systems** without taking ownership of the systems themselves.

Better Bonfire Menu did two different jobs:

1. discover and call the game's real bonfire/service entry points;
2. build a better menu for reaching those services.

Those two jobs reached different proof levels at different times. Keeping them separate is the main lesson.

## What the mod was trying to do

The goal was not to invent new crafting, saving, storage, travel, merchant, or equipment systems.

It was to give the player one richer bonfire menu that could open native services such as:

- stash;
- cooking;
- alchemy;
- handcrafting;
- rest;
- level up;
- sharpening/upgrades;
- relic management;
- transmogrify;
- identify items;
- selected loaded merchants;
- fast travel;
- save;
- pet recall.

The intended design was:

~~~text
Better Bonfire Menu
→ choose service
→ check the same native precondition
→ call the native FoA action/UI
→ let FoA own the actual transaction
→ return to the bonfire menu
~~~

That keeps gameplay ownership in the game instead of recreating each service inside the mod.

## Step 1: map the native services

The first useful result was identifying the actual FoA entry points.

Examples include:

| Player-facing action | Native route |
| --- | --- |
| Open Stash | `FireplaceUI.OpenHeroStorage()` |
| Cooking | `FireplaceUI.CookAction()` |
| Alchemy | `FireplaceUI.AlchemyAction()` |
| Handcrafting | `FireplaceUI.HandcraftingAction()` |
| Rest | `FireplaceUI.GoToSleepAction()` |
| Level Up | `FireplaceUI.LevelUpAction()` |
| Save | `FireplaceUI.SaveGame()` |
| Fast Travel | `WyrdRepellingFireplaceUI.FastTravel()` |
| Recall Pet | `WyrdRepellingFireplaceUI.RecallPet()` |
| Identify Items | `GemsUI.OpenIdentifyUI()` |
| Sharpen / Upgrade | native Gems UI sharpening tab |
| Manage Relics | `GemsUITabType.GemManagement` |
| Transmogrify | `GemsUITabType.Transmogrify` |
| Gear Care | `GemsUITabType.WeightReduction` |

This mattered because several tempting labels were misleading.

### "Repair" was not a proven durability-repair service

The researched native route behind the repair-side entry was the armour **weight-reduction** service.

No separate native durability-repair service had been established.

So the public description must not turn that into "Repair gear durability."

### Arrow Crafting did not create recipes

The Arrow Crafting convenience entry routed into native bonfire handcrafting.

It did **not**:

- inject recipe templates;
- force-learn recipes;
- bypass the bonfire crafting upgrade.

### Merchant support did not spawn a universal merchant

The merchant feature enumerated currently loaded `Shop` Models, grouped them through UI profiles, and called `Shop.OpenShop()` on the selected live shop.

It did **not** create:

- a new merchant;
- a custom shop template;
- custom stock pools;
- rare-item injection;
- quest/unique-item handling.

That distinction is one of the strongest reusable lessons from the project.

## Step 2: preserve native preconditions

Finding the service method was not enough.

The upgraded bonfire already has conditions for several actions:

- handcrafting requires the relevant bonfire crafting level;
- fast travel requires an upgraded bonfire and open-world context;
- save availability depends on native save restrictions/guards;
- pet recall depends on a pet actually being left behind.

The mod therefore kept configured services visible but disabled when the game could not currently run them.

That is better than either:

- hiding every unavailable option with no explanation; or
- forcing the native action in an invalid state.

The menu can explain availability, but the game still decides whether the service is valid.

## Step 3: the first UI was deliberately separate from service ownership

The earlier implementation used a plugin-owned styled panel.

When the player selected a service:

~~~text
plugin panel
→ native bonfire UI hidden
→ native service opened
→ plugin tracks opened Model
→ service Model discarded
→ plugin panel / bonfire restored
~~~

This already proved an important architecture:

> The mod could provide navigation while native FoA UI and gameplay still owned the service.

That service-routing proof existed before the final native submenu work.

## Step 4: a UI ownership bug exposed the difference between "visible" and "usable"

Version 0.5.3 added shared custom-UI scope handling while the native bonfire page was still open.

User screenshot/runtime feedback showed the panel itself worked, but vanilla bonfire options and normal close input stopped working.

The problem was not service routing.

The problem was **input ownership**: the custom UI scope was claiming input that still belonged to the native bonfire page.

The 0.5.4 correction stopped claiming modal custom-UI input ownership while the native bonfire page remained active.

That is a valuable case-study result:

~~~text
correct service calls
+
correct-looking panel
≠
correct input ownership
~~~

## Step 5: avoid continuous world searches in IMGUI

Another iteration found that the plugin should not repeatedly rediscover the visible fireplace from broad world state during every IMGUI pass.

The implementation moved toward:

- caching the active fireplace while valid;
- throttling fallback discovery;
- skipping fireplace discovery while a native service opened by the panel was active;
- scanning loaded Shops only while the merchant picker was open, with limits.

This did not change service behavior. It changed how the UI found and retained its current owner.

That separation matters: performance fixes should not silently alter gameplay/service semantics.

## Step 6: replace the fake entry with a real native bonfire button

The next major improvement was the **Services entry itself**.

The researched native route patches:

~~~text
VFireplaceUI.OnInitialize
→ obtain initialized native Level Up ButtonWithDescription
→ clone its ButtonConfig / GameObject
→ register a Services button through native ButtonWithDescription flow
→ use native hover-description behavior
~~~

By version 0.5.10, the primary Services entry was a runtime-cloned native bonfire button, with the old lightweight IMGUI `SERVICES` row retained only as a fail-safe.

User feedback on 2026-07-12 reported that the 0.5.10 native Services entry and the existing service flow worked in game.

That is a stronger statement than the previous public case study made.

What it proved:

- a native-looking entry could be attached without editing the bonfire prefab;
- the entry could participate in the native button stack;
- service routing behind that entry already had working lineage.

What it did **not** prove:

- the later full native submenu.

## Step 7: the full native submenu was a new feature, not a continuation of the same proof

Version 0.6.0 attempted to replace the styled plugin-owned Services panel with a runtime-built **native submenu** under the active `VFireplaceUI`.

The design reused:

- the initialized native Level Up button as the button template;
- the bonfire's `buttonContent` hierarchy;
- native focus/description behavior;
- disabled button styling;
- a two-column scrollable layout;
- the existing bonfire Cancel prompt as Back.

It also retained the prior IMGUI panel as a fallback if native submenu construction failed.

This was a much larger UI-lifecycle claim than "we can add a Services button."

## Step 8: Back/Cancel ownership needed a narrow patch

The native submenu needed Cancel/Back to close the submenu without breaking the fireplace's other close paths.

The first source pass intercepted private `FireplaceUI.Close(bool)` too broadly.

Review caught that this could consume closure that belonged to:

- combat interruption;
- an active service transition;
- other native fireplace shutdown.

The patch was narrowed so it only consumes the relevant `saveOnExit` close while:

- this plugin's submenu is visible;
- it belongs to that exact fireplace;
- the submenu is not suspended by an opened service.

Everything else stays vanilla-owned.

This is a strong example of why "Back works" is an ownership problem, not just a button callback.

## Step 9: build/deploy success did not prove the native submenu

The 0.6.0 native-submenu code:

- built successfully;
- was deployed;
- had matching local/live DLL identity;
- had source review for the close-lifecycle correction.

But the runtime evidence from 2026-07-12 showed:

- Better Bonfire Menu 0.6.0 loaded;
- the **styled IMGUI fallback** was visible;
- native Cooking opened successfully from that fallback;
- the log did **not** contain native entry-attach/submenu-prepared evidence for that session.

So that session proves:

~~~text
plugin loaded
+ fallback UI worked
+ native Cooking service opened
~~~

It does **not** prove:

~~~text
0.6.0 full native submenu constructed and worked
~~~

That distinction was lost in the old public case study.

## Step 10: diagnostics were added instead of guessing

Version 0.6.1 added stage-coded diagnostics for:

- Harmony patch registration;
- `VFireplaceUI.OnInitialize`;
- native Level Up/button discovery;
- Services button registration;
- submenu construction stages.

Runtime startup confirmed the plugin loaded and the `VFireplaceUI.OnInitialize` postfix was owned.

But during the observed diagnostic windows, no bonfire was opened, so there was no `OnInitialize` interaction-stage evidence.

Again, this is a narrower proof:

> Patch registration was confirmed; interactive native submenu construction was still not observed in that capture.

## Current implementation direction

Later source (0.6.3) extends the intended native submenu with:

- service entries;
- merchant-profile and loaded-shop entries;
- configuration entries;
- native focus/descriptions;
- disabled-state explanations;
- two-column scrolling;
- relic management;
- transmogrify;
- fallback IMGUI compatibility.

That describes the **implemented source design**.

It should not be conflated with an older runtime proof.

The corresponding release notes still call out live validation needs for the 0.6.x native grid, scrolling, controller focus, Back handling, disabled explanations, service return flow, and related layout behavior.

## What this case study actually proves

### Strong service-ownership findings

The game already has real service owners for the major bonfire actions.

A convenience mod should call those owners instead of recreating:

- storage;
- crafting;
- level-up;
- saving;
- travel;
- pet recall;
- gear-service transactions.

### Strong UI architecture findings

- adding one native bonfire entry is separate from building a complete submenu;
- input ownership must match which page is currently active;
- native service return has to restore the correct host UI;
- Cancel/Back ownership must be narrower than global fireplace close ownership;
- a fallback route is useful when private UI members fail to resolve.

### Strong evidence-discipline finding

Different versions proved different things.

Do not compress:

~~~text
service route known
→ service opened successfully
→ native entry worked
→ native submenu source implemented
→ native submenu fully validated
~~~

into one "working" claim.

They are separate gates.

## Reusable pattern

For another "hub menu" mod, a sensible sequence is:

1. identify the real native service methods;
2. preserve the game's availability/precondition checks;
3. prove each service independently;
4. build the simplest host UI that can call them;
5. validate input/focus/close/restoration;
6. only then migrate to deeper native UI integration;
7. keep a fail-closed fallback if private UI members are patch-sensitive;
8. validate the new host separately even when the underlying service calls are already proven.

## Related pages

- [Bonfire / Fireplace Native Services](../../../knowledge/systems/world/bonfire-services.md)
- [UI, Cursor, Focus, and Input](../../../knowledge/systems/presentation/ui-input.md)
- [Vendor Pricing](../../../knowledge/systems/gameplay/economy-pricing.md)
- [Merchants, Loot, Rewards, and Distribution](../../../knowledge/systems/gameplay/distribution-merchants-loot.md)

## Evidence limits

This public case study is a derived summary of project source, research, validation notes, runtime observations, and versioned implementation history.

It does not reproduce private paths, logs, or proprietary source.

The important current distinction is:

- native service mappings are source/decompilation-backed;
- the 0.5.10 native Services entry has reported working runtime lineage;
- native Cooking was visibly opened through the 0.6.0 fallback route;
- the 0.6.x full native submenu had source/build/deploy evidence but still required interaction/layout/controller/service-return validation before being called fully proven.
