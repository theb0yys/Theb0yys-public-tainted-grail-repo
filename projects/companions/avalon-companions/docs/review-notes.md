# Review Notes

## 0.1.31 HUD placement hotfix review

- Reviewer: Codex
- Date: 2026-06-20
- Status: Approved for local build and live DLL deploy; BepInEx load and in-game HUD smoke pending

Findings:
- No findings after Release build validation and live DLL hash validation.

Validation gaps:
- BepInEx load validation is pending for Avalon Companions 0.1.31.
- In-game HUD placement smoke is pending: top-left badge clears the Wyrd Scent/status overlay and still disappears when no managed companion is active.

## 0.1.30 command-surface polish review

- Reviewer: Codex
- Date: 2026-06-20
- Status: Approved for build/deploy slice; in-game command-surface smoke pending

Findings:
- No findings after Debug build, Release build, Release deploy build, and local/live DLL hash validation.

Validation gaps:
- In-game command-surface smoke is pending: open `Companion`, verify row states, click Follow/Stay/Goodbye, press Esc, confirm `KeypadPeriod` remains debug-panel-only, and inspect `companion-command-log.csv` for the new dialogue audit rows.
- Native dialogue decision, lifecycle validation, Core registration, custom AI, taming, training, and loyalty remain future gates.

## 0.1.29 current build review

- Reviewer: Codex
- Date: 2026-06-20
- Status: Approved for local build, live DLL deploy, and BepInEx load; in-game HUD smoke pending

Findings:
- No findings after Avalon Companions Release build validation and Tainted Interface embedded Release build/resource validation.

Validation gaps:
- Live deploy passed for the updated `AvalonCompanions.dll` and embedded `TaintedInterface.dll`; live hashes matched the current local Release outputs.
- BepInEx load validation passed for Avalon Companions 0.1.30 and Tainted Interface 0.1.4.
- In-game HUD smoke is pending: active managed companion badge appears with the Dragon background, updates per companion, and disappears after dismiss without command behavior changes.

## 0.1.28 current build review

- Reviewer: Codex
- Date: 2026-06-20
- Status: Approved for build/deploy/load and command-click smoke; icon and close/Esc/debug-only smoke pending

Findings:
- No findings after Debug build, Release deploy build, local/live DLL hash validation, BepInEx load validation, and command-click CSV validation.

Validation gaps:
- The fallback-specific `Avalon Companions dialogue direct choice clicked` marker was not observed; the validated command clicks likely used the normal Unity button path.
- Goodbye, Esc input restoration, and `KeypadPeriod` debug-only behavior still need separate observation.
- Debug-panel icon smoke is pending when Tainted Interface 0.1.2 is installed.

## 0.1.27 dialogue direct-click review

- Reviewer: Codex
- Date: 2026-06-20
- Status: Approved for build/deploy slice; in-game click smoke pending

Findings:
- No findings after Debug, Release, and Release deploy build validation.

Validation gaps:
- In-game direct-click smoke is pending for Follow/Stay/Goodbye/Esc and `KeypadPeriod` debug-only behavior.
- True native story dialogue, custom AI, taming, training, and loyalty remain future research gates.

## 0.1.25 dialogue input review

- Reviewer: Codex
- Date: 2026-06-20
- Status: Approved for build/deploy slice; fresh-load and in-game click smoke pending

Findings:
- No findings after Debug, Release, and Release deploy build validation.

Validation gaps:
- Fresh BepInEx load validation for 0.1.25 is pending because the game process was already running after deploy.
- In-game dialogue click smoke is pending after restart for Follow/Stay/Goodbye/Esc and `KeypadPeriod` debug-only behavior.
- True native story dialogue, custom AI, taming, training, and loyalty remain future research gates.

## 0.1.24 shared icon consumer proof review

- Reviewer: Codex
- Date: 2026-06-19
- Status: Approved for build slice; in-game icon smoke pending

Findings:
- No findings after Release build validation.

Validation gaps:
- In-game debug-panel smoke is pending: install Tainted Interface with the curated icon catalog, open `Avalon Companions Debug`, select a wolf/bear/pet row, and confirm the icon renders without changing command behavior.
- True native story dialogue, custom AI, taming, training, loyalty, and persistence remain future research gates.

## 0.1.23 vanilla-style dialogue layout review

- Reviewer: Codex
- Date: 2026-06-19
- Status: Approved for build/deploy slice; prompt-layout smoke pending

Findings:
- No findings after Debug, Release, and Release deploy build validation.

Validation gaps:
- In-game prompt/layout smoke is pending for the new right-side choices plus bottom text band.
- The `layout=vanilla-style` marker has not been observed yet because the native `Companion` prompt still needs to be opened after the 0.1.23 load.
- True native story dialogue, custom AI, taming, training, and loyalty remain future research gates.

## 0.1.22 Unity UI dialogue host review

- Reviewer: Codex
- Date: 2026-06-19
- Status: Approved for build/deploy slice; load/in-game validation pending

Findings:
- No findings after correcting the initial build failure.

Validation gaps:
- In-game prompt/dialogue click smoke for Unity UI buttons, close/Esc, and debug-panel separation is pending.
- BepInEx load is confirmed for 0.1.22. Debug-panel open/close was observed in the live log tail, but repeatable archived log evidence remains pending.
- True native story dialogue, custom AI, taming, training, and loyalty remain future research gates.

## 0.1.20 Custom dialogue interface review

- Reviewer: Codex
- Date: 2026-06-19
- Status: Approved for build/deploy slice

Findings:
- No findings after Debug and Release deploy build validation.

Validation gaps:
- BepInEx load validation for `Avalon Companions 0.1.20 loaded` is pending.
- In-game prompt/dialogue/input smoke for `Companion`, all visible choices, close/Esc, and `Debug.EnableDebugPanel=false` is pending.
- True native story dialogue, custom AI, taming, training, and loyalty remain future research gates.

## 0.1.19 Debug panel roadmap review

- Reviewer: Codex
- Date: 2026-06-19
- Status: Approved for debug-panel build/deploy slice

Findings:
- No findings for the debug-panel label/config gate after build validation.

Validation gaps:
- BepInEx load validation for `Avalon Companions 0.1.19 loaded` is pending.
- In-game prompt/panel smoke for `Companion Debug`, `Avalon Companions Debug`, and `Debug.EnableDebugPanel=false` is pending.
- Native custom dialogue, custom AI, taming, training, and loyalty remain future research gates.

## 0.1.18 Core diagnostic bridge review

- Reviewer: Codex
- Date: 2026-06-19
- Status: Approved for diagnostic bridge build slice

Findings:
- No findings after correcting the initial string-literal build failure.

Validation gaps:
- In-game companion behavior was not re-smoked because the bridge only logs read-only Core diagnostics.
- Adapter registration, companion-profile registration, and advanced companion behavior remain blocked.

## 0.1.17 Core handoff design review

- Reviewer: Codex
- Date: 2026-06-19
- Status: Approved for design/version slice only

Findings:
- No findings for the 0.1.17 Core handoff design slice.

Validation gaps:
- In-game smoke was not run because runtime behavior did not change beyond version metadata.
- Avalon Core load-order, optional dependency behavior, read-only trust-report access, descriptor registration, and companion-profile registration remain future validation gates.
- Advanced companion behavior remains blocked.

## Implementer self-review

- Diff limited to intended files: yes, new `mods/avalon-companions/` scaffold only.
- Research/design notes updated: yes.
- No generated binaries or game files included: yes.
- Dangerous file/network/process behavior absent or justified: yes; optional file output is a disabled-by-default CSV under this plugin's BepInEx config folder, and pet companion controls are config-gated.
- Hot-path performance checked: yes; the `Update` paths are disabled-by-default diagnostics and config-gated pet companion hotkeys.
- Player-facing UI checked against `docs/in-game-ui-quality-standard.md`: not applicable; no UI exists.
- Validation notes updated: yes.

## Code review

- Reviewer: Codex
- Date: 2026-06-14
- Status: Approved for scaffold and pet companion roster build

## Findings

- No findings for the 0.1.0 scaffold.
- Pet companion roster controls still require an in-game smoke check before release.

## Validation gaps

- Diagnostics-enabled game launch and BepInEx log validation passed on 2026-06-14.
- Prototype spawn behavior passed in the BepInEx log on 2026-06-14.
- Pet companion roster controls have not yet been smoke-tested in game.
- First roster smoke found command routing was too strict after a spawned Qrko no longer passed the managed marker check. The controller now scopes command adoption to the four validated roster GUIDs instead of the marker alone.
- Follow-up roster smoke showed command input was detected but too invisible. The controller now displays short command status text and makes next/previous swap the active pet immediately.
- Added a styled command panel for pet controls. Screenshot `Screenshot (3570).png` showed the fixed-size version was too small on the active display, so the panel now uses the FoA Mod Manager's responsive/clamped IMGUI window pattern. It still needs in-game visual validation before release-ready UI claims.
- Explicit Follow/Stay/Defend panel modes now build. Defend mode still uses only the native `NpcHeroPetAlly.EnterCombat()` path when the hero has live attackers, and the automatic defend assist remains the fallback outside Defend mode. In-game command-mode behavior, attack-response, civilian safety, transition, rest, quit, reload, return, and persistence behavior have not yet been validated.
