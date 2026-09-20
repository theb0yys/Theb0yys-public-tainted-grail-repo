# Mono Development Loop

For Mono / BepInEx 5 mods, optimize the iteration loop without treating in-process hot reload as a universal capability.

The default loop is:

~~~text
edit source
→ project check
→ build
→ install exact build artifact
→ start/restart FoA
→ watch filtered BepInEx log
→ trigger one bounded interaction
→ inspect patch ownership or invocation only when needed
→ repeat
~~~

## Why restart instead of generic hot reload?

Harmony patches, Unity objects, static state, event subscriptions and BepInEx plug-in lifecycles can outlive the assumptions of a simple DLL replacement.

Replacing a DLL on disk does not prove the running process unloaded the old assembly, removed patches, cleared static state, or unsubscribed handlers.

Use an explicit restart as the baseline unless a specific mod/tool owns and proves its reload lifecycle.

## 1. Check the project

Use the repository's project doctor before spending a runtime cycle on a structurally broken project.

    .\platform\developer-tools\Test-FoAModProject.ps1 -Project ".\MyMod\MyMod.csproj" -GameRoot "<GameRoot>"

## 2. Build

    .\platform\developer-tools\Build-FoAMod.ps1 -Project ".\MyMod\MyMod.csproj" -GameRoot "<GameRoot>"

Keep local game/loader references outside the repository.

## 3. Install the exact build artifact

    .\platform\developer-tools\Install-FoAMod.ps1 -Project ".\MyMod\MyMod.csproj" -GameRoot "<GameRoot>"

The install helper verifies the copied DLL identity. That answers which file was installed, not whether the game loaded it.

## 4. Restart the game process

Close the previous FoA process before relying on a new DLL.

A clean process boundary avoids carrying forward:

- old Harmony patch chains;
- static fields;
- cached Unity objects;
- event subscriptions;
- stale plug-in instances;
- old assembly images.

## 5. Watch the relevant log slice

    .\platform\developer-tools\Watch-FoALog.ps1 -GameRoot "<GameRoot>" -Pattern "My Mod|Harmony|Exception"

Prefer a narrow pattern tied to the plug-in GUID/name or the exact subsystem under test.

## 6. Trigger one interaction

Do not change several independent behaviors before the next observation.

For a Harmony feature, separate these questions:

~~~text
plug-in loaded?
→ target resolved?
→ patch owner present?
→ target invoked?
→ downstream feature correct?
~~~

Use the [Harmony runtime audit](../../research/tools/harmony-runtime-audit/README.md) for the live patch table and the [runtime tracer](../../research/tools/runtime-tracer/README.md) for exact invocation observation.

## 7. Keep cleanup explicit

A mod that owns subscriptions, temporary objects, patches or sidecar resources should release them in its normal BepInEx/Unity teardown path.

Do not use a development-only reload trick as evidence that cleanup is correct.

## 8. After a game update

Switch to [Validate mods after a game update](game-update-validation.md) before resuming the normal iteration loop.

Re-establish symbol identity first; do not burn runtime cycles debugging a target that no longer exists.
