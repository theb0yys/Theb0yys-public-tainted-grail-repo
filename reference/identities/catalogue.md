# Identity Catalogue

> **Reference page.** Use this for curated identities that appear in the public examples. This is not a complete dump of game GUIDs.

## What this system is

The catalogue provides a small number of exact identities so examples can be concrete without turning the repository into an extracted game database.

## Who owns it in FoA

Native identities belong to FoA. Custom identities belong to the example/mod that creates them.

The public repository records identities only when they are needed to explain or reproduce a documented path.

## Important identities, types, and methods

### Item example

| Kind | Name | Exact identity | Use |
|---|---|---|---|
| Native item template | `ItemTemplate_Crafting_Cooking_Apple` | `1527b1864369efd48b49abb54f1e42e4` | Reviewed native prototype in the first runtime-visible custom-item proof |
| Custom item template | `ItemTemplate_Mod_FoodDrink_AppleGreen` | `fdaf0000000000000000000000000001` | Separate runtime custom identity used by the Green Avalon Apple proof |
| Player-facing name | `Green Avalon Apple` | presentation text, not a GUID | UI display |

### Lockpick example

| Kind | Name | Exact identity | Use |
|---|---|---|---|
| Native item template | `ItemTemplate_1_Lockpick` | `4874d14cab8060c4a981ae4440298096` | Native source prototype |
| Custom item template | `ItemTemplate_Mod_TaintedLockpick` | `10b2665ab64c75f51aaf836574aa0dd1` | Custom clone identity in source-inspected implementation |

### Merchant example

| Kind | Name | Exact identity | Use |
|---|---|---|---|
| Native shop template | Tier-1 vendor target used by item proofs | `75a071140bc819d4ab6e9e37abfdfa59` | Controlled merchant stock integration target |

## Where it exists in the lifecycle

An identity becomes meaningful only when the owning system resolves it:

- item template GUID → template provider;
- shop template GUID → shop instance/template relationship;
- plug-in GUID → BepInEx;
- asset address → asset loader.

## How we interact with it

Use the curated identity to follow the example, then learn how to discover and verify another identity rather than hard-coding the example into unrelated mods.

Record the exact source/build from which a native identity was established.

## Why this route

A small curated catalogue supports teaching while avoiding:

- giant extracted GUID dumps;
- stale database claims;
- accidental redistribution of game data;
- name-based guessing.

## What goes wrong

- copying an example GUID into unrelated content;
- confusing custom GUID with source/native GUID;
- changing a custom GUID after it becomes persistent;
- assuming the same GUID relationship across a different game build without rechecking;
- treating a display name as the machine identity.

## How to verify

Resolve the identity through its native owner and confirm the returned object/type/name matches the expected subject.

## Current proof boundary

These identities are examples used by documented paths, not a promise that every native relationship remains unchanged forever.

For the identity model itself, see [Identity: GUIDs, Names, Addresses, and Stable IDs](identity-guids-names.md).
