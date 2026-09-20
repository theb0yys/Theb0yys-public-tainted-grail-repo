# Custom Rigid Weapon Presentation

This is a minimal **consumer** of the shared Tainted Weapons framework. It does not build its own socket renderer or construct Drake ECS entities directly.

The source demonstrates:

~~~text
custom weapon package identity
→ TaintedWeaponsApi.RegisterWeaponPackage(...)
→ explicit native ItemTemplate registration request
→ registered custom ItemTemplate
→ normal FoA Item/equip route
→ Tainted Weapons equipped-prototype redirect
→ framework-owned Drake mesh/material presentation
~~~

## Requirement: one small AssetBundle

Place the example DLL and a bundle named:

~~~text
community_rigid_blade.bundle
~~~

in the same plug-in directory.

The bundle must contain the rigid weapon prefab at:

~~~text
Assets/TGCommunity/RigidBlade.prefab
~~~

For the simplest v1 asset, author one rigid mesh/material presentation. Do not put inventory, combat, save, or equip logic in the prefab.

The example deliberately does **not** ship a proprietary game asset or a prebuilt weapon bundle.

## Native source profile

The registration request derives the custom weapon ItemTemplate from the known native rigid-weapon source:

~~~text
a04d79985ec011245a8383530fc72dd7
~~~

The example reserves its own custom identity:

~~~text
GUID: c0ffee00000000000000000000000001
name: ItemTemplate_Mod_CommunityRigidBlade
~~~

Change those values for a real mod.

## Build

~~~powershell
dotnet build .\RigidWeaponPresentation.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Install beside the current Tainted Weapons framework.

Expected successful startup includes a Tainted Weapons package/registrar receipt. After the registered template is available, press F8 to grant one item through the normal World.Add → HeroItems.Add path.

Equip it normally. The consumer does not parent a Unity MeshRenderer to the hand; Tainted Weapons owns the equipped Drake prototype/resource route.

## What to change first

Change only these values first:

~~~text
WeaponId
CustomTemplateGuid
CustomTemplateName
BundleFileName
EquippedPrefabAssetPath
display name / description
~~~

Keep the native source profile and framework route unchanged until your custom item can be registered and equipped consistently.

Guide: [Build custom rigid weapon presentation through Drake](../../../../guides/tasks/weapons/custom-rigid-weapon-presentation.md)
