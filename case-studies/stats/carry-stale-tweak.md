---
document_type: case
scope: Carry Weight Tweaks stale StatTweak correction
evidence:
  source: PROJECT_CORRECTION
last_verified: 2026-09-20
---

# Carry Tweak Must Follow the Current Stat Instance

A configured final carry capacity could still appear near vanilla even though the mod retained a tweak object.

The project corrected the ownership assumption: `HeroStatsWrapper.Initialize` can rebuild `EncumbranceLimit`.

The mod now discards its old helper and attaches a fresh non-saved tweak to the current stat.

## Lesson

A modifier can be perfectly valid but attached to an object the game no longer owns.
