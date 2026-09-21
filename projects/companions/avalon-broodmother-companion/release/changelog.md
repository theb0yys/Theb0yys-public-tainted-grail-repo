# Avalon Broodmother Companion Changelog

## 0.1.34

- Corrects source ownership by removing wrong-target Elephant, Bull, Dragon, Goblin, and Vampire call definitions from the Spider/Broodmother family companion mod.
- Keeps Avalon Awakened as the creature provider and preserves the six Spider/Broodmother summon definitions in this family mod.
- Replaces the old `Skin` item labels and generic route tooltip copy with six distinct companion names and role-specific summon descriptions.
- Does not create standalone unrelated-creature companion mods; those remain behind creature-specific ownership, profile, plugin-ID, and validation gates.

## 0.1.33

- Fixes Goblin companion runtime overlay loading by resolving the exact Addressables resource location through unique native locators before loading the Goblin visual prefab.
- Keeps the existing Avalon Awakened reflection boundary and the one-session companion lifecycle unchanged.

## 0.1.31

- Corrects the six-item inventory/merchant icon path by retaining one sprite cache entry per spider-family summon definition.
- Prevents refreshing one Broodmother or smaller Spider row from destroying the sprite already assigned to another variant row.
- Keeps the six independent item GUIDs, merchant no-stack route, cast dispatch, resolver/visual selection, shared lifecycle, animation, audio, AI, and cleanup behavior unchanged from `0.1.30`.

## 0.1.30

- Corrects the known-merchant stock insertion for cloned custom call items by adding each of the six spider-family summon items without stock stacking.
- Adds a post-loop merchant stock count log that reports whether all six expected spider-family definitions are present after insertion.
- Keeps the six-summon lifecycle, icon, resolver, visual, audio, and AI behavior unchanged from `0.1.29`.

## 0.1.29

- Embeds the three supplied smaller Spider JPG icons for `Spider's Call - Skin 1`, `Spider's Call - Skin 2`, and `Spider's Call - Skin 3`.
- Wires the smaller Spider call items, HUD, and command-dialogue portrait to their matching embedded red/black, pale, and gold smaller Spider icon variants.
- Keeps the six-summon lifecycle, merchant, resolver, visual, audio, and AI behavior unchanged from `0.1.28`.

## 0.1.28

- Adds the researched spider-family summon set: three Broodmother spell items and three smaller Spider spell items, all cloned from native `Wolf's Call` and routed through the same guarded one-session companion lifecycle.
- Stocks all six custom call items through the existing configured known-merchant route.
- Selects the matching Broodmother or Spider CI4 template, visual address, scale multiplier, and AI authority identity per spell item while keeping runtime hotkey/panel summon on the original Broodmother default.
- Uses embedded Broodmother icons for the three Broodmother variants and the existing companion/procedural fallback icon route for the smaller Spider variants.

## 0.1.27

- Adds the missing Broodmother-owned input lock/pass-through segment from the proven Avalon Companions command-dialogue path.
- Blocks game hover and player movement/look while Broodmother companion UI is visible.
- Allows Rewired axis/button reads to pass while the Unity command-dialogue menu is visible so EventSystem button input can use the known working route, while keeping the IMGUI command panel locked down.

## 0.1.26

- Changes the native `Companion` command-dialogue cursor/input ownership to match the proven Avalon Companions path: Tainted Interface custom UI scope first, FoA Mod Manager custom UI scope fallback, and local cursor capture/restore around the visible menu.
- Reasserts the real cursor as visible/unlocked while the menu is visible, while keeping the Broodmother-owned virtual cursor removed.
- Leaves command dispatch unchanged: Unity `Button.onClick` and manual hit-test both still route through the same command handler with only same-frame duplicate suppression.

## 0.1.25

- Removes the remaining Broodmother-only submit-release/debounce gate from the native `Companion` command-dialogue host.
- Routes Unity `Button.onClick` selection and manual hit-test selection through the same command dispatcher, leaving only same-frame duplicate suppression so one click cannot double-run.
- Keeps the `0.1.24` proven manual fallback path intact: `Input.GetMouseButtonDown(0)`, one frame guard, real `Input.mousePosition`, visible enabled row hit-test, and direct dispatch.

## 0.1.24

- Copies the proven Avalon Companions direct command-menu click path for the Broodmother manual fallback: one `Input.GetMouseButtonDown(0)` check, one frame guard, real `Input.mousePosition`, visible enabled row hit-test, and direct command dispatch through the same menu command handler.
- Removes the Broodmother-only mouse-release fallback and pointer-specific release state introduced in `0.1.23`.
- Superseded by `0.1.25`: the normal Unity `Button.onClick` route no longer keeps a separate Broodmother submit-release gate.

## 0.1.23

- Fixes native command-dialogue choices that visibly hover but do not dispatch by splitting mouse-pointer choice acceptance from keyboard/controller submit release.
- Keeps the opening debounce and the full submit-release guard for normal Unity button submit, while the direct pointer fallback can dispatch a real mouse down or mouse release over an enabled row after the mouse has been released once since menu open.
- Keeps non-terminal commands in the menu and still closes only `Leave` and `Dismiss`.

## 0.1.22

- Embeds the three supplied Broodmother JPG icon resources into the plugin assembly.
- Uses the current red/black Broodmother image for `Broodmother's Call`, companion HUD, and native command-dialogue portrait before any Tainted Interface fallback.
- Keeps the pale and gold Broodmother icon resources reserved for later variant summons; no additional summons are wired in this build.

## 0.1.21

- Removes the Broodmother-owned visible virtual cursor from the native `Companion` command-dialogue menu.
- Restores the proven Unity UI button path plus real-pointer manual click fallback using `Input.mousePosition`, matching the working Avalon Companions command dialogue pattern.
- Keeps FoA Mod Manager custom UI scope, native `ForceCursorVisibility`, submit-release debounce, and persistent non-terminal command switching.

## 0.1.20

- Keeps the native `Companion` command-dialogue menu open after non-terminal commands so Follow/Hold/Defend and range choices visibly switch selected state and bottom status text.
- Leaves `Leave` and `Dismiss` as the only menu-closing choices; `Dismiss` still clears the active companion.
- Adds the canonical custom creature importer process guide for future creature lanes.

## 0.1.19

- Requires initialized native animated-death visibility before logging `BROODMOTHER_COMPANION_LIFECYCLE_READY`.
- Applies the documented `PlayAnimationDeathBehaviour` alive-prefab suppression correction to keep the no-ragdoll Death(44) body visible through FoA's native death handoff.
- Adds lifecycle diagnostics for native death-behaviour counts, DeathElement linkage, visibility readiness, and alive-prefab suppression clearing.

## 0.1.18

- Hardened native `Companion` command-dialogue opening so menu choices cannot dispatch until the opening submit input has been released once and the debounce window has elapsed.

## 0.1.17

- Added a short command-choice debounce after the native `Companion` command-dialogue menu opens so the opening input cannot immediately dispatch a range/command choice.
- Changed dead/discarded live Broodmother state to keep the same-session active location in a native-death pending handoff instead of clearing it before FoA creates the dummy/corpse.
- Retains Broodmother death visual material references after accepted native death so the same-session body is not stripped of its runtime materials during command-state cleanup.

## 0.1.16

- Added a transient Broodmother-owned LeapJump visual arc on successful leap/ranged damage bridge commits.
- Stopped the full companion follow/combat tick while the native companion menu is open, preventing follow catch-up recall from fighting menu interaction.
- Changed native menu Keep Close / Keep Pace / Keep Distance commands to update spacing without hidden recall; explicit Come Close and Recall still relocate the Broodmother.

## 0.1.15

- Registers the active Broodmother's enabled actor colliders with native `HealthElement` hitboxes and moves those runtime hitbox objects onto FoA's `Hitboxes` layer while the companion is active.
- Restores Broodmother-owned runtime hitbox registrations and layer changes on dismiss, failed spawn cleanup, shutdown, and active companion clear.
- Adds a Broodmother-only short-range damage bridge for event-free `ShortBite` and `LeapJump` commits, sending damage through native `HealthElement.TakeDamage(Damage)` after the existing native target handoff succeeds.

## 0.1.14

- Added FoA native `ForceCursorVisibility` ownership while the Broodmother companion menu is open, matching the working popup cursor pattern.
- Added an `OnGUI` mouse-event path for the Broodmother virtual cursor so movement is not starved when shared UI input resets Unity axes.
- Draws the virtual cursor through `OnGUI` over the Unity UI menu and handles GUI mouse-click hit-testing against the virtual cursor.

## 0.1.13

- Added a Broodmother-owned virtual menu cursor so the companion menu no longer depends on FoA's OS cursor staying away from the center reticle.
- Menu hover and selection now use the virtual cursor position, moved by mouse delta, keyboard/controller axes, and submitted by left click, Enter, Space, or controller south button.

## 0.1.12

- Changed the Broodmother companion menu cursor/input ownership to use FoA Mod Manager `SetCustomUiScope` by soft reflection while open.
- Removed per-frame Tainted Interface cursor reassertion that could fight FoA cursor recentering and cause cursor flashing.
- Kept Tainted Interface as icon/fallback-scope reflection only; no hard FoA Mod Manager or Tainted Interface dependency was added.

## 0.1.11

- Removed the false Broodmother lifecycle failure that required wolf-style layer-24 `BoxCollider` hitboxes; the active actor now validates against its enabled native AlivePrefab colliders.
- Added a scoped native-interact recovery path that opens the same Broodmother companion menu from `E` only when the active Broodmother has the runtime `Companion` action attached, is interactable, nearby, and camera-focused.
- Added command-log markers for native menu recovery open/block results.

## 0.1.10

- Re-armed the active Broodmother as a native damageable lifecycle actor: corpse retention, death element readback, healthbar marker removal, summon invisibility removal, collider/hitbox re-enable, and native dummy/corpse death handoff acceptance.
- Added an active-Broodmother-only bypass for native summon friendly-fire prevention so hero hits can damage the companion during lifecycle validation.
- Added lifecycle readback logs for collider/hitbox counts, death readiness, health readability, and native death acceptance.

## 0.1.2

- Added a custom `Broodmother's Call` cast intercept on `Skill.Perform()` for the exact custom spell item and native summon graph.
- Routed spell casts through the guarded one-session Broodmother companion summon path instead of native `SkillSpawnLocation`.
- Changed the legacy native `SummonPrefab` rewrite setting to default `false`.

## 0.1.1

- Added known Tier 1 merchant stock insertion for `Broodmother's Call`.
- Added `MerchantStock.Enabled`, `MerchantStock.TargetShopGuid`, and `MerchantStock.Quantity` settings.
- Kept random spawning, recipes, all-merchant distribution, loot, rewards, and native spellbook UI edits out of scope.

## 0.1.0

- Added first Broodmother-only command companion scaffold.
- Added `Broodmother's Call` custom spell item template cloned from native `Wolf's Call`.
- Added custom-spell-only runtime skill-reference rewrite to point `SummonPrefab` at the Avalon Awakened Broodmother template.
- Added default-disabled grant setting for the custom spell item.
- Added explicit-enable summon safety with no default hotkeys.
- Added resolver-only Broodmother template lookup through Avalon Awakened.
- Added one-session not-saved summon/swap, recall, dismiss, and disabled-by-default native defend assist.
