# Research: Custom Dialogue UI System Host

Date: 2026-06-19
Scope: correct the 0.1.21 dialogue route after in-game validation showed the native `Companion` prompt still opened a custom panel-style surface instead of a real dialogue options UI.
Question: Should the next companion UI slice clone vanilla dialogue or build a custom dialogue UI through Unity UI while preserving the native-safe companion lane?

## Evidence read

- `docs/in-game-ui-quality-standard.md`
- `mods/avalon-companions/docs/research/native-dialogue-interaction-surface-2026-06-15.md`
- `mods/avalon-companions/docs/research/custom-dialogue-interface-2026-06-19.md`
- `mods/avalon-core/docs/decisions/0037-shared-custom-ui-layer-contract.md`
- `mods/Tainted Interface/src/TaintedInterfaceApi.cs`
- `mods/Tainted Interface/docs/design.md`
- `mods/Tainted Interface/docs/validation-plan.md`
- `mods/foa-mod-manager/README.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`
- `mods/avalon-companions/src/Patches/PanelInputLockPatch.cs`
- User screenshot/report on 2026-06-19 showing the surface as a custom panel, not a dialogue options UI.

## Findings

- The 0.1.21 route improved styling and input-scope handoff, but it still renders the companion command surface through `OnGUI`, `GUI.Window`, and `GUILayout`. That is a panel implementation, not a native or custom dialogue UI host.
- Tainted Interface proof 0.1.0 currently exposes IMGUI styles, helper wrappers, cursor support, and custom UI scope. It does not yet expose a full renderer, prefab host, or native dialogue clone.
- True vanilla `VDialogue` remains blocked because prior decompilation shows `DialogueAction`, `PetTalkAction`, and `StoryInteractAction` depend on valid game-authored `StoryBookmark` and `StoryGraphRuntime` data. There is still no evidence that this BepInEx plugin can safely author/register baked story graph content at runtime.
- A local Unity UI host is available now because Avalon Companions already references Unity UI assemblies, and `UnityEngine.UI` gives a real `Canvas`, `Button`, `Text`, `Image`, `GraphicRaycaster`, and `EventSystem` route.
- The existing IMGUI debug panel can keep disabling Unity `BaseInputModule` instances because IMGUI receives events directly. A Unity UI dialogue must not disable those input modules, or its buttons may render but fail to click.
- The existing FoA Mod Manager/Tainted Interface custom UI scope remains useful for cursor ownership and world freeze. The Harmony input lock can still block gameplay input while the dialogue is visible.

## Approved route for 0.1.22

- Keep one runtime-only native `Companion` action as the game-owned world prompt.
- Replace the `Companion` prompt's `GUI.Window` dialogue surface with a plugin-owned Unity UI `Canvas` dialogue host.
- Keep the `KeypadPeriod` debug panel as IMGUI and explicitly debug-only.
- Wire Unity UI buttons to the same existing approved command methods: Follow, Stay, Defend, Close/Normal/Far range, Come Close, Recall, Recover, Dismiss, and Goodbye.
- Use FoA Mod Manager/Tainted Interface custom UI scope for cursor ownership/world freeze when present.
- Keep Unity event modules available for the dialogue host, while preserving the stricter BaseInputModule disable path for the IMGUI debug panel.

## Not approved

- Fake `StoryBookmark` values.
- Runtime-authored story graphs.
- `DialogueAttachment`, `PetTalkAttachment`, `StoryInteractAction`, or template edits.
- Custom AI/pathing.
- Attack buttons, target selectors, healing, resurrection, squads, persistence, reload restoration, taming, training, or loyalty.
- Calling the 0.1.22 Unity UI host true vanilla dialogue.

## Validation needed

- Debug build.
- Release build and deploy hash check before live use.
- BepInEx load log showing `Avalon Companions 0.1.22 loaded`.
- In-game throwaway-save smoke: summon one managed one-session companion, activate the native `Companion` prompt, confirm a Unity UI dialogue surface opens rather than the IMGUI debug panel, click each command once, confirm commands close the dialogue, confirm `Esc`/Close/Goodbye close and restore cursor/game input, and confirm `KeypadPeriod` still opens only `Avalon Companions Debug`.
- Conflict-state screenshot or note before any release-ready UI claim.
