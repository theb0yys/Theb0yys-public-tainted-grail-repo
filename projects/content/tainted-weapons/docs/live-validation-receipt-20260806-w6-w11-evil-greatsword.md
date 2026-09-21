# Tainted Weapons W6-W11 Live Validation Receipt - Evil Greatsword

## Scope

- Date: 2026-08-06
- Receipt type: `TaintedWeaponsLiveValidationReceipt`
- Target item: `kane.tgfoa.evil-greatsword-moveset:evil-greatsword`
- Deployed framework: `Tainted Weapons 0.3.6`
- Live log checked: `<local-path>`
- Live log timestamp checked: `2026-08-06 01:51:18` local time.
- Process state after validation check: no `Fall`, `Avalon`, `Tainted`, or `UnityCrashHandler` process found.

## Evidence Basis

- Weapons report line 183 requires the W6-W11 route to follow native equip selection, redirect only receipt-owned GUIDs, build or reuse a prototype, apply Drake bindings, and prove method order plus ownership transitions.
- Validation matrix lines 172-177 define W6-W11 as equip, unequip loop, two instances, hide/show, scene transition, and UI isolation.
- Tainted Weapons research line 50 requires the user to start or restart the game manually; Codex must not control the live game process.

## Observed Runtime Path

- Startup observed: `Tainted Weapons 0.3.6 loaded`.
- Capability receipt observed: `version=1; ready=True; denialReasons=ok`.
- Framework registration observed: Evil Greatsword accepted with `assetResolved=True`, `drakeReady=True`, `identityReservation=reserved`, mesh `Evil_eye_greatsword_LowUV1`, and material `MI_EvilGreatsword`.
- Native equip redirect observed: `Tainted Weapons equipped visual redirect; redirected; key=kane.tgfoa.evil-greatsword-moveset:evil-greatsword; source=address=a7e2b7bb330757446877f620de7cf755; target=mod://kane.tgfoa.tainted-weapons/equipped-prototype/kane.tgfoa.evil-greatsword-moveset/evil-greatsword-e7bbc85788c0`.
- Framework Drake prototype build observed: `Tainted Weapons Drake prototype built`, prototype `TaintedWeapons_evil-greatsword_e7bbc85788c0_RuntimeEquippedPrototype`, `characterHands=1`, `drakeLodGroups=1`, `drakeMeshRenderers=1`, `unityRenderers=0`.
- Framework prototype handle observed through the equipped prototype address with `route=built-framework-prototype`, then `route=cached-framework-prototype`.
- Drake loading-manager mesh key served: `type=Mesh`, asset `Evil_eye_greatsword_LowUV1`, `previousCounter=0`, `started=True`.
- Drake loading-manager material key served: `type=Material`, asset `MI_EvilGreatsword`, `previousCounter=0`, `started=True`.
- Negative scan observed no `identity-collision`, `queued-identity-collision`, `capability-blocked`, `framework runtime capabilities blocked`, `framework registration blocked`, `could not patch`, `Type=UnityEngine.Material`, or `InvalidKeyException`.
- Clean shutdown observed in Avalon Exceptions session state with `cleanShutdown = true`.

## W6-W11 Result

| Gate | Result | Evidence |
|---|---|---|
| W6 equip | Pass, log-level | Native `ItemEquip.GetHeroItem` redirect occurred, framework prototype was built, and Drake served registered mesh/material keys. |
| W7 unequip loop | Not run | No 100-cycle unequip/equip loop evidence exists in the checked log. |
| W8 two instances | Not run | No two-instance fixture evidence exists in the checked log. |
| W9 hide/show | Not run | No native hide/show visibility fixture evidence exists in the checked log. |
| W10 scene transition | Not run | No equipped scene-transition restore fixture evidence exists in the checked log. |
| W11 UI isolation | Partial pass | Negative scan found no UI Addressables type coercion or `InvalidKeyException`; exact creator/inventory/map/menu screen coverage was not separately proven. |

## Implementation Verdict

- The DrakeRendererLoadingManager-specific registered-key route is present and live: `framework Drake mesh key served` and `framework Drake material key served` were both observed.
- A global Addressables mesh/material hook is not required by this evidence and must remain disabled.

## Remaining Blocks

- Full W7-W10 lifecycle fixtures still require dedicated manual runtime action or a researched validation harness.
- W11 needs explicit creator, inventory, map, and menu coverage before it can be recorded as a full pass.
- This receipt proves log-level custom Drake equip/presentation routing, not screenshot or video-level visual acceptance.
