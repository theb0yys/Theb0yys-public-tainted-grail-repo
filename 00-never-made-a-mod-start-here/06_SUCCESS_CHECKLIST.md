# 06 - What Success Looks Like

Use this to decide whether level 00 is actually complete.

## Runtime plug-in path

### PASSED

Your first plug-in smoke test passed when:

- BepInEx starts;
- your plug-in DLL is discovered;
- your own unique log line appears;
- no plug-in load exception is produced;
- the game remains stable through startup;
- you can change the log text, rebuild, redeploy, and observe the new text.

### NOT YET PASSED

Do not move on merely because:

- the C# project builds;
- the DLL exists;
- BepInEx exists;
- an old version of your DLL loaded once.

The complete first loop is:

~~~text
edit -> build -> deploy -> game launch -> log observation
~~~

## Content path

For a first content session, success means you can:

- open the expected toolkit/editor cleanly;
- create a mod-owned definition through the documented route;
- save it without project errors;
- explain its template/category relationship;
- distinguish logical item data from icon/equipment/world representation;
- repeat the edit/save cycle.

Do not claim game-runtime success until you actually test the relevant behaviour in the game.

## Rollback check

You should also know how to undo your test:

- remove your plug-in DLL/folder; or
- remove/revert your mod-owned authoring change.

If you cannot undo the first test confidently, improve the workflow before adding more complexity.

## Next

Continue to 01-basic/.
