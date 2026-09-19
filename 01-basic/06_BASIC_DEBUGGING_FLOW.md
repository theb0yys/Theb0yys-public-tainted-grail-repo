# 06 - Basic Debugging Flow

Do not change five things because one test failed.

## Runtime plug-in did not load

Check in this order:

1. Did BepInEx itself start?
2. Is this the correct Mono/IL2CPP lane?
3. Is your DLL in the correct BepInEx\plugins folder?
4. Is it the newest DLL you just built?
5. Does the log show your plug-in GUID/name?
6. What is the **first** exception/error associated with the plug-in?
7. Are required local references/dependencies present?

Fix the earliest meaningful failure first.

## Plug-in loads but feature does not work

Now check:

1. Did config disable it?
2. Did the patch install?
3. Does the target type/method still exist?
4. Are you targeting the correct overload?
5. Did a game update change the target?
6. Is another mod patching the same behaviour?

## Content editor problem

Separate these questions:

1. Did the Unity/toolkit project open cleanly?
2. Did the authoring tool create/save the definition?
3. Are required references/addressables valid?
4. Does the editor preview/validation work?
5. Did you actually test the game runtime?

Do not describe editor success as game-runtime success.

## Known-good rollback

Keep the last known-good source/definition.

When a new change fails:

- revert that one change;
- confirm the previous state still works;
- reintroduce the change in a smaller form.

## Ask for help with minimal evidence

Useful report:

- game build;
- runtime lane;
- BepInEx/toolkit version;
- mod version/commit;
- exact step that failed;
- first relevant error;
- what you expected;
- what actually happened.

Do not upload the whole game, proprietary content, credentials, saves, or unredacted private paths.
