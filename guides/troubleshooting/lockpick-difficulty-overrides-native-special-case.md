# Lockpick Preset Overrides a Special Lock's Difficulty

If a lock-specific authored difficulty no longer feels different, check whether your mod **replaced the final `LockAction.Tolerance` result** with a preset.

A final-result override can erase:

- lock-specific tolerance;
- visual-script reductions;
- other native modifiers already included in the effective result.

If preserving relative authored difficulty matters, compose with the native result instead of replacing it outright.
