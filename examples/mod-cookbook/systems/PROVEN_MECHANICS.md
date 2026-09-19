# Proven Mechanics Index

This index connects the maintainer's working-mod evidence to reusable public cookbook directions.

It does **not** copy private-project implementations. It records mechanism lineage and evidence scope so public clean-room examples can be derived from proven behaviour instead of guessed APIs.

| Working owner / proven scope | Runtime evidence | Native mechanism / owner | Reusable public pattern | Current cookbook relationship |
| --- | --- | --- | --- | --- |
| Magic Tweaks 0.1.0 | Feature-tested player projectile speed/range/homing behaviour | native magic projectile configuration and homing paths | projectile speed/range/homing tuning with owner filtering | [04 Magic projectile speed](../04-magic-projectile-speed-mono/README.md) has the strongest direct relationship; exact public rewrite NOT_RUN |
| Hold to Steal 0.2.1 | direct, container, take-all and readable theft paths user-tested; plugin load confirmed | direct pickup, container transfer/take-all and readable steal paths | conditional theft/interaction guard | [06 Modifier-gated illegal pickup](../06-illegal-pickup-guard-mono/README.md); exact public rewrite NOT_RUN |
| Immersive Footsteps 0.1.2 proof scope | exact FMOD path loaded; custom banks loaded; walking trace captured custom footstep plays with native suppression | FMOD one-shot interception filtered to hero footsteps | bounded native-footstep replacement/additive policy | [07 Footstep replacement](../07-footstep-beep-replacement-mono/README.md); exact public rewrite NOT_RUN |
| Tainted Blood working release lineage, including 1.0.3 confirmation | confirmed working in game with repeated screenshot/live correction history | character damage and terminal death event VFX lifecycle | damage/death VFX sidecars, bounded presentation and cleanup | [23 Character damage](../23-character-damage-observer-mono/README.md), [42 Character death](../42-character-death-observer-mono/README.md), [Combat VFX recipe](../recipes/09-combat-vfx/README.md) |
| Avalon Mounts 0.1.3 N3 Proof125 | active native running/turning getter use; user-confirmed mount/movement/transition/save-load/relaunch smoke on tested stack | `VMount.RunningVelocity` and `VMount.TurningVelocity` | native getter-based mount velocity tuning | no clean-room public pattern yet |
| Views of Avalon 0.2.8 | feature-tested; live screenshots showed major fog/visibility change | existing active HDRP/FoA volume-profile fog components reached by reflection | capture/apply/restore existing fog state | no clean-room public pattern yet |
| Tainted Weather controlled 0.1.0 proof scope | live bundle/prefab render probes and controlled LightRain executor call | Weather Maker runtime prefab/profile execution | bounded asset-bundle/profile execution with teardown | no public advanced weather pattern yet; normal-play weather is not broadly proved by this row |
| Tainted Economy validated vendor-price lane | throwaway-save buy-side, sell-side, common-gear resale and disable/reload rollback validation | native vendor price calculation surface | direction-aware vendor price tuning with rollback | current merchant examples 11/12 are different mechanisms; no direct public pattern yet |
| Better Bonfire Menu 0.6.3 | direct user runtime feedback that restored service entries and service grid worked correctly | native fireplace/bonfire service methods | route to native camp services instead of reimplementing them | no clean-room public camp-services pattern yet |
| Avalon Cheat Panel 0.10.7 item grant flow | user screenshot showed one item grant completed and receipt reported success | loaded existing item template → native item object → hero inventory add path | validated existing-native-item grant | no clean-room public item-grant pattern yet |
| Avalon Companions Qrko/one-session lineage | prototype spawn passed in game; lifecycle/native ally evidence exists at documented scopes | native location spawn + not-saved posture + native pet/ally ownership | one-session companion lifecycle | no public advanced companion pattern yet |
| Avalon Human Companions proof lineage | proof actor/panel commands and input-lock slices user-smoke-tested and log-confirmed | one-session proof actor + bounded command UI/input ownership | actor command UI with explicit input/cursor lifecycle | no public advanced pattern yet |
| Tainted Instincts proven profiles | Outlaw feature-tested; multiple exact creature profiles reached documented runtime/Level-4 validation | exact identity gates plus native aggression/sight/melee/cooldown/slot paths | exact-target NPC tuning while preserving unrelated native AI | no public advanced NPC-AI pattern yet |
| Wyrd Decoy working decoy route | working behaviour confirmed by user report/logs; inventory requirement/consumption path exercised | fail-closed provider API bridge plus native carried-item removal after success | cross-mod API + transactional resource consumption | no public cross-mod gameplay pattern yet |
| Wyrd Hunt fixed Wyrdspirit lane | first-target path live validated; follow-up reported fight/kill, leave-return and save-load working | exact native encounter selection/spawn/combat/death lifecycle | bounded exact-identity encounter lifecycle | no public advanced encounter pattern yet |

## Reading this table correctly

A row proves only the stated working-owner mechanism and scope.

It does not automatically prove:

- later versions that changed the mechanism;
- a related method with similar semantics;
- every option in the working mod;
- the clean-room public rewrite;
- IL2CPP portability;
- release compatibility outside the observed environment.

Use this index to decide **what is worth publishing next**, not to inflate evidence labels.
