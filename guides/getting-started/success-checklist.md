# Confirm Your First Mod Worked

## What you're doing

You are checking whether your first exercise completed an entire usable loop rather than stopping at a partial result such as "the project built."

## What you need

For the runtime plug-in path:

- your first plug-in project;
- the DLL you deployed;
- the BepInEx log from the game launch.

For the first new-item path:

- the Mono/BepInEx 5 plug-in implementing the controlled item proof;
- the exact native prototype GUID;
- the exact custom GUID;
- registration/stock diagnostic lines;
- the in-game merchant observation from a disposable test session.

## What you'll learn

You will learn:

- what counts as a complete first success;
- which partial results are not enough;
- how to verify that a change can be repeated;
- why rollback and proof boundaries are part of a safe workflow.

## Steps

### 1. Check the runtime plug-in path

Your first plug-in smoke test passes when:

- BepInEx starts;
- your plug-in DLL is discovered;
- your own unique log line appears;
- no plug-in load exception is produced;
- the game remains stable through startup;
- you can change the log text, rebuild, redeploy, and observe the new text.

The complete first loop is:

~~~text
edit -> build -> deploy -> game launch -> log observation
~~~

Do not move on merely because:

- the C# project builds;
- the DLL exists;
- BepInEx exists;
- an old version of your DLL loaded once.

### 2. Check the first new-item path

The bounded first-item proof passes when you can demonstrate:

~~~text
native prototype GUID resolves
→ separate custom GUID exists
→ clone is valid
→ registration succeeds
→ custom GUID resolves through TemplatesProvider
→ World creates a native Item
→ decompressed merchant stock owns the Item
→ shop UI visibly shows the separate custom item
~~~

Do not stop at "the clone exists" or "the code compiled."

Do not call a custom model/icon load an item-registration success.

Do not call the visible current-session item save-safe unless you performed the separate persistence test.

### 3. Check the proof boundary

For the first custom-item path, state explicitly:

- **proved:** bounded runtime custom identity/registration plus the controlled acquisition route you observed;
- **not automatically proved:** save/load, missing-mod behavior, uninstall/orphan handling, recipes, loot, custom visuals, weapons, armour or creatures.

A useful result includes what it **does not** establish.

### 4. Prove you can roll back

For a plug-in-only smoke test, remove the mod DLL/folder and restore your known-good setup.

For the first custom-item proof:

- use a disposable test session/save;
- avoid turning the proof into durable player state until persistence is intentionally being tested;
- remove the plug-in and confirm the test surface is gone.

If you bought/saved a custom item, do not assume removing the plug-in is a safe rollback: the saved GUID may depend on the custom registration being available again.

## What success looks like

This page is complete when your chosen path satisfies its full checklist, you can explain why every stage is required, and you know the exact boundary of what was tested.

## Common problems

**"It builds, so it works":** a successful compile does not prove deployment or runtime loading.

**An old DLL loaded once:** change your log message, rebuild, redeploy, and prove the new build is the one running.

**"The ItemTemplate clone exists, so FoA knows it":** a Unity clone is not a registered FoA definition until normal template lookup can resolve the new GUID.

**"The stock contains it, but I cannot see it":** investigate merchant/UI lifecycle timing and list capture.

**"I can see it, so it is save-safe":** presentation and persistence are separate proof lanes.

## Where to go next

Continue to **[Learn the Everyday Modding Loop](../learning-paths/everyday-modding/README.md)**.

For the reasoning behind the custom-item process, use **[Items: Proven Custom Item Integration](../../knowledge/systems/gameplay/items.md)**.
