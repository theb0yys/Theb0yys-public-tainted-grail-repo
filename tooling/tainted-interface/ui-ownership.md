# Tainted Interface vs FoA Mod Manager

These tools solve different UI problems.

## Tainted Interface owns

- visual styles;
- textures/icons;
- shared visual resource packs;
- presentation helpers.

## FoA Mod Manager owns

- shared custom-UI active scope;
- cursor/controller cursor;
- gameplay-input suppression;
- optional world freeze;
- settings/status/controller-action management.

## Your mod owns

- feature-specific screen structure;
- commands;
- feature state;
- gameplay consequences;
- save/config semantics.

A common integration is:

```text
FoA Mod Manager acquires modal scope
→ feature creates its view
→ Tainted Interface supplies styles/resources
→ feature handles commands
→ feature destroys view
→ Mod Manager releases scope
```
