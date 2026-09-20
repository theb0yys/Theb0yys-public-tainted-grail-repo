# Save Completion Observer

Minimal observer for concrete FoA CloudService.EndSave(string) implementations.

The Harmony postfix does only one thing: enqueue the completed slot ID. The plug-in Update method drains that queue and performs the follow-up work.

## Build

~~~powershell
dotnet build .\SaveCompletionObserver.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Expected log after a normal FoA save:

~~~text
FoA save completion observed. slotId=...
~~~

Use the slot ID as a notification/correlation key; do not perform heavy file I/O inside the postfix.

Guide: [Observe native save completion](../../../../guides/tasks/saving/observe-native-save-completion.md)
