# Performance Notes

The probe uses loaded-scene object scans only on startup auto-run and manual hotkey. It does not poll every frame beyond checking the configured key and auto-run timer.

The scan is diagnostic-only and row-limited by `WeaponMaterialProbe.MaxRows` and `WeaponMaterialProbe.MaxMemberValues`.

Drake material evidence resolves only already-loaded materials through the game's loaded-material tracker. It does not start Addressables loads or register replacement materials.

The native item registrar caches all reflected member identities. Its component, attachment, and nested `TemplateReference` profiles are captured only for an explicit registration request, with 512 total nested-reference rows and 64 entries per reference collection as hard limits. Readiness normally uses the one-shot `TemplatesLoader.FinishedLoading` event; the fallback checks only while requests are pending and is throttled to once every 0.5 seconds.
