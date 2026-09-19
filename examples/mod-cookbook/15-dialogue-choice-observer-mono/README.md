# 15 — Observe Dialogue Choices Being Offered

**Category:** dialogue / story observation  
**Source-path evidence:** LOAD_EVIDENCED  
**This rewritten public example:** NOT_RUN

This example patches the real `Story.OfferChoice(ChoiceConfig)` entry point and only logs that a choice set was offered.

It does not select a choice, alter story state, change text, or write saves.

## Build

```powershell
dotnet build .\DialogueChoiceObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

## Limit

This target is a concrete observation candidate, not a full dialogue event API. Cancellation, selection, re-entry and stable payload semantics need their own evidence before you build a framework on top of it.
