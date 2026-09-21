# Tainted Armour FoA Observer

`Tainted.Armour.FoA` is the Tainted Armour-owned BepInEx transport for the
A2K-T34 live visual-runtime observer route.

It is a no-write observer plugin:

- hotkey: `F6` by default;
- no-hotkey trigger: increment `A2K-T34.CaptureRequestId` while
  `A2K-T34.PollConfigForCaptureRequest=true`;
- config-triggered captures wait by default until the observer sees
  `CampaignMap_HOS` hero/controller/body-renderer context, then write a
  packet from that live context; pose binding is recorded in the packet and may
  still fail closed if the target TPP clip binding is not proven;
- output: `BepInEx/config/theboyyss.tgfoa.tainted-armour.foa-observer/`;
- packet: `a2k_t34_live_observer_packet.json`;
- route: `bounded_no_write_live_foa_runtime_body_equip_metric_observer`;
- candidate-map application, conversion, sidecar generation, source FBX
  mutation, Unity project asset mutation, native game-file writes, runtime
  loader changes, item registration, equip, inventory, and save writes remain
  false.

The first transport slice captures live FoA hero/body renderer context,
reflected Kandra renderer/rig context, Animator current-clip evidence, and live
RuntimeAnimatorController clip-binding evidence. It feeds a real
`VisualRuntimeObservationRequest` payload shape, but it keeps clipping, seam
visibility, bounds/camera-culling, body-cover, and material/shadow/layer metrics
blocked unless the live runtime capture actually proves them.

The Kandra scan distinguishes active-scene Kandra renderers from loaded
player-body/equip candidates. Player-body candidates are read-only matches
against researched hero/FPP/TPP owner tokens such as `VHeroController`,
`HeroMale_TPP`, `HeroFemale_TPP`, and `Mesh_BaseHuman_HeroTPP`, while rejecting
known NPC ownership tokens. Candidate discovery is diagnostic only: it never
enables, disables, reparents, equips, registers, converts, or writes renderers.

The packet also includes
`runtimeContext.kandraRegistrationContractDiagnostic`, a reflection-only
fingerprint of the current live Kandra registration surface. It records the
loaded `Awaken.Kandra.dll` identity, the `KandraRendererManager.Register`
signature, required `KandraRenderer.RendererData`/`KandraMesh` member surface,
manager-chain readiness method signatures, deferred lifecycle markers, and
no-write boundary flags. It does not invoke `Register`, `CanRegister`, renderer
creation/activation, conversion, item/equip/save mutation, or game writes.

Pose-binding owner discovery first scans the captured live hero roots, then adds
a read-only loaded-object scan for player TPP `ARHeroAnimancer` owners matching
the same TPP identity tokens. It records whether the target
`Anim_Hero_TPP_Base_Knockdown_Air_Loop` clip is bound; it does not switch
perspective, instantiate or activate TPP bodies, or mutate animation state.

This plugin does not launch the game and does not make armor release-ready.
