# 28 — Status Application Observer

**Category:** statuses / application observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example observes FoA's native status-application seam without changing the status.

Target:

~~~text
Awaken.TG.Main.Heroes.Statuses.CharacterStatuses.AddStatus(...)
~~~

For meaningful applications it logs:

- add type: Add, Upgrade, AddAndProlong, AddAndRenew, Replace or Stack;
- positive versus negative classification;
- whether the source character is the current hero, another character, or unavailable;
- whether an item source exists;
- whether the target is the current hero or another character.

A depth guard suppresses nested AddStatus calls so one native application chain does not become duplicate top-level rows.

This example does **not** add, remove, prolong, replace, stack or otherwise mutate statuses.

## Build

~~~powershell
dotnet build .\StatusApplicationObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

Status removal/expiry and the active set are intentionally separate concerns; continue with example 29 for membership changes.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
