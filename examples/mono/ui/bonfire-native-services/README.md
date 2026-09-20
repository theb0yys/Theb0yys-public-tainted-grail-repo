# Bonfire Native Services

This example keeps the UI layer deliberately small and demonstrates the important owner boundary: route into the existing FireplaceUI service methods instead of rebuilding their gameplay transactions.

At an initialized bonfire:

- F6 → OpenHeroStorage()
- F7 → CookAction()
- F8 → AlchemyAction()

VFireplaceUI.OnInitialize is observed only to capture the current native FireplaceUI owner.

## Build

~~~powershell
dotnet build .\BonfireNativeServices.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

## Boundary

The production Better Bonfire Menu also proved native-styled service entries/grid. This example focuses on the reusable service-routing mechanism and does not claim a production submenu/input lifecycle.

Guide: [Add native services to the bonfire menu](../../../../guides/tasks/ui/add-native-services-to-the-bonfire-menu.md)  
Evidence: [Native bonfire services](../../../../research/case-studies/gameplay/bonfire-services.md)
