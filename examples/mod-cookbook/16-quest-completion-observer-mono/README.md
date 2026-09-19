# 16 — Observe Quest Completion Candidates

**Category:** quests / story observation  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example uses the owner-side candidate set:

- `QuestUtils.Complete` methods with two parameters;
- `QuestUtils.SetQuestState(..., QuestState.Completed)`.

It logs before the candidate call and does not mutate quest state.

## Build

```powershell
dotnet build .\QuestCompletionObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

## Limit

These are interception candidates, not a proven canonical "quest completed exactly once" event. Different quest paths may behave differently.
