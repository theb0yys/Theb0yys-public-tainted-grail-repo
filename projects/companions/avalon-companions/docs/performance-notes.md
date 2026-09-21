# Performance Notes

## 0.1.0

- Harmony patches are limited to companion-panel input locking and return immediately while the panel is hidden.
- `Update` only calls a diagnostic gate. With default config, it exits after checking booleans.
- When `Diagnostics.LogPetSystemDiagnostics=true`, `Diagnostics.WritePetTemplateDump=true`, or `Diagnostics.WritePetCreatureShortlistDump=true`, diagnostics wait for common references, `Hero.Current`, and loaded templates, enumerate pet/summon/template data once, and stop. The pet/creature shortlist dump additionally scans loaded scene spawner attachments once.
- When `Diagnostics.WritePetCreatureDiagnosticToolCrosscheck=true`, the pet/creature shortlist pass also reads the latest FOA-Diagnostic Tool `spawner_refs.csv` once and writes comparison CSVs. This is file I/O during an explicit diagnostic pass only, not a normal-play loop.
- The optional CSV write is one-shot, disabled by default, and limited to this plugin's BepInEx config folder.
- When `Companions.EnablePetCompanionRoster=true`, the update path checks readiness and six keyboard shortcuts. Spawn/swap, recall, dismiss, and follow/stay paths only run after their shortcuts are pressed. Explicit panel Follow/Stay/Defend modes only run when clicked.
- When a one-session creature candidate is active and either `Companions.EnableNativeDefendAssist=true` or explicit Defend mode is selected, the update path checks the hero's `PossibleAttackers` on a throttled 0.35 second tick and only calls native `NpcHeroPetAlly.EnterCombat()` when a live attacker exists. Defend logging is throttled.
- The command panel draws only while open, caches its 1x1 style textures, and reuses a clamped IMGUI window rect. Responsive placement recalculates only when the panel opens or the screen size changes.
- The optional command status overlay draws only while a recent command message is active.
- `Diagnostics.WriteCompanionCommandLog=true` appends one CSV row only when a companion command runs, is blocked, or an auto catch-up occurs. It is not a per-frame diagnostic loop.
- No scene scanning, repeated reflection loops, or repeated logging hot path exists during normal play.
- Future companion behavior must avoid repeated scene-wide searches and log spam around AI/pathing updates.
