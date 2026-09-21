# Performance Notes

- No Harmony patches are registered.
- `Update` exits immediately unless the mod is enabled, the disabled summon-test gate is enabled, and a non-`None` hotkey is configured.
- No scene scans, template scans, file writes, or repeated logs run in the hot path.
