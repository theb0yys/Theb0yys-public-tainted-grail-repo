# Research: Session Continuity and Persistence Gate

Date: 2026-06-16
Scope: define the next gate for session continuity without approving actor persistence, reload restoration, respawn, or new combat commands.

## Evidence read

- `mods/avalon-companions/docs/research.md`
- `mods/avalon-companions/docs/design.md`
- `mods/avalon-companions/docs/validation-plan.md`
- `mods/avalon-companions/docs/research/pet-and-summon-targets-2026-06-14.md`
- `mods/avalon-companions/docs/research/transition-save-lifecycle-gate-2026-06-15.md`
- `mods/avalon-companions/docs/research/lifecycle-validation-checkpoint-2026-06-15.md`
- `mods/avalon-companions/docs/research/managed-companion-recovery-2026-06-16.md`
- `mods/avalon-companions/src/Plugin.cs`
- `mods/avalon-companions/src/Patches/PetCompanionController.cs`

## Current evidence

- The approved companion path is still a one-session, explicit-command, native ally path.
- Managed companion actors are marked `MarkedNotSaved=true` during summon, recall, lifecycle checks, shutdown, native action attachment, and recovery.
- The lifecycle guard is already approved to remove untracked one-session roster allies and excess active roster actors.
- The recovery command is already approved to recall live managed actors or remove dead/invalid managed actors, but not to heal, revive, respawn, restore, or persist them.
- The plugin already persists low-risk player preferences through config, including selected roster index and follow range profile.
- `PetElement`, `PetVariantBase`, `NpcAlly`, `NpcHeroSummon`, and `NpcHeroPetAlly` show native save/travel behavior, but prior research has not proven a safe plugin-owned save record or reload restoration path.

## Decision

The next researched step is a session-continuity and persistence gate, not additional combat command behavior.

Avalon Companions may research and, after validation, implement only continuity behavior that keeps actor ownership one-session and player-commanded:

1. Metadata-only continuity: remember selected roster index, selected roster GUID, last command mode, and follow range in plugin config or plugin-owned diagnostic state.
2. Status-only reload feedback: after load, report that the previous companion selection is available to summon again, without creating an actor.
3. Manual continuity command: allow the player to explicitly summon/swap the remembered selection after reload through the existing summon path.
4. Same-session live actor re-adoption: only consider a live actor already present in the world when it is a reviewed roster GUID, has the native ally marker or approved Qrko pet identity, is marked not saved, and passes the lifecycle guard.
5. Diagnostics first: any implementation must write command/lifecycle rows proving whether the actor was still live, missing, discarded, re-adopted, or only remembered as metadata.

## Boundary

This gate does not approve:

- changing managed companions to saved actors,
- setting managed companion locations or actions to saved ownership,
- automatic respawn on load,
- automatic companion restoration after quit/reload,
- resurrection or healing during recovery,
- active squads or multiple persistent companions,
- custom save records that own game actors,
- quest, story, dialogue graph, vanilla pet variant, or serialized interaction edits,
- broad adoption of wild actors,
- humanoid companion persistence,
- new attack buttons, target selection, or combat AI.

If quit/reload removes a managed companion because it was marked not saved, that remains the expected result. A later player command may summon the remembered selection again only through the existing explicit one-session summon path.

## Implementation gate

Do not implement persistence until these checks are complete:

1. Inspect the latest `companion-lifecycle.csv` from 0.1.12 and 0.1.13 smoke tests.
2. Inspect the latest `companion-command-log.csv` from 0.1.12 and 0.1.13 smoke tests.
3. Confirm fast travel and area-transition rows show no duplicate active companion and no orphan native prompt.
4. Confirm rest rows show no duplicate, saved copy, or unmanaged action left behind.
5. Confirm save, quit, reload, and return either removes the one-session actor or discards any untracked leftover through the lifecycle guard.
6. Confirm no rows imply `touchesPersistence=true`, saved command action ownership, or actor respawn.

Only after that evidence is clean may the next code slice add metadata-only continuity and status text. Actor restoration, reload respawn, and saved companion ownership remain blocked by this gate.

## Validation required

- Debug and Release build validation for any later code change.
- Live DLL hash/version verification for any later deploy.
- In-game continuity smoke test on a throwaway save:
  - summon one managed one-session candidate,
  - set Follow, Stay, Defend, and follow range at least once,
  - fast travel or transition and inspect lifecycle rows,
  - rest and inspect lifecycle rows,
  - save, quit, reload, and return,
  - confirm no companion auto-respawns,
  - confirm the remembered selection is metadata/status only until the player explicitly summons again,
  - confirm command and lifecycle logs still report `touchesPersistence=false`.
