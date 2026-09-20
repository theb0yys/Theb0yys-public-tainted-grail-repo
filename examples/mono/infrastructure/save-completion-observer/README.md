# Native Save Completion Observer

Patch EndSave(string) on the concrete FoA cloud-service implementations and hand the completed slot ID to Update through a queue.

~~~powershell
dotnet build .\SaveCompletionObserver.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Expected log:

~~~text
Native save completion observed. slotId=...
~~~

Guide: ../../../../guides/tasks/saving/observe-native-save-completion.md
