# Performance Notes

- Version 0.1.10 keeps the 0.1.9 Harmony input-lock behavior and only changes proof-panel sizing/button-state styling. The actor scanner, one-session ally proof, native quick commands, proof command panel, lifecycle diagnostics, proof-only Hold movement block, and panel input lock are bounded by config/proof state.
- The `Update()` and `LateUpdate()` work is limited to short config/hotkey guards plus a throttled command-maintenance tick only when one-session ally proof and either native quick commands or the proof panel are enabled. The disabled recruitment-test path, disabled scanner path, safe proof spawn path, ally proof spawn path, and ally proof dismiss path default off with `KeyCode.None`.
- The safe proof spawn path does one template lookup and one spawn only on explicit hotkey press.
- The one-session ally proof path does one template lookup, one spawn, one `NpcElement` check, one native faction override, and one `NpcHeroPetAlly` add only on explicit hotkey press.
- The native quick command path only touches the single active proof actor. Follow catch-up is throttled to 0.75 seconds and Defend assist is throttled to 0.35 seconds when explicitly enabled.
- The proof-only Hold command adds one plugin-owned `ICanMoveProvider` element to the active proof actor while Hold is selected and removes it when the actor returns to Follow, Come Close, Defend, or Dismiss. It does not scan the scene.
- The proof panel draws only while visible, caches its GUI styles/textures, and touches only the active proof actor. The 0.1.5 layout polish increases the visible panel size and button dimensions but does not add scene scans or new gameplay-loop behavior.
- The 0.1.9 panel flow polish adds status lines and a `Lifecycle Check` button. It does not add scene scans unless the existing gated `Scan Actors` button is clicked.
- The 0.1.9 input lock blocks `PlayerInput`, Rewired axes/buttons, game mouse-position updates, and captured Unity input modules only while the panel is visible.
- The 0.1.10 panel sizing/button-state polish adds no scene scans or gameplay-loop work. It only uses cached IMGUI styles/textures while the panel is visible and clears IMGUI control focus after clicked panel actions.
- The 0.1.6 panel scanner button only runs the existing bounded scanner when clicked and when the scanner config gate is enabled.
- The 0.1.7 lifecycle guard only inspects the tracked active proof actor. Lifecycle diagnostic ticks are throttled by `HumanLifecycle.LifecycleTickSeconds`, default `5`, and do not scan the scene.
- The scanner runs a bounded radius scan plus known pet/summon/ally marker checks only on explicit hotkey press, then writes three CSV files.
- The human NPC diagnostic map generator is an offline PowerShell tool. It reads existing dev diagnostic CSVs and writes repo-local generated review files; it does not run in game.
- Future diagnostics must avoid repeated scene-wide scans and must write bounded, user-triggered outputs only.
