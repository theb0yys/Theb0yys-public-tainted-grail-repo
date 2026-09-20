# Build a Native-Looking Bonfire Services Submenu

Reuse the current VFireplaceUI/FireplaceUI rather than rebuilding bonfire gameplay.

Working lineage: [Native Service Reuse and Submenu Ownership](../../../research/case-studies/bonfire/native-service-reuse.md).  

## Runnable source

Start with the [Bonfire services submenu example](../../../examples/mono/ui/bonfire-services-submenu/README.md). Build it unchanged first, then change one setting or mechanism at a time.

## Runnable source

Start from the buildable example: [Bonfire native services submenu](../../../examples/mono/ui/bonfire-native-services/README.md). Build it unchanged first, confirm the documented behavior, then make one change at a time.

Native services: [Bonfire / Fireplace Native Services](../../../knowledge/systems/world/bonfire-services.md).

## Capture the active bonfire owner

Observe:

~~~text
VFireplaceUI.OnInitialize
~~~

and read its GenericTarget as FireplaceUI.

Keep that current FireplaceUI reference only while the view/model is alive.

## Reuse a native button as the visual template

The maintained Better Bonfire Menu reads VFireplaceUI members including:

- buttonContent;
- levelUp;
- ShowDescription.

A working native-looking entry is created by cloning the existing Level Up button config:

~~~csharp
GameObject clone =
    Instantiate(levelUpConfig.gameObject, parent);
~~~

Rename the clone, place it at the intended sibling index, replace its label/action/description bindings, and leave the original native button untouched.

The submenu implementation uses the same principle for each child row: clone the native button template rather than constructing an unrelated visual style.

## Native service calls

Route each custom button to the real FireplaceUI method.

Known services include:

~~~text
OpenHeroStorage()
CookAction()
AlchemyAction()
HandcraftingAction()
GoToSleepAction()
LevelUpAction()
SaveGame()
~~~

For an upgraded Wyrd-repelling fireplace, additional native services include:

~~~text
FastTravel()
RecallPet()
~~~

Do not duplicate the transaction behind those methods.

## Submenu lifecycle

The custom submenu owns only:

- cloned button GameObjects;
- labels/descriptions;
- selected/focused row;
- disabled state;
- Back/Cancel;
- cursor/input scope;
- teardown.

The native FireplaceUI still owns the service.

A good sequence is:

~~~text
VFireplaceUI initialized
→ attach one Services entry
→ Services clicked
→ hide/suspend normal row presentation as needed
→ show cloned service rows
→ focus first usable row
→ invoke real FireplaceUI action
→ close/rebuild submenu after native service returns
~~~

## Input

Do not disable the EventSystem that your buttons require.

Acquire a UI/input scope that suppresses gameplay controls while keeping UI event dispatch alive. Restore exactly the previous cursor/input state when the submenu closes.

## Teardown

On VFireplaceUI discard, scene change, feature disable, or plug-in unload:

- destroy only your cloned rows/containers;
- clear the captured owner;
- release cursor/input scope;
- restore any native presentation state you changed.

Never leave duplicated service buttons behind after reopening a bonfire.
