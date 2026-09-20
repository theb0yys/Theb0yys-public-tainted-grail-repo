# Give a Custom Weapon Its Own 3D Model

This example shows how a custom rigid weapon can use its own model while still using Tainted Grail's normal item and equipment systems.

You do **not** need to write your own hand renderer or combat system.

## Before you build

This example uses the shared Tainted Weapons framework already documented in this repository.

You also need one small Unity AssetBundle containing your own rigid weapon prefab.

Place the built mod DLL and this bundle in the same plug-in folder:

~~~text
community_rigid_blade.bundle
~~~

Inside the bundle, the example expects:

~~~text
Assets/TGCommunity/RigidBlade.prefab
~~~

For your first test, keep the prefab simple: one rigid mesh and material.

Do not put gameplay logic into the prefab.

## Build it

~~~powershell
dotnet build .\RigidWeaponPresentation.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Install the result beside the current Tainted Weapons framework.

## Try it in game

The example registers a new custom weapon identity, then lets Tainted Grail treat it as a normal item.

After startup:

1. check the BepInEx log for the registration message;
2. press F8 to grant the test weapon;
3. equip it normally through the inventory;
4. confirm the custom model appears in the equipped weapon path.

## What to change first

Start with these values only:

~~~text
WeaponId
CustomTemplateGuid
CustomTemplateName
BundleFileName
EquippedPrefabAssetPath
display name
description
~~~

Keep the native source weapon and framework plumbing unchanged until your own model appears reliably.

## How it works

There are two separate parts:

~~~text
gameplay item
→ registered custom ItemTemplate
→ normal Tainted Grail inventory/equip flow

visual model
→ Tainted Weapons framework
→ Drake-compatible equipped presentation
→ your mesh/material
~~~

Drake is part of Tainted Grail's character/equipment rendering system. The framework handles that presentation path for you.

The Unity prefab does not become the owner of inventory, damage, saving, or equip rules.

## Next

[Read the custom rigid weapon guide](../../../../guides/tasks/weapons/custom-rigid-weapon-presentation.md)
