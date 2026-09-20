---
document_type: system
scope: FoA character progression layers
runtime: mono
evidence:
  static: DECOMPILED_AND_RUNTIME_CORROBORATED_BY_PROJECT_RESEARCH
last_verified: 2026-09-20
---

# Progression Layers

Use this page when you need to answer **which FoA progression system you are actually changing**.

FoA progression is not one unified skill system.

## The main layers

### Character level

Hero XP feeds the ordinary character-level path and related development resources.

### Proficiencies

Separate use-based skills have their own XP/progress state. Examples include weapon, movement, stealth, cooking, and magic-related proficiency sources.

### RPG stats

The main RPG attributes are:

- Strength
- Endurance
- Dexterity
- Spirituality
- Perception
- Practicality

These are not the same thing as proficiency XP.

### Talent trees

Talent/perk progression is spent through the character-sheet progression flow and has its own native availability, temporary selection, confirmation, and persistence behavior.

The ordinary upgrade route is also tied to native fireplace/rest context.

## Do not invent a proficiency from the UI

A visible talent group is not automatically a native proficiency.

For example, groups labelled around Critical Hits, Daggers, Health, or Attack Speed may represent talent organization without having their own independent proficiency XP store.

Before writing XP or progression logic, identify the exact native state that owns it.

## Mod-owned progression can still exist

A mod can add its own rank, insight, practice, or branch system around confirmed native signals.

Just keep the distinction explicit:

~~~text
native progression
≠
mod-owned progression overlay
~~~

If a mod later converts its own state into native XP/talent/stat changes, that conversion is a separate operation that should use the native owner deliberately.
