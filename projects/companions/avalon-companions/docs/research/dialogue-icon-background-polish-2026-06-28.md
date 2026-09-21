# Research: Dialogue Icon Background Polish

Date: 2026-06-28
Scope: add the existing shared companion icon and HUD background treatment to the supported plugin-owned Unity UI companion dialogue.
Question: Can the dialogue surface show the same companion identity art used by the roster panel and HUD badge without changing command behavior?

## Evidence read

- `mods/avalon-companions/docs/research/vanilla-style-dialogue-layout-2026-06-19.md`
- `mods/avalon-companions/docs/research/shared-icon-consumer-proof-2026-06-19.md`
- `mods/avalon-companions/docs/research/per-companion-icon-mapping-2026-06-20.md`
- `mods/avalon-companions/docs/research/command-surface-polish-2026-06-20.md`
- `mods/avalon-companions/docs/research/summon-panel-polish-2026-06-28.md`
- `mods/avalon-companions/docs/decisions/0005-unity-ui-dialogue-host.md`
- `mods/avalon-companions/src/Patches/PetCompanionController.DialogueUi.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Decision

The dialogue may render a non-interactive companion portrait frame inside the existing bottom dialogue band. The portrait uses the existing `GetRosterIconId` mapping and the existing `hud.companion-icon-background` shared texture ID through the optional Tainted Interface bridge.

Allowed:

- Add a non-raycasting Unity UI portrait/background frame to the bottom dialogue text band.
- Sync the portrait from the active managed companion's existing roster entry.
- Hide the portrait when no shared icon is available.
- Keep the right-side command rows, selected/hover/disabled states, direct-click fallback, and command audit rows unchanged.

Not allowed:

- Copying icon assets into Avalon Companions.
- Reading icon files directly from Avalon Companions.
- Changing command labels, command IDs, command delegates, policy gates, actor ownership, AI, movement, targeting, persistence, taming, training, saved progression, or Core-executed behavior.

## Validation needed

- `git diff --check -- mods/avalon-companions`
- Debug build.
- Release build.
- Live DLL deploy and hash check.
- In-game smoke: summon a managed companion, open `Companion`, confirm the bottom dialogue band shows the companion icon with the shared background when Tainted Interface is installed, confirm command choices still click, confirm Goodbye/Esc closes and restores input, and confirm the dialogue still works when the shared icon is unavailable.
