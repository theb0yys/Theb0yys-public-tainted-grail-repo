# Dragon Knight AI Package

Current status: AIR-49 Boss AI package contract implemented and fixture-validated.

Dragon Knight needs its own AI package lane. The package should adapt the proven Avalon AI Runtime method:

- actor observation schema;
- blackboard facts;
- goal policy;
- planning policy;
- action-gateway boundary;
- execution lease;
- fail-closed host activation and release;
- explicit validation gates before runtime dispatch.

Implemented now:

- `DragonKnight.AI.Package.V2`, a `netstandard2.1` package assembly;
- `AvalonAI.Contracts.V2` reference only;
- stable Dragon Knight Boss AI package identity `dragon-knight.boss.ai.v2`;
- boss role `dragon-knight.boss`;
- actor, owner/lease, target, Rabbit, GOAP, capability, FoAHost bridge, and phase policy validators;
- Dragon Knight boss goals, actions, costs, preconditions, effects, cooldown metadata, and no-direct-native action contracts;
- required capabilities and one bounded `avalon.core.use-interactable.v1` procedure requirement;
- no persistent keys;
- offline fixtures for AIR-49 identity, manifest shape, fail-closed policy, target rejection, Rabbit/GOAP/capability/bridge/phase policy, Runtime V2 offline registration, and contracts-only assembly boundary.

No live runtime registration, host mapping, third-party AI runtime dependency, boss action dispatch, companion action dispatch, targeting, movement, combat, save, or persistence behavior is implemented yet.
