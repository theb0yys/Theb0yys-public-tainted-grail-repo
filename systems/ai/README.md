<!-- Canonical Wave 5 native-system page split from docs/reference/AI_PERCEPTION_BEHAVIOR.md. -->
# AI, Perception, Combat Pressure, and Native Behavior Ownership

> **Document type: native system.** Intervention guidance from the legacy page now lives in [the canonical mechanic](../../mechanics/ai/intervention-patterns.md).

## What this system is

FoA AI is not one method or one state machine.

The research separates several responsibilities:

- perception and alert facts;
- target ownership;
- combat entry/exit;
- pursuit/search movement;
- combat action availability;
- attack-slot pressure;
- native behavior states;
- actor lifecycle;
- optional project-owned AI/framework policy layers.

A mod should change the narrowest native responsibility that matches the desired behavior.

## Who owns it in FoA

Important native owners/surfaces include:

- `NpcAI`;
- `NpcAI.AlertStack`;
- native AI states such as investigation/search;
- native movement helpers such as `Wander` / `Patrol`;
- `CombatDirector`;
- combat behaviors such as `CombatEnemyBehaviourBase` and `MeleeAttackBehaviour`;
- `NpcAIDistancesUtils`;
- actor `Location` / `NpcElement` lifecycle.

Project-owned AI frameworks can coordinate policy or commands, but they do not automatically replace native actor ownership.

## Important identities, types, and methods

Researched examples include:

- `NpcAI.AlertStack.NewPoi(200f, Hero.Current)`;
- `NpcAI.HeroVisible`;
- `NpcAI.InCombat`;
- `NpcAI.Working`;
- `NpcAI.AllWorkingAI`;
- `NpcAIDistancesUtils.LoseTargetDelayByDistanceToLastIdlePoint`;
- `NpcAIDistancesUtils.ShouldForceEndCombat`;
- `StateInvestigation`;
- `StateSearchForCriminal`;
- `CombatEnemyBehaviourBase.CooldownDuration`;
- `MeleeAttackBehaviour.RequiresCombatSlot`;
- `CombatDirector` attack-slot coordination;
- difficulty values such as `MaxEnemiesAttacking` and attack-action release timing.

## Where it exists in the lifecycle

A simplified native pursuit/combat path can look like:

~~~text
perception / crime / event
→ alert target or point of interest
→ native AI state changes
→ movement/search/pursuit
→ combat target acquired
→ combat director / attack-slot coordination
→ behavior executes
→ target lost
→ native loss-delay / return rules
→ combat exit
~~~

Different NPC families can use different behavior/configuration within this ownership model.

## Current proof boundary

The repository has strong native AI ownership research and many scoped source implementations.

That does not establish a universal custom AI replacement process. Existing native actors should remain under native ownership unless an explicit, reversible ownership-transfer contract is separately proven.
