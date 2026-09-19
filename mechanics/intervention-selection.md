<!-- Canonical documentation path. Migrated from docs/reference/HOOKS_AND_INTERVENTION_PROCESS.md. -->
# Hooks and Intervention Rules

Use the game's real owner and intervene as narrowly as possible.

## Result adjustment

Use a postfix when the game should perform its normal calculation and the mod only adjusts the returned result.

## Action guard

Use a prefix/guard when the game owns the action and the mod only decides whether that exact action may continue.

## Observation

Use a postfix/event observer when the mod only needs completed game state for UI, VFX, audio or diagnostics.

## Ownership rules

- keep targeting, inventory transfer, damage calculation and persistence native unless the mod explicitly owns them;
- filter to the exact actor/item/context the feature supports;
- do not patch a broad manager when a narrower owner exists;
- undo mod-owned runtime state on disable/unload;
- keep hot-path work bounded.

See [Hook Catalogue](../docs/reference/HOOK_CATALOGUE.md) for working game surfaces.
