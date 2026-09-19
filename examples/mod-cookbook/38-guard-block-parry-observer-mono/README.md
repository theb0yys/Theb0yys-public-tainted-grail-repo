# 38 — Guard / Block / Parry Observer

**Category:** combat / defence-result observation  
**Source-path evidence:** SOURCE_BUILD_EVIDENCED guard entry; RUNTIME_EVIDENCED damage-result fields  
**Validation:** NEEDS_VALIDATION  
**This rewritten public example:** NOT_RUN

This example deliberately separates two different facts.

Guard-entry lifecycle:

~~~text
BlockStart.AfterEnter(float)
~~~

Observed damage result:

~~~text
HealthElement.TakeDamage(Damage)
Damage.IsBlocked
Damage.IsParried
~~~

The first row means the hero entered the native guard-start state.

The second row is emitted only when an incoming hero-targeted damage result reports `IsBlocked` or `IsParried`.

That avoids treating "the player pressed block" or "guard animation started" as proof that an attack was successfully blocked or parried.

## Not changed

The observer does not modify:

- parry windows;
- stamina or block costs;
- damage;
- animation state;
- proficiency XP;
- weapon/item stats.

## Build

~~~powershell
dotnet build .\GuardBlockParryObserverExample.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
~~~

The public rewrite is **NOT_RUN** and **NEEDS_VALIDATION**.
