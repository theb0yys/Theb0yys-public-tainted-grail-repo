# Validation Plan

## Required level

- Level: build plus throwaway-save runtime validation for the proof build.
- Reason: 0.1.4 adds a gated proof command panel for the active one-session ally proof actor because native quick command actions did not work in live testing. The panel is runtime-only and not saved, but it touches UI, cursor/input state, recall movement, and native ally defend behavior, so it must be tested in game before release-ready claims.

## Build validation

- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.6.0 with 0 warnings and 0 errors after adding scanner-only capture/recruitment research output and blocked dry-run `recruit-friendly` / `capture-enemy` rows.
- Local DLL file version: `0.6.0.0`.
- Local DLL SHA-256: `549A82D41DDD037E9B21D43021FD96670A86E0BA6FDCB35C11786CA3C0574EB4`.
- Live deployed DLL update: completed. Live DLL file version `0.6.0.0`, SHA-256 `549A82D41DDD037E9B21D43021FD96670A86E0BA6FDCB35C11786CA3C0574EB4`.
- In-game 0.6.0 scanner classification validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.5.3 with 0 warnings and 0 errors after adding live debug-panel controls for `HumanBrainTuning` values, `Save Config`, `Reset Defaults`, and scrollable panel content.
- Local DLL file version: `0.5.3.0`.
- Local DLL SHA-256: `1C2708832C610DF3C45CC7B0F27C7A95FD4D80909E8581D2B360C787DED00869`.
- Live deployed DLL update: completed after a first copy attempt found the previous DLL locked; retry succeeded. Live DLL file version `0.5.3.0`, SHA-256 `1C2708832C610DF3C45CC7B0F27C7A95FD4D80909E8581D2B360C787DED00869`.
- In-game 0.5.3 live tuning-control validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.5.2 with 0 warnings and 0 errors after adding `HumanBrainTuning` config entries for responsive brain tick, Follow leash distance, Follow recall grace, responsive combat recall distance, automatic recall cooldowns, and native defend prompt cooldown.
- Local DLL file version: `0.5.2.0`.
- Local DLL SHA-256: `82473D1E7A4C6EB0FB8ED9686B34657E620C5C4E70BD91997FEA344C72AB3394`.
- Live deployed DLL update: completed; live DLL file version `0.5.2.0`, SHA-256 `82473D1E7A4C6EB0FB8ED9686B34657E620C5C4E70BD91997FEA344C72AB3394`.
- In-game 0.5.2 config-driven tuning validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.5.1 with 0 warnings and 0 errors after widening the responsive Follow leash, adding a short responsive Follow recall grace window, widening the responsive combat recall boundary, and resetting leash grace state on mode/recall/brain reset.
- Local DLL file version: `0.5.1.0`.
- Local DLL SHA-256: `B4FD5D1B88A1761CB3B6E3572DB24BB775EA13637F6238C6ECCB1633FC75B623`.
- Live deployed DLL update: completed after closing the running game process; live DLL file version `0.5.1.0`, SHA-256 `B4FD5D1B88A1761CB3B6E3572DB24BB775EA13637F6238C6ECCB1633FC75B623`.
- In-game 0.5.1 follow-leash tuning validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.5.0 with 0 warnings and 0 errors after adding responsive native assist for faster Follow recovery and proactive native defend on live hero attackers.
- Local DLL file version: `0.5.0.0`.
- Local DLL SHA-256: `FDB997FC32E1BB97E1D35E9252622B254087E61B140B96E1ABE7938E00012080`.
- Live deployed DLL update: completed; live DLL file version `0.5.0.0`, SHA-256 `FDB997FC32E1BB97E1D35E9252622B254087E61B140B96E1ABE7938E00012080`.
- In-game 0.5.0 responsive native assist validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.4.0 with 0 warnings and 0 errors after adding one-session companion behavior polish for follow recall buffering/cooldowns, Hold anchor feedback, Defend feedback, and Recall/Come Close status.
- Local DLL file version: `0.4.0.0`.
- Local DLL SHA-256: `04674DA1E54922534EFEC177584B74D3DA537F3B482D4D0CF41B30E4304D90C6`.
- Live deployed DLL update: completed; live DLL file version `0.4.0.0`, SHA-256 `04674DA1E54922534EFEC177584B74D3DA537F3B482D4D0CF41B30E4304D90C6`.
- In-game 0.4.0 behavior polish validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.3.7 with 0 warnings and 0 errors after improving promoted-candidate portrait identity mapping.
- Local DLL file version: `0.3.7.0`.
- Local DLL SHA-256: `4891834C9493A255D8508A8F7723F0979DB5627311E15A6EE1396A456378A666`.
- Live deployed DLL update: completed; live DLL file version `0.3.7.0`, SHA-256 `4891834C9493A255D8508A8F7723F0979DB5627311E15A6EE1396A456378A666`.
- In-game 0.3.7 portrait mapping validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.3.6 with 0 warnings and 0 errors after adding the active companion HUD badge and framed portrait layering for panel/dialogue/HUD.
- Local DLL file version: `0.3.6.0`.
- Local DLL SHA-256: `E89999C95514035E677E23186608815FB2FAC1C4E08FC6BD6CD41A45AF7BD307`.
- Live deployed DLL update: completed; live DLL file version `0.3.6.0`, SHA-256 `E89999C95514035E677E23186608815FB2FAC1C4E08FC6BD6CD41A45AF7BD307`.
- In-game 0.3.6 HUD badge and framed portrait validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.3.5 with 0 warnings and 0 errors after adding the embedded Tainted Interface companion icon background as a layered portrait frame.
- Local DLL file version: `0.3.5.0`.
- Local DLL SHA-256: `089B62D2AD3A0CC35238006EB3EE014EEBA4E85E57409A66BDA33D35C88CFE49`.
- Live deployed DLL update: completed; live DLL file version `0.3.5.0`, SHA-256 `089B62D2AD3A0CC35238006EB3EE014EEBA4E85E57409A66BDA33D35C88CFE49`.
- In-game 0.3.5 framed portrait validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.3.4 with 0 warnings and 0 errors after adding embedded Tainted Interface character portraits to the dialogue and summoning panel.
- Local DLL file version: `0.3.4.0`.
- Local DLL SHA-256: `BF25D5758E07BA85B8555D253D43A2EEA219F456468BA955D10E48A2462DA028`.
- Live deployed DLL update: completed; live DLL file version `0.3.4.0`, SHA-256 `BF25D5758E07BA85B8555D253D43A2EEA219F456468BA955D10E48A2462DA028`.
- In-game 0.3.4 portrait validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.3.3 with 0 warnings and 0 errors after promoting 13 selector candidates and removing player-facing dev/proof wording from the dialogue and summon panel.
- Local DLL file version: `0.3.3.0`.
- Local DLL SHA-256: `BF93DCC20AE49F1C301CA68F86D5B4EB0DEBDCBCFBC1FAA5BFDA0B3B504E2820`.
- Live deployed DLL update: completed; live DLL file version `0.3.3.0`, SHA-256 `BF93DCC20AE49F1C301CA68F86D5B4EB0DEBDCBCFBC1FAA5BFDA0B3B504E2820`.
- In-game 0.3.3 dialogue and summon-panel validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.3.1 with 0 warnings and 0 errors after adding role-aware bounded recall placement to the companion brain.
- Local DLL file version: `0.3.1.0`.
- Local DLL SHA-256: `A43EB5B6BDB241DC11F52479A02223A5BFFAA1E205D112620A7101AF53F5905D`.
- Live deployed DLL update: completed; live DLL file version `0.3.1.0`, SHA-256 `A43EB5B6BDB241DC11F52479A02223A5BFFAA1E205D112620A7101AF53F5905D`.
- In-game 0.3.1 role-aware brain validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.3.0 with 0 warnings and 0 errors after adding the config-gated runtime-only companion brain.
- Local DLL file version: `0.3.0.0`.
- Local DLL SHA-256: `EFFE5613338BD21E59B9255C5911C7F01117C861C5DFAF29FFBC4872EF4CF8B5`.
- Live deployed DLL update: completed; live DLL file version `0.3.0.0`, SHA-256 `EFFE5613338BD21E59B9255C5911C7F01117C861C5DFAF29FFBC4872EF4CF8B5`.
- In-game 0.3.0 companion brain validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.2.4 with 0 warnings and 0 errors after cleaning companion command feedback and keeping proof diagnostics off the NPC dialogue.
- Local DLL file version: `0.2.4.0`.
- Local DLL SHA-256: `3580BB92D35EF32FFA73EDF5F11B7B759672B487503A5CC1A1D433DA004F21CF`.
- Live deployed DLL update: completed; live DLL file version `0.2.4.0`, SHA-256 `3580BB92D35EF32FFA73EDF5F11B7B759672B487503A5CC1A1D433DA004F21CF`.
- In-game 0.2.4 command-feedback validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.2.0 with 0 warnings and 0 errors after moving debug proof actions out of the NPC dialogue and replacing player-facing proof wording with companion-facing text.
- Local DLL file version: `0.2.0.0`.
- Local DLL SHA-256: `B3442043D4AF0120AD8B68B4A96B4F7B0622AA9CEB5F8663A9362F0F9F4E2494`.
- Live deployed DLL update: completed; live DLL file version `0.2.0.0`, SHA-256 `B3442043D4AF0120AD8B68B4A96B4F7B0622AA9CEB5F8663A9362F0F9F4E2494`.
- In-game 0.2.0 dialogue validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.21 with 0 warnings and 0 errors after making Follow reselectable and clamping Follow-mode bounded catch-up recall to the closer proof range.
- Local DLL file version: `0.1.21.0`.
- Local DLL SHA-256: `411E6EEBB39BFCF6DDE978016E36CEDAF7E1E711BD674E71C29A63DADD254885`.
- Live deployed DLL update: completed on 2026-06-24 after the game process was closed; live DLL file version `0.1.21.0`, SHA-256 `411E6EEBB39BFCF6DDE978016E36CEDAF7E1E711BD674E71C29A63DADD254885`.
- In-game Follow validation: log-confirmed on 2026-06-24. `LogOutput.log` confirmed `Avalon Human Companions 0.1.21 loaded`, native `Companion` prompt attachment, dialogue open/click handling, and repeated `follow catch-up` recall logs. `human-proof-lifecycle.csv` contained 44 `recall,follow catch-up` rows with tracked proof actor state and mode `Follow`. Independent visual confirmation of the actor staying near the hero is still required before release-ready behavior claims.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.20 with 0 warnings and 0 errors after adding the missing Avalon Companions GUI-pass dialogue maintenance path to the human dialogue surface.
- Local DLL file version: `0.1.20.0`.
- Local DLL SHA-256: `BC243B7C98B27D0D47BF777A6E597906973F2E4201165CB3005460A2F2DFAEC0`.
- Live deployed DLL update: completed; live DLL file version `0.1.20.0`, SHA-256 `BC243B7C98B27D0D47BF777A6E597906973F2E4201165CB3005460A2F2DFAEC0`.
- In-game dialogue input/cursor validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.19 with 0 warnings and 0 errors after changing the native `Companion` dialogue input/cursor path to mirror the proven Avalon Companions controller lifecycle.
- Local DLL file version: `0.1.19.0`.
- Local DLL SHA-256: `7761052AB78112AA14F9F2582AADA87A2EA4D2F2B69CF5A847A3101DBC04DCB3`.
- Live deployed DLL update: not run in this build pass.
- In-game dialogue input/cursor validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.18 with 0 warnings and 0 errors after correcting the native `Companion` dialogue input/cursor path to prefer FoA Mod Manager custom UI scope and allow dialogue Rewired reads.
- Local DLL file version: `0.1.18.0`.
- Local DLL SHA-256: `7AB149870F4B433FB021F16B0D6A8D39C13A88E2B8A3CF58A1722C2B94969950`.
- Live deployed DLL update: not run in this build pass.
- In-game dialogue input/cursor validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.17 with 0 warnings and 0 errors after making the proof roster panel responsive on large viewports.
- Local DLL file version: `0.1.17.0`.
- Local DLL SHA-256: `E89BB7775ECFA2918F0D82F60006707CF58920E2CB3B5AE22D306BBA5EE0E506`.
- In-game proof roster panel scale validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.16 with 0 warnings and 0 errors after registering the human proof-candidate roster and panel Prev / Spawn / Next controls.
- Local DLL file version: `0.1.16.0`.
- Local DLL SHA-256: `8847A01F782F29337B54376B4FAEE7DD40D8C7127A75EA62873B60D2E22F21E6`.
- In-game proof-candidate roster validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.15 with 0 warnings and 0 errors after correcting the surface split between native NPC dialogue and the hotkey debug panel.
- Local DLL file version: `0.1.15.0`.
- Local DLL SHA-256: `3CE5A60E4EA2BD34C6A7F0DFA57A28E40CECC3CAE20C6427CCD768EABDF7331F`.
- In-game corrected surface validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.14 with 0 warnings and 0 errors after replacing the visible centered IMGUI proof panel with a companion-style Unity UI command surface.
- Local DLL file version: `0.1.14.0`.
- Local DLL SHA-256: `1B90E4D525905DE4597FB19BAAC96E77457C5937252A7D314F8BAE1959CE0155`.
- In-game companion-style command surface validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.13 with 0 warnings and 0 errors after making `HumanCommands.EnableNativeCommandActions` and `HumanCommandPanel.EnableProofCommandPanel` default `true` for newly generated configs.
- Local DLL file version: `0.1.13.0`.
- Local DLL SHA-256: `B7FA193A4425D024ACF0E05D00FB829A2CD1A0AA89A59FB89D775C5E9B245AAC`.
- In-game native prompt validation with the new defaults: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.12 with 0 warnings and 0 errors after adding the runtime-only native `Companion` prompt bridge to the existing proof panel.
- Local DLL file version: `0.1.12.0`.
- In-game native prompt validation: not run.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.11 with 0 warnings and 0 errors after adding optional Tainted Interface shared style/scope integration.
- Local DLL file version: `0.1.11.0`.
- Local DLL SHA-256: `12DBA3890230B8A226141713849230C04FF90DDC588486D54E15EADE94F3AECF`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.11 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `12DBA3890230B8A226141713849230C04FF90DDC588486D54E15EADE94F3AECF`; file version was `0.1.11.0`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed on 2026-06-15 with 0 warnings and 0 errors.
- Earlier scaffold-only deployed DLL hash matched local Release build output SHA-256 `C815D7D178877D4F7B665DD5D7A48BED4D356AD81DA76C044CDBEABAAA7039E1`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed after adding the safe proof spawn command with 0 warnings and 0 errors.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed after adding the safe proof spawn command with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `F9037631E0E8BCC0C974525A21088A6B314B8075A62483E60A908EFB76DAE8C5`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed after adding the disabled actor scanner with 0 warnings and 0 errors.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.1 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `B10341DD4AEC6DC607AB2129FBBEEB835D99E8B4B787FCD38B428E12625F6FBE`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.2 with 0 warnings and 0 errors.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.2 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `4129694CFC93F13CF478BF56164FE1B42EF92A35F068DFE4301CFAC12C8BAFD0`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.3 with 0 warnings and 0 errors after adding native quick commands and removing unsupported Stay/Wait behavior.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.3 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `1B6F13B0612827B7C1FE9A9B0B9BBDAD70B242554CA7FFC123D8E4AE672BD82C`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: initial 0.1.4 build failed because Unity IMGUI/text module references were missing. After adding references, passed with 0 warnings and 0 errors.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.4 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `BCE887854A46EE4A4CB117C1A2A9F0D66E135D1E94B0FF0BBDC61C54C8F544D7`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.5 with 0 warnings and 0 errors after proof panel layout polish.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.5 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `0EE75B0D45834BABEC4EBCF6C1A83A969115DA3472EEEFEED552E1D5214612A5`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.6 with 0 warnings and 0 errors after adding the gated proof-panel scanner trigger.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.6 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `DAB81B687CA3FFABD3092EF7A96483FB0C142EDE5DD6095282A84B448DB8BE33`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.7 with 0 warnings and 0 errors after adding lifecycle diagnostics/guardrails.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.7 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `D554D9993BB3236C871D9A488B2AA34D9A61FFED7F0547C8AD850A728B42B1A2`; file version was `0.1.7.0`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.8 with 0 warnings and 0 errors after adding the proof-only Hold runtime movement block.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.8 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `082B296FC05AE51E75E7B1295C039C2736F4A6FF86E01E8D3967C50FBC82B5C8`; file version was `0.1.8.0`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.9 with 0 warnings and 0 errors after panel flow/input lock and diagnostic-map generator changes.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.9 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `9BB774BA4BE8966D78F444C10B73061EA9CB5C494099BB5B5759B41D917076FC`; file version was `0.1.9.0`.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Result: passed for version 0.1.10 with 0 warnings and 0 errors after proof panel size/button-state polish.
- Command: `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Result: passed for version 0.1.10 with 0 warnings and 0 errors.
- Deployed DLL hash matched local Release build output SHA-256 `38B81B350893816C3436FAC512DDB1D9F4E14D1439F54ADCBCEC0DEE586AA81D`; file version was `0.1.10.0`.

## Research validation

- Command: `ilspycmd -l c "A:\SteamLibrary\steamapps\common\Tainted Grail FoA\Fall of Avalon_Data\Managed\TG.Main.dll"` filtered for NPC, summon, ally, faction, interaction, presence, template, and story-step terms.
- Command: `ilspycmd -t ...` for selected actor, summon/ally, presence, and story-step classes.
- Result: passed. Evidence recorded in `docs/research/human-npc-proof-step-2026-06-15.md`.

## Load validation

- Game version: not verified.
- Branch: Mono expected.
- BepInEx version: v5 expected.
- Expected for current build: BepInEx logs `Avalon Human Companions 0.1.19 loaded` after deploy and restart/apply.
- Last confirmed: `LogOutput.log` confirmed `Avalon Human Companions 0.1.7 loaded` on 2026-06-16 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`. The final deployed 0.1.7 DLL hash was `D554D9993BB3236C871D9A488B2AA34D9A61FFED7F0547C8AD850A728B42B1A2`.
- Current confirmed: `LogOutput.log` confirmed `Avalon Human Companions 0.1.8 loaded` on 2026-06-16 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- Current confirmed: `LogOutput.log` confirmed `Avalon Human Companions 0.1.9 loaded` on 2026-06-16 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- Input-lock confirmation: `LogOutput.log` confirmed the 0.1.9 panel input lock patched `GameUI.UpdateMousePosition`, `PlayerInput.ProcessLateUpdate`, 8 Rewired axis overloads, and 34 Rewired button overloads.

## Feature validation

- Scenario: generated config after first launch.
- Expected: `Research.ResearchModeOnly=true`, `Prototype.EnableDisabledRecruitmentTest=false`, and `Prototype.DisabledRecruitmentTestHotkey=None`.
- Actual: live config was backed up and set for proof testing after generation. Default generation itself has not been rerun on a clean config.
- Scenario: enable the disabled recruitment-test hotkey.
- Expected: pressing the configured key only logs a blocked-behavior warning and never touches NPCs.
- Actual: not run.
- Live config state after 0.1.3 deploy: backed up to `kane.tgfoa.avalon-human-companions.cfg.codex-backup-20260615-humancommands` and set to `ResearchModeOnly=false`, `EnableOneSessionAllyProof=true`, `AllyProofHotkey=PageUp`, `DismissAllyProofHotkey=PageDown`, `HumanCommands.EnableNativeCommandActions=true`, `EnableFollowCatchUp=true`, `FollowCatchUpDistance=18`, `EnableNativeDefendAssist=false`, `ProofSpawn.EnableSafeSpawnCommand=false`, and `SafeSpawnHotkey=None`.
- Live config state after 0.1.4 deploy: backed up to `kane.tgfoa.avalon-human-companions.cfg.codex-backup-20260615-humanpanel`, then section-corrected with backup `kane.tgfoa.avalon-human-companions.cfg.codex-backup-20260615-humanpanel-sectionfix`. It is set to `ResearchModeOnly=false`, `EnableOneSessionAllyProof=true`, `AllyProofHotkey=PageUp`, `DismissAllyProofHotkey=PageDown`, `HumanCommands.EnableNativeCommandActions=false`, `EnableFollowCatchUp=true`, `FollowCatchUpDistance=18`, `EnableNativeDefendAssist=false`, `[HumanCommandPanel] EnableProofCommandPanel=true`, `[HumanCommandPanel] TogglePanelHotkey=End`, `ProofSpawn.EnableSafeSpawnCommand=false`, and `SafeSpawnHotkey=None`.
- Scenario: safe proof spawn command on a throwaway save.
- Test target: `Spec_Enemy_Generic_Tier1_Outlaw_1H` / `2bd34a05d1e1fb94f9770b9ee7f23be2`.
- Expected: with the reviewed non-unique `Target.TemplateGuid`, `ProofSpawn.EnableSafeSpawnCommand=true`, and `ProofSpawn.SafeSpawnHotkey` set, pressing the key spawns one NPC location behind the hero, marks it not saved immediately, logs the template GUID and safety statement, and applies no faction, ally, follow, dialogue, story, crime, or persistence behavior.
- Actual: user smoke-tested in game on 2026-06-15 and reported it worked, with screenshot evidence showing the spawned human outlaw. Codex has not rechecked the BepInEx log after that successful run.
- Follow-up: user reported the proof-spawned outlaw still attacks. This matches the research expectation because the target is a hostile enemy template and no behavior conversion is implemented. Live config was changed afterward to disable further proof spawns.
- Scenario: disabled actor scanner.
- Expected: with `ActorScanner.EnableDisabledActorScanner=true` and a scanner hotkey set, pressing the key writes `human-actor-candidates.csv`, `human-actor-components.csv`, `human-actor-command-dry-run.csv`, and `human-recruitment-capture-research.csv` under `BepInEx/config/kane.tgfoa.avalon-human-companions`. All command dry-run rows and recruitment/capture research rows stay blocked with `liveAction=false`.
- Actual: live config was set to `ActorScanner.EnableDisabledActorScanner=true`, `ActorScanner.ScannerHotkey=F6`, `ActorScanner.ScanRadius=35`, and `ActorScanner.SelectedTarget=2bd34a05d1e1fb94f9770b9ee7f23be2`. Runtime CSV validation not run yet.
- Follow-up: no scanner CSV output was present during the attack report check.
- Scenario: proof-panel scanner trigger.
- Expected: with the proof panel open and `ActorScanner.EnableDisabledActorScanner=true`, pressing `Scan Actors` calls the existing scanner path and writes `human-actor-candidates.csv`, `human-actor-components.csv`, `human-actor-command-dry-run.csv`, and `human-recruitment-capture-research.csv` under `BepInEx/config/kane.tgfoa.avalon-human-companions`. It does not spawn, move, command, dismiss, recruit, capture, convert, faction-edit, target-edit, story-edit, crime-edit, interaction-edit, or persist actors.
- Actual: partially passed. `LogOutput.log` confirmed 0.1.6 loaded, the proof panel opened, and the scanner wrote `9` candidate rows, `28` component rows, and `6` dry-run command rows to the expected plugin config folder. The CSV files exist and all dry-run command rows stayed `blocked=true` and `liveAction=false`. The captured rows did not include the reviewed outlaw target GUID because the log shows the scan ran before the one-session proof actor was created. The next scanner validation must spawn the proof actor first, then press `Scan Actors`.
- Follow-up: user provided screenshot evidence at `C:/Users/kane0/Pictures/Screenshots/Screenshot (3640).png` showing live combat against `Corpse Eater`. The latest scanner CSVs were written at 2026-06-15 18:53:40 with `4` candidate rows, `14` component rows, and `6` dry-run command rows. Safety stayed intact (`blocked=true`, `liveAction=false`, `BAD_SAFETY_ROWS=0`), but the reviewed outlaw target GUID still had `0` candidate rows. The dry-run candidate was `Spec_EnemyZombie_T1_Classic[1d110a8ec95ab1745a364562ec311e50]`, matching the current enemy context rather than the proof actor.
- Diagnostic-tool follow-up: the dev diagnostic tool path is `mods/template-diagnostics`; `mods/Tainted-Diagnostic Tool` is the release/public package folder. The newest live FOA-Diagnostic Tool dump checked after this gate was `20260615-191733` from version `0.4.7`, but it was captured on the title screen with `heroAvailable=false`, `sceneServiceActiveScene=TitleScreen`, and `World.All NpcHeroPetAlly=0`. It confirms the reviewed template GUID exists in loaded templates, but it is not proof-actor evidence. The next valid diagnostic capture must be taken in game after the proof actor is spawned and visible.
- Scenario: one-session native ally proof.
- Test target: `Spec_Enemy_Generic_Tier1_Outlaw_1H` / `2bd34a05d1e1fb94f9770b9ee7f23be2`.
- Expected: on a fresh throwaway save with `Research.ResearchModeOnly=false`, `HumanAllyProof.EnableOneSessionAllyProof=true`, and a configured ally proof hotkey, pressing the hotkey spawns one non-unique proof actor, marks it not saved, applies `OverrideFaction(hero summon faction, Summon)` plus `NpcHeroPetAlly`, logs the boundary, and does not apply recruitment, dialogue, story, crime, quest, custom target selection, panel, or persistence behavior.
- Actual: user smoke-tested in game on 2026-06-15 and reported "looks perfect", with screenshot evidence at `C:/Users/kane0/Pictures/Screenshots/Screenshot (3616).png` showing the spawned human proof actor standing close to the player without visible combat UI. Codex checked `LogOutput.log` afterward and confirmed 0.1.4 load, `native ally setup applied`, and `one-session ally proof created` lines for the reviewed target.
- Scenario: native quick command actions.
- Test target: active one-session human ally proof actor only.
- Expected: with `HumanCommands.EnableNativeCommandActions=true`, the active proof actor exposes Follow, Hold, Come Close, Recall, Defend, and Dismiss as runtime-only native interaction actions. Hold applies the 0.1.8 proof-only movement lock. Come Close and Recall reposition the proof actor near the hero. Follow allows bounded catch-up recall. Defend calls `NpcHeroPetAlly.EnterCombat()` only when the hero already has live attackers. Dismiss discards the proof actor and removes the runtime actions. True Stay/Wait and saved hold positions remain blocked.
- Actual: user reported this path was not working.
- Scenario: proof command panel.
- Test target: active one-session human ally proof actor only.
- Expected: with `HumanCommandPanel.EnableProofCommandPanel=true`, pressing `End` opens a styled proof panel. `Esc`, `End`, and Close close it. Spawn Proof creates the one-session ally proof through the existing guarded path. Follow, Hold, Come Close, Recall, Defend, and Dismiss act only on the active proof actor. True Stay/Wait and saved hold positions remain blocked. Cursor state restores after close.
- Actual: user smoke-tested in game on 2026-06-15 and reported "commands work perfectly". Screenshot evidence:
  - `C:/Users/kane0/Pictures/Screenshots/Screenshot (3623).png` shows the proof command panel open in game.
  - `C:/Users/kane0/Pictures/Screenshots/Screenshot (3627).png` shows the human proof ally participating near hostile NPCs after command testing.
- Log result: `LogOutput.log` confirmed 0.1.4 loaded and recorded proof command panel open/close events. Per-command action log confirmation was not captured; command behavior is currently validated by user smoke test and screenshot evidence.
- UI result: 0.1.4 was not release-ready because the panel was visibly too small/crowded. Version 0.1.5 builds and deploys a larger responsive two-column modal with bigger text/buttons and header dragging. Version 0.1.6 adds the gated scanner evidence button. The updated layout still needs in-game screenshot validation before release-ready UI claims.
- Scenario: proof-only Hold command.
- Test target: active one-session human ally proof actor only.
- Expected: with the proof panel enabled, pressing `Hold` applies a runtime-only movement block to the active spawned proof actor. The actor should stop follow/catch-up movement while Hold is active. `Recall` should reposition the actor without changing Hold mode. `Follow`, `Come Close`, `Defend`, and `Dismiss` should remove the hold block. No existing NPC, unique actor, save data, dialogue, quest, crime, global faction, or vanilla interaction list should be touched.
- Actual: build and deploy passed for 0.1.8. In-game Hold validation has not been run yet.
- Scenario: proof panel focus/input lock.
- Expected: with the proof panel open, cursor unlocks and game camera, movement, and actions do not pass through the panel. `End`, `Esc`, and Close close the panel and restore cursor/input state. The implementation must not change `Time.timeScale`.
- Actual: user smoke-tested in game on 2026-06-16 and reported the test passed perfectly. `LogOutput.log` confirmed 0.1.9 load, panel input-lock patch application, proof panel open/close events, and Hold/Follow command events. Codex did not personally observe the camera/focus behavior on screen during this run, so the runtime pass is user-reported plus log-confirmed.
- Scenario: proof panel size/button-state polish.
- Expected: with the proof panel open, the panel is large enough at the user's in-game resolution to show the proof flow bottom section and footer without clipping, and button colors do not get stuck in strange clicked/focused states after actions.
- Actual: build and deploy passed for 0.1.10. User-provided screenshot evidence for 0.1.9 showed the panel was still too small and vertically clipped, with odd action color state. Version 0.1.10 expands the panel and pins IMGUI button states. In-game 0.1.10 visual validation has not been run yet.
- Scenario: proof panel with Tainted Interface installed.
- Expected: with `HumanCommandPanel.EnableProofCommandPanel=true`, pressing `End` opens the existing proof panel using Tainted Interface shared dark-fantasy styling and shared custom UI scope. BepInEx logs `Avalon Human Companions using Tainted Interface shared styles and scope bridge`, cursor remains visible/unlocked while open, and `End`, `Esc`, or Close releases the scope.
- Actual: build and deploy passed for 0.1.11. In-game 0.1.11 visual/scope validation has not been run yet.
- Scenario: proof panel without Tainted Interface installed.
- Expected: the panel keeps the local IMGUI skin and direct FoA Mod Manager scope fallback.
- Actual: not run for 0.1.11.
- Scenario: native `Companion` prompt default gates.
- Expected: newly generated configs default `HumanCommands.EnableNativeCommandActions=true` and `HumanCommandPanel.EnableProofCommandPanel=true`, so the active proof actor can expose one native `Companion` prompt that opens the proof panel. Existing BepInEx configs are not overwritten by the default-value change.
- Actual: build passed for 0.1.13. Clean config generation and in-game native prompt validation have not been run.
- Scenario: companion-style proof command surface.
- Expected: with the proof command surface open from `End` or the native `Companion` prompt, the old centered IMGUI panel is gone; choices appear on the right side; the proof status text appears in a bottom dialogue band; Follow, Hold, Goodbye, and Dismiss route through the existing proof backend; close/Esc restores input.
- Actual: build passed for 0.1.14. In-game visual/native-prompt validation has not been run.
- Scenario: corrected native dialogue versus debug panel split.
- Expected: native NPC `Companion` prompt opens the right-side dialogue choices and bottom dialogue band. `HumanCommandPanel.TogglePanelHotkey` opens a compact Avalon Companions Debug-style IMGUI control panel.
- Actual: build passed for 0.1.15. In-game validation has not been run.
- Scenario: native `Companion` dialogue input/cursor with FoA Mod Manager scope.
- Expected: native NPC `Companion` prompt opens the right-side dialogue choices and bottom dialogue band, cursor remains usable, right-side choices accept pointer/controller navigation, gameplay movement/camera remain frozen, and close/Esc/Goodbye restores input.
- Actual: build passed for 0.1.18. In-game validation has not been run.
- Scenario: native `Companion` dialogue input/cursor using the proven Avalon Companions controller path.
- Expected: native NPC `Companion` prompt opens the right-side dialogue choices and bottom dialogue band, cursor remains usable through the shared cursor update path, right-side choices accept pointer/controller navigation, gameplay movement/camera remain frozen, and close/Esc/Goodbye restores input.
- Actual: build passed for 0.1.19. In-game validation has not been run.
- Scenario: dev diagnostic human NPC map.
- Expected: use dev FOA-Diagnostic Tool evidence from `mods/template-diagnostics`; generate repo-local review-only map files; all generated rows keep `safeSpawnCandidate=false`, `rosterApproved=false`, `behaviorApproved=false`, and `persistenceApproved=false`.
- Actual: passed. `tools/New-HumanNpcDiagnosticMap.ps1` generated `docs/generated/human-npc-diagnostic-map-20260616-090615.csv` and `.md` from dev dump `20260616-090615`. The map contains 544 rows: 42 `ReviewQueueNonUniqueSpawnerBacked`, 102 `NeedsSpawnerEvidence`, and 400 `BlockedUniqueOrStory`. Safety approval columns are false for every row.

## Not run

- Disabled human NPC proof scanner runtime CSV validation.
- Per-command BepInEx log confirmation for the proof panel command test.
- Dedicated proof panel UI polish validation after the 0.1.6 resizing/layout/scanner-button changes.
- Proof actor lifecycle/no-persistence gate is not passed yet; the 2026-06-16 evidence check did not include lifecycle CSV rows or fresh Avalon Human Companions scanner output after the 0.1.7 load.
- In-game proof-only Hold validation for 0.1.8.
- Independent screenshot/video capture of the 0.1.9 proof panel focus/input test.
- In-game 0.1.10 proof panel size/button-state visual validation.
- In-game 0.1.11 Tainted Interface proof-panel style/scope validation and no-layer fallback validation.
- Clean config generation for 0.1.13 default gate values.
- In-game 0.1.14 companion-style native `Companion` prompt validation: spawn proof actor, interact with it, confirm the prompt opens the companion-style surface, and confirm old quick prompts do not appear while the panel gate is enabled.
- In-game 0.1.14 visual validation: confirm right-side choices, bottom dialogue band, no centered IMGUI panel, and input restoration after `Esc`/Goodbye.
- In-game 0.1.15 split validation: confirm native NPC prompt opens dialogue, `End` opens the debug panel, and each surface closes/restores input independently.
- In-game 0.1.18 dialogue input/cursor validation with FoA Mod Manager installed.
- In-game 0.1.19 dialogue input/cursor validation using the proven Avalon Companions controller path.

## Next hard gate: proof actor live capture

This gate must pass before implementing more human companion behavior.

Required test order:

1. Load a throwaway playable save, not the title screen.
2. Confirm `FOA-Diagnostic Tool` is the dev build from `mods/template-diagnostics` and the manual dump hotkey is `F3`.
3. Spawn the one-session proof actor through the Avalon Human Companions proof panel or `PageUp`.
4. Confirm the proof actor is physically visible in world and still active.
5. Press `F3` to write a dev FOA-Diagnostic Tool dump.
6. Open the proof panel and press `Scan Actors`.

Pass criteria:

- The newest FOA-Diagnostic Tool dump has `heroAvailable=true` and a playable scene, not `TitleScreen`.
- `runtime_object_counts.csv` shows at least one `NpcHeroPetAlly` row.
- The Avalon Human Companions scanner output includes `2bd34a05d1e1fb94f9770b9ee7f23be2` in `human-actor-candidates.csv`.
- `human-actor-command-dry-run.csv` keeps every row blocked and `liveAction=false`.
- No row or log output claims recruitment, persistence, existing-NPC conversion, quest ownership, dialogue, save safety, or release-ready UI.

Fail conditions:

- The dump is captured on the title screen or with `heroAvailable=false`.
- The proof actor is not visible before `F3` or `Scan Actors`.
- The scanner captures only enemies, world objects, chests, wolves, or zombies instead of the proof actor target GUID.
- Any command dry-run row has `blocked=false` or `liveAction=true`.

Result on 2026-06-15:

- Passed with a logging caveat. Dev FOA-Diagnostic Tool dump `20260615-193847` reported `FOA-Diagnostic Tool 0.4.8`, `applicationVersion=1.23.401`, scene `CampaignMap_HOS` / `Horns of the South`, `heroAvailable=true`, hero coordinates `-1678.583|56.446|-3711.768`, and `heroInCombat=false`.
- `runtime_object_counts.csv` in dump `20260615-193847` reported `NpcHeroPetAlly=1` and `NpcHeroSummon=1`.
- Avalon Human Companions scanner files written at 2026-06-15 19:38:39 reported `5` candidate rows, `39` component rows, and `6` dry-run command rows.
- `human-actor-candidates.csv` included one reviewed proof target row for `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`, location `RuntimeLocation:Spec_Enemy_Generic_Tier1_Outlaw_1H:598`, coordinates `-1681.411|56.399|-3705.65`, distance `6.741`, `markedNotSaved=true`, `npcIsUnique=false`, `hasNpcElement=true`, `hasAlive=true`, `hasHeroPetAlly=true`, `behaviorApproved=false`, and `panelApproved=false`.
- `human-actor-components.csv` included `25` component rows for the reviewed target, including `NpcElement`, `NpcHeroPetAlly`, and `RepetitiveNpcAttachment`.
- `human-actor-command-dry-run.csv` included `6` target dry-run rows and `BAD_SAFETY_ROWS=0`; every row stayed `blocked=true` and `liveAction=false`.
- Caveat: the current `LogOutput.log` excerpt did not include a manual `F3` trigger line for dump `20260615-193847`. The validation relies on the dump content and matching scanner CSV timestamp. A later dump, `20260615-194153`, had `NpcHeroPetAlly=0` and is not the proof-actor gate capture.

## Next hard gate: lifecycle and no persistence

This gate must pass before adding more human companion behavior or making stronger command claims.

Required test order:

1. Load a throwaway playable save.
2. Spawn the one-session proof actor.
3. Confirm `Scan Actors` captures the proof actor target GUID.
4. Use panel `Dismiss`.
5. Run `Scan Actors` again and confirm the proof actor target GUID is absent.
6. Reload a fresh throwaway save, spawn the proof actor again, then cross a scene/area transition or use a long-move/fast-travel route if available.
7. Press F3 and run `Scan Actors`; confirm no duplicate proof actor exists.
8. Save the throwaway save, quit to menu or desktop, reload it, press F3, and run `Scan Actors`.
9. Confirm the proof actor does not persist as an unmanaged ally after reload.

Pass criteria:

- Dismiss removes the active proof actor from scanner output.
- Transition/long-move testing does not create duplicate proof target rows.
- Save/quit/reload does not restore the proof actor unless explicitly spawned again.
- Any visible proof actor row remains `markedNotSaved=true`, `hasHeroPetAlly=true`, `behaviorApproved=false`, and `panelApproved=false`.
- Dry-run command rows remain `blocked=true` and `liveAction=false`.
- `human-proof-lifecycle.csv` rows keep `behaviorApproved=false` and `persistenceApproved=false`.

Fail conditions:

- The proof actor remains visible or scanner-visible after Dismiss.
- More than one proof target row appears during transition or long-move testing.
- A reloaded save restores the proof actor without an explicit new spawn.
- Any unrelated NPC gains proof actor markers or proof commands.
- Any command dry-run row changes to `blocked=false` or `liveAction=true`.

Result on 2026-06-16:

- Not passed; evidence was incomplete.
- `LogOutput.log` confirmed `Avalon Human Companions 0.1.7 loaded` with the reviewed target GUID and `ResearchModeOnly=False`.
- Live config had `HumanLifecycle.EnableLifecycleSafetyGuard=true`, `HumanLifecycle.WriteLifecycleDiagnostics=true`, `ActorScanner.EnableDisabledActorScanner=true`, `HumanCommandPanel.EnableProofCommandPanel=true`, and target GUID `2bd34a05d1e1fb94f9770b9ee7f23be2`.
- `human-proof-lifecycle.csv` was missing, so no lifecycle event rows proved spawn, guard tick, Dismiss, discard, transition, or reload behavior.
- Latest Avalon Human Companions scanner files were still from `2026-06-16 06:28:55`, before the 0.1.7 build/deploy confirmation. They had `4` candidate rows, `0` rows for the reviewed target GUID, and `6` dry-run rows with `BAD_SAFETY_ROWS=0`.
- Dev FOA-Diagnostic Tool dumps from `20260616-065331` through `20260616-070419` were playable-scene dumps with `heroAvailable=true`. Most reported `NpcHeroPetAlly=0`; the latest dump `20260616-070419` reported `NpcHeroPetAlly=1` and `NpcHeroSummon=1`, but the reviewed target GUID appeared only in template/spawner reference files and there was no matching lifecycle CSV, scanner target row, or Avalon Human Companions spawn/dismiss log line.
- The next valid run must spawn the proof actor through Avalon Human Companions, press `Scan Actors` after spawn, Dismiss, press `Scan Actors` after Dismiss, then capture transition and save/quit/reload evidence with fresh lifecycle CSV and dev diagnostic dumps.

Follow-up result on 2026-06-16 at 07:22:

- Partial progress; still not a full gate pass.
- `human-proof-lifecycle.csv` was written at `2026-06-16 07:22:27` with `13` rows: `ally-proof-created=1`, `guard-tick=3`, `recall=5`, `mode-set=2`, `ally-proof-dismiss-requested=1`, and `ally-proof-discarded=1`.
- Every lifecycle row kept `behaviorApproved=false` and `persistenceApproved=false`.
- Every active lifecycle row for `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]` had `markedNotSaved=true`, `hasNpcElement=true`, `npcIsUnique=false`, and `hasHeroPetAlly=true`.
- The discard row for location `RuntimeLocation:Spec_Enemy_Generic_Tier1_Outlaw_1H:646` had `tracked=false`, `locationDiscarded=true`, and `hasHeroPetAlly=false`.
- `LogOutput.log` confirmed the proof panel opened, the proof actor spawned at `2026-06-16 07:22:16`, recall/mode commands ran, and the actor was discarded by panel Dismiss.
- Dev FOA-Diagnostic Tool manual F3 dump `20260616-072231` was a playable-scene dump with `heroAvailable=true`, `NpcHeroSummon=0`, and `NpcHeroPetAlly=0`, which supports no pet-ally/summon leftover after Dismiss.
- The Avalon Human Companions scanner output at `2026-06-16 07:22:14` had `11` candidate rows, `45` component rows, `6` dry-run rows, and `BAD_SAFETY_ROWS=0`, but it ran before the proof actor spawn log at `07:22:16`. Therefore it had `0` rows for the reviewed target GUID and cannot prove scanner capture after spawn or scanner absence after Dismiss.
- Still required: `Scan Actors` after the proof actor is visible, `Scan Actors` after Dismiss, transition/long-move duplicate check, and save/quit/reload absence check.
