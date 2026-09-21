# Exact Taming Tool Acquisition Probe — 2026-08-19

## Document control

- Owner: `mods/avalon-companions/`
- Canonical branch: `companions`
- Scope: creature/animal companion acquisition diagnostics only
- Implementation state: `PARTIAL` — source implemented; build and runtime validation remain `NOT_RUN`
- Item/weapon owner: Tainted Weapons
- AI Framework mutation: none
- Dialogue Framework mutation: none
- Human Companion mutation: none
- Quest service mutation: none

## Controlling ownership boundary

```text
Avalon AI owns AI.
The Dialogue Framework owns dialogue.
Avalon Human Companions owns human companions.
Avalon Companions owns creature and animal companions.
Tainted Weapons owns the Taming Bow and Taming Arrow identities and registration.
```

Avalon Companions does not define, clone, rename, register or catalogue the items. It requests an immutable item-set receipt from the owning Tainted Weapons service and records only what the exact pair means for a creature-acquisition candidate.

## Exact borrowed item contract

The probe consumes:

```text
TaintedWeapons.TaintedWeaponsTamingToolsApi.EnsureRegistered(out receipt)
```

It does not copy the custom GUIDs into companion-owned truth. The receipt supplies:

- registered state;
- status and reason code;
- Taming Bow template GUID/name;
- Taming Arrow template GUID/name.

The item-owner implementation is tracked separately on the canonical `tainted-weapons` branch and draft PR #345.

## Implemented source

- `src/Patches/ProjectileAcquisitionProbe.cs`
- `src/Patches/CompanionPatchRegistry.cs`
- `src/AcquisitionGlobalUsings.cs`

## Native seam

The probe patches the already-researched private FoA method:

```text
Awaken.TG.Main.AI.Fights.Projectiles.Arrow.OnContactEnding(
    HitResult,
    Collider,
    IAlive,
    bool environmentHit)
```

The Harmony hook is a read-only prefix. It changes no arguments, return values, projectile state, damage, ammunition, actors, factions, AI, dialogue, quests, saves or companion state.

A postfix on the existing Avalon Companions `Plugin.Update` method performs the bounded main-thread retry of the Tainted Weapons registration request while the diagnostic is enabled. No companion gameplay logic is executed by that postfix.

## Configuration

```text
AcquisitionProbe.Enabled = false
AcquisitionProbe.WriteCsv = true
AcquisitionProbe.BindFirstReviewedWolfHit = true
```

The diagnostic is disabled by default. When enabled, it waits for a registered Taming Tool receipt. Missing Tainted Weapons, a missing service API, a queued registration or a denied registration leaves the probe unarmed.

## Strict observation gate

A native projectile contact reaches the diagnostic only when both conditions are true:

```text
Arrow.SourceWeapon.Template.GUID
    == receipt.TamingBowTemplateGuid

AND

Arrow.ItemTemplate.GUID
    == receipt.TamingArrowTemplateGuid
```

`Arrow.SourceProjectile.Template` is retained only as a native fallback for ammunition identity when `Arrow.ItemTemplate` cannot be read.

Every other projectile is ignored, including:

- vanilla Shortbow + Wooden Arrow;
- Taming Bow + ordinary arrow;
- ordinary bow + Taming Arrow;
- every unrelated bow, arrow, spell or throwable.

This is the required baseline for all later subdual and acquisition logic.

## Runtime-only wolf binding

The reviewed target family remains:

```text
Spec_AnimalWolf
9086dee514edc644b9b55890d885db3f
```

The first player-fired contact from the exact Taming Bow/Taming Arrow pair can bind one wolf for the current scene by:

- exact `Location.ID`; and
- exact `NpcElement` object reference through `WeakReference<NpcElement>`.

Later exact-pair contacts report `Location.ID` and object-reference matches independently. Scene changes and probe toggles clear the wolf binding. No target identity is persisted.

## Output

```text
BepInEx/config/kane.tgfoa.avalon-companions/acquisition-projectile-probe.csv
```

The probe records at most 500 exact-pair contacts per diagnostic generation.

Rows include:

- Tainted Weapons receipt status/reason;
- `exactToolPair=true`;
- current-Hero provenance;
- external weapon and ammunition GUID/name/item name;
- projectile type;
- collider and resolved `IAlive` identity;
- hit coordinates;
- target `Location.ID` and template;
- reviewed-wolf classification;
- unique/summon/alive/discarded/unconscious readbacks;
- bound wolf identity and match results;
- outcome and rejection reason;
- explicit `mutation=false`, `ai=false`, `dialogue=false`, `quest=false`, and `persistence=false` fields.

## Required runtime matrix

| Test | Required result |
|---|---|
| Probe enabled before Tainted Weapons/service readiness | no projectile rows; bounded owner-service waiting receipt in log |
| Tainted Weapons registers the pair | exact GUIDs/names consumed; probe becomes armed |
| Vanilla Shortbow + Wooden Arrow into terrain/wolf | no probe row |
| Taming Bow + ordinary arrow into terrain/wolf | no probe row |
| Ordinary bow + Taming Arrow into terrain/wolf | no probe row |
| Exact Taming pair into terrain | one row; living target absent; no wolf binding |
| Exact Taming pair into first reviewed wolf | one row; exact pair recorded; wolf bound |
| Second exact-pair shot into bound wolf | both identity matches true |
| Exact-pair shot into another reviewed wolf | bound-target matches false |
| Exact pair fired by another actor into reviewed wolf | row retained as non-player provenance rejection |
| Scene transition | diagnostic generation changes and wolf binding clears |
| Exact-pair shot into another living target | row retained; no creature-companion transition |

## Explicit non-effects

The implementation does not:

- own or register the bow or arrow;
- grant inventory items;
- alter damage or ammunition;
- create or accumulate subdual;
- force unconsciousness;
- change faction or ally state;
- create a `CompanionId`;
- start or advance a quest;
- call Avalon AI;
- call the Dialogue Framework;
- persist a creature or pending acquisition;
- observe ordinary projectiles for companion logic.

## Validation state

```text
source pair defined by Tainted Weapons = PASSED_SOURCE
owner-service consumer boundary = IMPLEMENTED_SOURCE
exact-pair gate = IMPLEMENTED_SOURCE
ordinary-projectile exclusion = IMPLEMENTED_SOURCE
wolf runtime binding = IMPLEMENTED_SOURCE
branch/path containment = PASSED_SOURCE_REVIEW
build = NOT_RUN
plugin load = NOT_RUN
Tainted Weapons service reflection = NOT_RUN
Taming Tool registration = NOT_RUN
exact-pair native attribution = NOT_RUN
ordinary-pair negative controls = NOT_RUN
CSV output = NOT_RUN
scene invalidation = NOT_RUN
runtime mutation absence = NOT_RUN
subdual = NOT_RUN
unconsciousness = NOT_RUN
companion acquisition = NOT_RUN
```

## Next researched task

Build and deploy both the `tainted-weapons` and `companions` branches on the matching FoA Mono installation, enable `AcquisitionProbe.Enabled`, allow Tainted Weapons to register the exact Taming Bow and Taming Arrow, grant those named items through an existing item-service consumer, and execute the strict exact-pair/negative-control matrix before any subdual or unconscious implementation is authorised.
