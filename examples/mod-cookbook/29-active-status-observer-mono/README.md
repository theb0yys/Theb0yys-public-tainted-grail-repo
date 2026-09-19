# 29 — Active Status Observer

**Category:** statuses / active-set observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example snapshots the current hero's native active-status collection:

~~~text
Hero.Statuses.AllStatuses
~~~

It polls at a bounded interval but writes logs only when the observed set changes.

On the first valid hero snapshot it reports the current count and each active status once. Later it reports only status-instance additions and removals.

The comparison uses runtime object identity deliberately. This makes the example a membership observer, not a claim that template identity uniquely describes every stack or refreshed status.

A renew, duration change or stack-field mutation that keeps the same Status instance may therefore be visible in example 28's application event without appearing as a remove/add transition here.

## Build

~~~powershell
dotnet build .\ActiveStatusObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

This example does not remove, expire, cleanse or otherwise change the status collection.

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
