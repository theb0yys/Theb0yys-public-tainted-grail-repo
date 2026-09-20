# Action Receipts

A small IMGUI surface demonstrating the important receipt rule:

~~~text
user clicks
→ stable GUID captured
→ current owner state re-resolved
→ gameplay action executes
→ Done / Blocked / Failed is returned
→ UI renders that result
~~~

The demo uses the proven existing-item grant route so the receipt reflects a real gameplay action.

## Build

~~~powershell
dotnet build .\ActionReceipts.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Use a disposable save. The small window appears after the plug-in loads.

## Boundary

The example demonstrates action-result ownership, not a production modal UI/input system. The IMGUI window is deliberately minimal.

Guide: [Report mod actions with real receipts](../../../../guides/tasks/ui/report-action-results-with-receipts.md)  
Evidence: [Action receipts for mod UI](../../../../research/case-studies/ui/action-receipts.md)
