# Drake Resource and Entity Lifetime

Use this page when a rigid visual appears correct but leaks, duplicates, or breaks after equip/scene transitions.

Canonical overview: [Drake and MergedDrake](README.md).

## Resource ownership is shared

Drake can share mesh/material resources across multiple renderers.

The relevant lifecycle is closer to:

```text
first owner
→ acquire resource
→ additional shared owners
→ ECS entity use
→ owners release
→ final owner release
→ resource can be freed
```

Do not manipulate internal reference counts directly.

## Visual entity vs gameplay owner

For a movable weapon:

```text
Item/equip/hand owner
→ Drake presentation registration
→ ECS render entity
```

The ECS entity is not the weapon.

For scene-static MergedDrake:

```text
scene/build records
→ bulk entity creation
→ scene owner release
```

Do not treat MergedDrake as the normal path for runtime movable items.

## Mod replacement rule

When replacing a rigid presentation:

1. keep the gameplay owner intact;
2. acquire/load the replacement through the presentation path you actually use;
3. track only resources/entities your mod owns;
4. release them on the owner/equip/scene teardown boundary;
5. do not destroy shared native resources.

## Verification

Test repeated equip/unequip, scene transitions, item removal, mod disable and multiple instances using the same mesh/material.

A single visible render does not prove lifetime correctness.
