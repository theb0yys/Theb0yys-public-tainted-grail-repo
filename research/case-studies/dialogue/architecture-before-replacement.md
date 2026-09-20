# Architecture Before Replacing Native Dialogue

Dialogue Overhaul did not treat a third-party conversation package as permission to replace FoA Story globally.

The research first separated:

- native Story identity/lifecycle;
- external conversation presentation;
- AI intent;
- canonical FoA state;
- semantic consequence commits;
- persistence;
- interruption/fallback;
- migration per exact binding.

Implementation remained blocked where package fingerprints and native runtime hooks were missing.

## Lesson

Dialogue replacement is a **state-integration problem**, not primarily a UI/conversation-file problem.
