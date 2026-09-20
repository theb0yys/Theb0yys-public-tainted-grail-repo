# Debug the Basic Modding Loop

## What you're doing

You are isolating the earliest meaningful failure instead of changing several unrelated things at once.

## What you need

- the failing plug-in or content-authoring test;
- the last known-good state;
- the relevant BepInEx log, editor error, or other direct result;
- the exact game/runtime/toolkit versions involved.

## What you'll learn

You will learn how to:

- debug from the lowest failing layer upward;
- separate loader failure from feature failure;
- separate editor success from runtime success;
- return to a known-good state;
- ask for help with the smallest useful diagnostic information.

## Steps

### 1. If the runtime plug-in did not load

Check in this order:

1. Did BepInEx itself start?
2. Is this the correct Mono/IL2CPP lane?
3. Is your DLL in the correct `BepInEx\plugins` folder?
4. Is it the newest DLL you just built?
5. Does the log show your plug-in GUID/name?
6. What is the **first** exception/error associated with the plug-in?
7. Are required local references/dependencies present?

Fix the earliest meaningful failure first.

### 2. If the plug-in loads but the feature does not work

Check:

1. Did config disable it?
2. Did the patch install?
3. Does the target type/method still exist?
4. Are you targeting the correct overload?
5. Did a game update change the target?
6. Is another mod patching the same behaviour?

### 3. If the content editor path fails

Separate these questions:

1. Did the Unity/toolkit project open cleanly?
2. Did the authoring tool create/save the definition?
3. Are required references/addressables valid?
4. Does the editor preview/validation work?
5. Did you actually test the game runtime?

Do not describe editor success as game-runtime success.

### 4. Return to a known-good state

Keep the last known-good source/definition.

When a new change fails:

- revert that one change;
- confirm the previous state still works;
- reintroduce the change in a smaller form.

### 5. Ask for help with minimal useful information

A useful report includes:

- game build;
- runtime lane;
- BepInEx/toolkit version;
- mod version/commit;
- exact step that failed;
- first relevant error;
- what you expected;
- what actually happened.

Do not upload the whole game, proprietary content, credentials, saves, or unredacted private paths.

## What success looks like

You can identify the earliest failing layer, make one targeted correction, and either restore the known-good state or produce a small report that another person can reason about.

## Common problems

**Changing five things after one failure:** you lose the ability to tell which change mattered.

**Debugging gameplay before confirming the loader and plug-in loaded:** prove the lower layers first.

**Treating editor success as if it proves the game behaviour:** check the editor and the game separately.

**Sharing excessive diagnostics:** redact private paths and never upload proprietary game content, credentials, or saves just to ask for help.

## Where to go next

Return to the tutorial that failed and repeat only the affected step.

When your loop is stable, continue to **[Understand How Mods Work](../foundations/README.md)**.
