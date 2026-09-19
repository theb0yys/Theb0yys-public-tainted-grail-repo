# Visual Effects Cookbook

## Damage and death sidecars

- [Character damage observer](../23-character-damage-observer-mono/README.md)
- [Character death observer](../42-character-death-observer-mono/README.md)
- [Combat VFX sidecar recipe](../recipes/09-combat-vfx/README.md)

The working pattern is:

~~~text
native damage/death event
→ classify the event
→ spawn only mod-owned presentation
→ cap active effects
→ clean them up
~~~

Do not replace native damage ownership just to add presentation.
