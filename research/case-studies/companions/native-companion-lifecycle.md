# One-Session Native Companion Lifecycle

This pattern creates a controlled **one-session** companion while keeping FoA's native NPC/ally systems in charge.

## Working implementation lineage

Avalon Companions proved the Qrko spawn path in game and collected live lifecycle rows for managed creature companions using the native ally marker. Working rows kept managed actors marked not-saved and bounded duplicate/untracked cleanup.

## Native owners

- reviewed non-unique `LocationTemplate`
- spawned `Location`
- `NpcElement`
- native summon/ally faction path
- `NpcHeroPetAlly`

## Working sequence

~~~text
explicit player command
→ resolve one reviewed non-unique LocationTemplate
→ native SpawnLocation
→ immediately set Location.MarkedNotSaved = true
→ require a valid NpcElement
→ apply native hero-summon/ally ownership
→ attach NpcHeroPetAlly
→ track at most one active managed roster actor
→ use native follow/defend entry points
→ dismiss/discard through owned lifecycle cleanup
~~~

## Native defend handoff

The working companion route uses `NpcHeroPetAlly.EnterCombat()` after checking that the hero has a live attacker. It does not invent a separate target-selection system for this path.

## Lifecycle guard

Useful invariants from the working implementation:

- managed actors remain `MarkedNotSaved=true`;
- excess active roster actors are discarded;
- untracked one-session roster allies can be cleaned up;
- recall and recovery re-assert the not-saved posture.

## Keep this pattern one-session

Do not silently turn this into persistent companion ownership. Saved roster state, reload restoration and auto-respawn are different systems.
