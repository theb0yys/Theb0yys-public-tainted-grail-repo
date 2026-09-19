# Gameplay Cookbook

## Magic projectile tuning

[Magic projectile speed](../04-magic-projectile-speed-mono/README.md)

Uses the native projectile configuration path to scale player-owned magic projectile speed while leaving unrelated projectiles alone.

## Theft and interaction guards

[Modifier-gated illegal pickup](../06-illegal-pickup-guard-mono/README.md)

Uses the same interaction families exercised by the working Hold to Steal implementation: direct pickup, container transfer/take-all and readable theft.

## Character damage

[Character damage observer](../23-character-damage-observer-mono/README.md)

Uses the character damage lifecycle as an observation/sidecar seam without replacing native damage calculation.

## Character death

[Character death observer](../42-character-death-observer-mono/README.md)

Uses the terminal character death lifecycle. Corpse, loot and reward handling remain separate game systems.
