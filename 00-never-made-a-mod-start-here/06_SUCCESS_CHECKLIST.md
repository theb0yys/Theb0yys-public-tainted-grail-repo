# Confirm Your First Mod Worked

## What you're doing

You are checking whether your first exercise completed an entire usable loop rather than stopping at a partial result such as "the project built."

## What you need

For the runtime path:

- your first plug-in project;
- the DLL you deployed;
- the BepInEx log from the game launch.

For the content path:

- the toolkit/editor project;
- the mod-owned definition you created;
- the editor or runtime observations you actually made.

## What you'll learn

You will learn:

- what counts as a complete first success;
- which partial results are not enough;
- how to verify that a change can be repeated;
- why rollback is part of a safe beginner workflow.

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

### 2. Check the content-authoring path

For a first content session, success means you can:

- open the expected toolkit/editor cleanly;
- create a mod-owned definition through the documented route;
- save it without project errors;
- explain its template/category relationship;
- distinguish logical item data from icon/equipment/world representation;
- repeat the edit/save cycle.

Do not claim game-runtime success until you actually test the relevant behaviour in the game.

### 3. Prove you can roll back

You should also know how to undo your test:

- remove your plug-in DLL/folder; or
- remove/revert your mod-owned authoring change.

If you cannot undo the first test confidently, improve the workflow before adding more complexity.

## What success looks like

This page is complete when your chosen path satisfies its full checklist **and** you know how to return to the previous known-good state.

## Common problems

**"It builds, so it works":** a successful compile does not prove deployment or runtime loading.

**An old DLL loaded once:** change your log message, rebuild, redeploy, and prove the new build is the one running.

**The editor saved an asset, so the game supports it:** saving successfully in the editor and working successfully in the game are two different checks.

**You cannot undo the test:** establish a clean rollback before adding more moving parts.

## Where to go next

Continue to **[Learn the Everyday Modding Loop](../01-basic/README.md)**.
