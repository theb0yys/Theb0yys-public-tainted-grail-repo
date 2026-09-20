# Build Custom Rigid Weapon Presentation Through Drake

Keep FoA's Item/equip/hand path and replace only the registered weapon presentation through the existing Drake pipeline.

Working lineage: [Evil Greatsword: Equip and Presentation Boundary](../../../research/case-studies/weapons/evil-greatsword-presentation.md).  
Canonical owners: [Native Weapon Integration](../../../knowledge/systems/gameplay/native-weapons/README.md) and [Drake](../../../knowledge/systems/presentation/drake/README.md).

## Native chain to preserve

~~~text
ItemTemplate
→ Item
→ ItemEquipSpec
→ ItemEquip
→ CharacterHandBase
→ equipped presentation reference
→ Drake prototype / resources
~~~

Do not turn a Unity MeshRenderer into a second weapon owner.

## Register the custom weapon identity first

The current Evil Greatsword consumer delegates registration to the shared Tainted Weapons registrar:

~~~text
TaintedWeaponsApi.RegisterNativeWeaponTemplate(request, out receipt)
~~~

and retries after TemplatesLoader.FinishedLoading when required.

After registration, resolve the custom ItemTemplate again through the normal TemplatesProvider path. The registered custom identity must be the one the inventory/equip system sees.

A concrete maintained identity is:

~~~text
template: ItemTemplate_Mod_EvilGreatsword
GUID: e6e91100000000000000000000000001
mesh: Evil_eye_greatsword_LowUV1
material: MI_EvilGreatsword
~~~

## Redirect only the equipped presentation reference

Capture a valid native rigid-weapon equipped presentation as the structural source.

When the registered custom ItemTemplate is equipped, redirect the presentation reference to a framework-owned address such as:

~~~text
mod://kane.tgfoa.tainted-weapons/equipped-prototype/<consumer>/<weapon-key>
~~~

The normal equip/hand code still initiates the presentation lifecycle.

## Build a Drake-compatible prototype

The working framework produces a prototype with:

~~~text
CharacterHands component(s)
Drake LOD group(s)
Drake mesh renderer(s)
0 ordinary Unity renderers
~~~

For the Evil Greatsword runtime receipt, the prototype shape was:

~~~text
characterHands=1
drakeLodGroups=1
drakeMeshRenderers=1
unityRenderers=0
~~~

That is the important distinction: the final equipped object participates in the Drake path rather than remaining a fallback Unity renderer.

## Serve mesh/material through Drake resource ownership

When Drake requests the registered mesh/material key:

- return the exact custom Mesh;
- return the exact custom Material;
- retain the associated resource handle while Drake owns it;
- release it when the framework/native lifetime releases the presentation.

Do not manually edit Drake resource counters or fabricate ECS lifecycle tags.

The known runtime path reached mesh/material loading-manager requests for the registered keys.

## Keep equip and presentation separate

The mesh/material does not own:

- inventory identity;
- damage;
- equip legality;
- hand choice;
- weapon animation;
- save identity.

Those remain on the native ItemTemplate/Item/equip chain.

## Preview uses the same principle

Inventory/equipment previews are another presentation owner.

Resolve the preview route separately and attach the registered custom presentation to that owner. Do not mutate vanilla preview cameras or vanilla weapon instances to make a custom preview visible.

## Cleanup

Track every framework-owned prototype/resource handle by the registered custom identity.

On release:

~~~text
native/Drake presentation ends
→ framework releases custom resource handles
→ cached prototype entry can be discarded when no longer owned
~~~

Do not suppress global Drake unload behavior.
