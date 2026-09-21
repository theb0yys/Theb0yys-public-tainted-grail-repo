# Test Notes

## Build

- Release build passed for version 0.1.15 with 0 warnings and 0 errors after correcting the split between native NPC dialogue and the hotkey debug panel:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Local release DLL file version: `0.1.15.0`; SHA-256 `3CE5A60E4EA2BD34C6A7F0DFA57A28E40CECC3CAE20C6427CCD768EABDF7331F`.
- In-game split validation has not been run yet.
- Release build passed for version 0.1.14 with 0 warnings and 0 errors after replacing the visible centered IMGUI proof panel with a companion-style Unity UI command surface:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Local release DLL file version: `0.1.14.0`; SHA-256 `1B90E4D525905DE4597FB19BAAC96E77457C5937252A7D314F8BAE1959CE0155`.
- In-game visual/native-prompt validation for the companion-style command surface has not been run yet.
- Release build passed for version 0.1.13 with 0 warnings and 0 errors after making `HumanCommands.EnableNativeCommandActions` and `HumanCommandPanel.EnableProofCommandPanel` default `true` for newly generated configs:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Local release DLL file version: `0.1.13.0`; SHA-256 `B7FA193A4425D024ACF0E05D00FB829A2CD1A0AA89A59FB89D775C5E9B245AAC`.
- Clean config generation and in-game native prompt validation with the new defaults have not been run yet.
- Release build passed for version 0.1.12 with 0 warnings and 0 errors after adding the runtime-only native `Companion` prompt bridge to the existing proof panel:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- In-game native prompt validation has not been run yet.
- Release build passed for version 0.1.11 with 0 warnings and 0 errors after optional Tainted Interface proof-panel style/scope integration:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.11 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.11 DLL hash matched local Release build output SHA-256 `12DBA3890230B8A226141713849230C04FF90DDC588486D54E15EADE94F3AECF`; file version was `0.1.11.0`.
- Release deploy build passed on 2026-06-15 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Earlier scaffold-only deployed DLL hash matched local Release build output SHA-256 `C815D7D178877D4F7B665DD5D7A48BED4D356AD81DA76C044CDBEABAAA7039E1`.
- Release build passed after adding the safe proof spawn command with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed after adding the safe proof spawn command with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed DLL hash matched local Release build output SHA-256 `F9037631E0E8BCC0C974525A21088A6B314B8075A62483E60A908EFB76DAE8C5`.
- Release build passed after adding the disabled actor scanner with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.1 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.1 DLL hash matched local Release build output SHA-256 `B10341DD4AEC6DC607AB2129FBBEEB835D99E8B4B787FCD38B428E12625F6FBE`.
- Release build passed for version 0.1.2 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.2 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.2 DLL hash matched local Release build output SHA-256 `4129694CFC93F13CF478BF56164FE1B42EF92A35F068DFE4301CFAC12C8BAFD0`.
- Release build passed for version 0.1.3 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.3 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.3 DLL hash matched local Release build output SHA-256 `1B6F13B0612827B7C1FE9A9B0B9BBDAD70B242554CA7FFC123D8E4AE672BD82C`.
- Initial 0.1.4 panel build failed because `UnityEngine.IMGUIModule` and `UnityEngine.TextRenderingModule` were not referenced. The references and MSBuild validation checks were added.
- Release build passed for version 0.1.4 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.4 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.4 DLL hash matched local Release build output SHA-256 `BCE887854A46EE4A4CB117C1A2A9F0D66E135D1E94B0FF0BBDC61C54C8F544D7`.
- Release build passed for version 0.1.5 with 0 warnings and 0 errors after the proof panel layout polish:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.5 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.5 DLL hash matched local Release build output SHA-256 `0EE75B0D45834BABEC4EBCF6C1A83A969115DA3472EEEFEED552E1D5214612A5`.
- Release build passed for version 0.1.6 with 0 warnings and 0 errors after adding the gated proof-panel scanner trigger:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.6 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.6 DLL hash matched local Release build output SHA-256 `DAB81B687CA3FFABD3092EF7A96483FB0C142EDE5DD6095282A84B448DB8BE33`.
- Release build passed for version 0.1.7 with 0 warnings and 0 errors after adding lifecycle diagnostics/guardrails:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.7 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.7 DLL hash matched local Release build output SHA-256 `D554D9993BB3236C871D9A488B2AA34D9A61FFED7F0547C8AD850A728B42B1A2`; file version was `0.1.7.0`.
- Release build passed for version 0.1.8 with 0 warnings and 0 errors after adding the proof-only Hold runtime movement block:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.8 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.8 DLL hash matched local Release build output SHA-256 `082B296FC05AE51E75E7B1295C039C2736F4A6FF86E01E8D3967C50FBC82B5C8`; file version was `0.1.8.0`.
- Release build passed for version 0.1.9 with 0 warnings and 0 errors after panel flow/input lock and diagnostic-map generator changes:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.9 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.9 DLL hash matched local Release build output SHA-256 `9BB774BA4BE8966D78F444C10B73061EA9CB5C494099BB5B5759B41D917076FC`; file version was `0.1.9.0`.
- Release build passed for version 0.1.10 with 0 warnings and 0 errors after proof panel size/button-state polish:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA"`
- Release deploy build passed for version 0.1.10 with 0 warnings and 0 errors:
  `dotnet build .\mods\avalon-human-companions\src\AvalonHumanCompanions.csproj -c Release -p:FoAGameRoot="A:\SteamLibrary\steamapps\common\Tainted Grail FoA" -p:DeployOnBuild=true`
- Deployed 0.1.10 DLL hash matched local Release build output SHA-256 `38B81B350893816C3436FAC512DDB1D9F4E14D1439F54ADCBCEC0DEE586AA81D`; file version was `0.1.10.0`.

## Research proof

- `ilspycmd` class-list and targeted type decompilation passed on 2026-06-15 against local `TG.Main.dll`.
- Evidence recorded in `docs/research/human-npc-proof-step-2026-06-15.md`.
- Safe proof spawn target selected from existing template diagnostics evidence: `Spec_Enemy_Generic_Tier1_Outlaw_1H` / `2bd34a05d1e1fb94f9770b9ee7f23be2`.
- Result: human NPC behavior remains blocked. The approved current code step is a disabled scanner that writes classification evidence only.
- 2026-06-15: creature companion research and source were reread for the exact native ally path used by Avalon Companions one-session creature candidates: per-instance summon-faction override plus `NpcHeroPetAlly`.

## Game launch

- `LogOutput.log` confirmed `Avalon Human Companions 0.1.2 loaded` on 2026-06-15 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- `LogOutput.log` confirmed `Avalon Human Companions 0.1.4 loaded` on 2026-06-15 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- `LogOutput.log` confirmed `Avalon Human Companions 0.1.6 loaded` on 2026-06-15 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- `LogOutput.log` confirmed `Avalon Human Companions 0.1.7 loaded` on 2026-06-16 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- `LogOutput.log` confirmed `Avalon Human Companions 0.1.8 loaded` on 2026-06-16 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- `LogOutput.log` confirmed `Avalon Human Companions 0.1.9 loaded` on 2026-06-16 with `ResearchModeOnly=False` and target `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`.
- `LogOutput.log` confirmed the 0.1.9 panel input lock patched `GameUI.UpdateMousePosition`, `PlayerInput.ProcessLateUpdate`, 8 Rewired axis overloads, and 34 Rewired button overloads.

## Feature checks

- Plugin load: not run.
- Config generation: not run.
- Disabled recruitment-test warning: not run.
- Safe proof spawn command with `Spec_Enemy_Generic_Tier1_Outlaw_1H`: user smoke-tested in game on 2026-06-15 and reported it worked. Screenshot evidence shows the spawned human outlaw. Codex has not rechecked the post-success BepInEx log.
- Disabled human NPC proof scanner: build passed; runtime CSV output not tested yet.

## Live troubleshooting

- 2026-06-15: BepInEx log confirmed the proof spawn hotkey path fired, but every attempt was blocked with `Target.TemplateGuid is empty`.
- Live config had `ProofSpawn.EnableSafeSpawnCommand=true` and `ProofSpawn.SafeSpawnHotkey=F4`, but `Target.TemplateName` and `Target.TemplateGuid` were empty.
- Live config was updated to `Spec_Enemy_Generic_Tier1_Outlaw_1H` / `2bd34a05d1e1fb94f9770b9ee7f23be2`.
- 2026-06-15: user reported the proof spawn works perfectly after the config target was set.
- 2026-06-15: live config was updated for scanner validation: `ActorScanner.EnableDisabledActorScanner=true`, `ActorScanner.ScannerHotkey=F6`, `ActorScanner.ScanRadius=35`, and `ActorScanner.SelectedTarget=2bd34a05d1e1fb94f9770b9ee7f23be2`. A backup was written beside the live config before editing.
- 2026-06-15: `LogOutput.log` confirmed `Avalon Human Companions 0.1.1 loaded`. FoA was running when the scanner config file was edited, so the config may need the mod manager apply/reload path or a game restart before `F6` is active in memory.
- 2026-06-15: user reported the proof-spawned outlaw still attacks. This is expected for `Spec_Enemy_Generic_Tier1_Outlaw_1H` because it is a hostile enemy template and the proof spawn deliberately applies no faction edit, ally marker, follow command, or behavior override.
- 2026-06-15: live config was updated to prevent more hostile proof spawns: `ProofSpawn.EnableSafeSpawnCommand=false` and `ProofSpawn.SafeSpawnHotkey=None`. `ActorScanner.EnableDisabledActorScanner=true` and `ActorScanner.ScannerHotkey=F6` remain set for evidence collection. A backup was written beside the live config before editing.
- 2026-06-15: no scanner CSV output was found under `BepInEx/config/kane.tgfoa.avalon-human-companions` during the attack report check.
- 2026-06-15: version 0.1.2 adds a disabled-by-default one-session native ally proof. This is intended to answer the hostile outlaw report by testing the same native ally marker path used by Avalon Companions creature candidates on a plugin-owned spawned human proof actor.
- 2026-06-15: `LogOutput.log` confirmed `Avalon Human Companions 0.1.2 loaded` with `ResearchModeOnly=False` and the reviewed outlaw target after the final deploy.
- 2026-06-15: live config was backed up to `kane.tgfoa.avalon-human-companions.cfg.codex-backup-20260615-humanallyproof`, old proof spawn stayed disabled, and one-session ally proof was set to `PageUp` spawn and `PageDown` dismiss.
- 2026-06-15: user smoke-tested the one-session ally proof in game and reported "looks perfect", with screenshot evidence at `C:/Users/kane0/Pictures/Screenshots/Screenshot (3616).png` showing the spawned human proof actor standing close to the player without visible combat UI.
- 2026-06-15: Codex searched `LogOutput.log` after the screenshot smoke test. The final `0.1.2` load was confirmed, but no `native ally setup applied` or `one-session ally proof created` line was present in the log search.
- 2026-06-15: version 0.1.3 adds disabled-by-default native quick command actions for the active one-session human ally proof actor only: Follow, Come Close, Recall, Defend, and Dismiss. True Stay/Wait/Hold Position remains blocked.
- 2026-06-15: live config was backed up to `kane.tgfoa.avalon-human-companions.cfg.codex-backup-20260615-humancommands`, old proof spawn stayed disabled, one-session ally proof stayed on `PageUp`/`PageDown`, and `HumanCommands.EnableNativeCommandActions=true` was set for testing.
- 2026-06-15: no Fall of Avalon process was found during the post-deploy process check, so the next launch should load the 0.1.3 DLL. BepInEx log confirmation for 0.1.3 has not been run yet.
- 2026-06-15: user reported the native command path was not working and requested a panel hotkey, suggesting `End` if unused. `End` was not reserved in `docs/hotkey-registry.md`.
- 2026-06-15: version 0.1.4 adds a disabled-by-default proof command panel with `HumanCommandPanel.TogglePanelHotkey=End`. It calls the existing proof backend for Spawn Proof, Follow, Come Close, Recall, Defend, and Dismiss only.
- 2026-06-15: live config was backed up to `kane.tgfoa.avalon-human-companions.cfg.codex-backup-20260615-humanpanel`, `HumanCommands.EnableNativeCommandActions=false` was set, and panel keys were added.
- 2026-06-15: live config was backed up again to `kane.tgfoa.avalon-human-companions.cfg.codex-backup-20260615-humanpanel-sectionfix` and the new panel keys were moved under the correct `[HumanCommandPanel]` section: `EnableProofCommandPanel=true`, `TogglePanelHotkey=End`.
- 2026-06-15: Fall of Avalon process `72520` was running after deploy, so the current live process may still have the old plugin loaded until restart.
- 2026-06-15: user smoke-tested the 0.1.4 proof command panel in game and reported "commands work perfectly". Screenshot evidence:
  - `C:/Users/kane0/Pictures/Screenshots/Screenshot (3623).png` shows the panel open in game.
  - `C:/Users/kane0/Pictures/Screenshots/Screenshot (3627).png` shows the proof ally active near hostile NPCs after command testing.
- 2026-06-15: user reported the UI still needs fixing. The panel remains proof UI only; a dedicated visual/layout pass is required before release-ready UI claims.
- 2026-06-15: live config check showed scanner evidence collection already enabled: `ActorScanner.EnableDisabledActorScanner=true`, `ActorScanner.ScannerHotkey=F6`, `ActorScanner.ScanRadius=35`, and `ActorScanner.SelectedTarget=2bd34a05d1e1fb94f9770b9ee7f23be2`.
- 2026-06-15: no scanner CSV files were present under `BepInEx/config/kane.tgfoa.avalon-human-companions` after the command-panel smoke test.
- 2026-06-15: `LogOutput.log` confirmed 0.1.4 loaded, the native ally setup applied for `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]`, a one-session proof actor was created and marked not saved, and the proof command panel opened and closed. Per-command action logs were not captured in that check.
- 2026-06-15: after the user said the scanner step was done, Codex checked the expected scanner output folder again. `BepInEx/config/kane.tgfoa.avalon-human-companions` did not exist, and `LogOutput.log` contained no actor scanner write or failure line. This suggests the scanner hotkey was not received in that run.
- 2026-06-15: version 0.1.5 polished the proof command panel after the user confirmed 0.1.4 commands worked but the UI needed fixing. The panel is now a larger responsive two-column modal with separated actor status and command sections, larger labels/buttons, clearer proof-only status text, and header dragging.
- 2026-06-15: 0.1.5 was built and deployed while Fall of Avalon process `19888` was still running, so the live folder has the new DLL but the active game process may need restart/apply before it loads 0.1.5.
- 2026-06-15: version 0.1.6 added a `Scan Actors` proof-panel button because the scanner hotkey path did not produce CSV output or a scanner log line. The button is enabled only by the existing `ActorScanner.EnableDisabledActorScanner` gate and calls the existing CSV-only scanner path.
- 2026-06-15: live config check after 0.1.6 deploy confirmed `ActorScanner.EnableDisabledActorScanner=true`, `ScannerHotkey=F6`, `ScanRadius=35`, `SelectedTarget=2bd34a05d1e1fb94f9770b9ee7f23be2`, `HumanCommandPanel.EnableProofCommandPanel=true`, `TogglePanelHotkey=End`, and `ResearchModeOnly=false`.
- 2026-06-15: no Fall of Avalon process was found after the 0.1.6 deploy check, so the next launch should load the 0.1.6 DLL.
- 2026-06-15: user ran the 0.1.6 panel scanner test. `LogOutput.log` confirmed 0.1.6 loaded, the proof command panel opened, and the actor scanner wrote `9` candidate rows, `28` component rows, and `6` dry-run command rows twice to `BepInEx/config/kane.tgfoa.avalon-human-companions`.
- 2026-06-15: the expected CSV files existed with sizes `7099`, `4206`, and `1930` bytes. CSV review found `9` candidate rows, `28` component rows, and `6` dry-run command rows. All dry-run rows were `blocked=true` and `liveAction=false`.
- 2026-06-15: the reviewed target GUID `2bd34a05d1e1fb94f9770b9ee7f23be2` had `0` candidate rows. The BepInEx log order explains this: the scanner wrote rows before `native ally setup applied` and `one-session ally proof created` were logged. The scan captured nearby world objects/wolves/chests, not the proof actor.
- 2026-06-15: user provided screenshot evidence at `C:/Users/kane0/Pictures/Screenshots/Screenshot (3640).png` and reported "looks good." The screenshot shows live combat against `Corpse Eater`, which is useful combat-context smoke evidence but not proof-panel UI evidence.
- 2026-06-15: latest scanner CSVs were written at `2026-06-15 18:53:40` with `4` candidate rows, `14` component rows, and `6` dry-run command rows. `TARGET_ROWS=0` for the reviewed outlaw GUID `2bd34a05d1e1fb94f9770b9ee7f23be2`. The dry-run candidate was `Spec_EnemyZombie_T1_Classic[1d110a8ec95ab1745a364562ec311e50]`. All dry-run command rows stayed `blocked=true` and `liveAction=false` with `BAD_SAFETY_ROWS=0`.
- 2026-06-15: user clarified that `mods/Tainted-Diagnostic Tool` is the release/public diagnostic package and all diagnostic dev work must use `mods/template-diagnostics`. Codex documented this in `docs/diagnostic-tool-development-policy.md`.
- 2026-06-15: live BepInEx log confirmed `FOA-Diagnostic Tool 0.4.7 loaded` with `DumpHotkey=F3`, matching the dev diagnostic tool path. The newest dump checked was `A:\SteamLibrary\steamapps\common\Tainted Grail FoA\BepInEx\config\kane.tgfoa.template-diagnostics\20260615-191733`.
- 2026-06-15: dump `20260615-191733` was not valid proof-actor evidence because `runtime_snapshot.txt` reported `sceneServiceActiveScene=TitleScreen`, `heroAvailable=false`, and `runtime_object_counts.csv` reported `World.All,NpcHeroPetAlly,0`. The dump did include `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]` in `templates.csv` and `research_term_matches.csv`, but that only proves the template is loaded, not that the spawned proof actor was visible or captured.
- 2026-06-15: proof actor live capture gate passed with a logging caveat. Dev FOA-Diagnostic Tool dump `20260615-193847` reported `FOA-Diagnostic Tool 0.4.8`, `CampaignMap_HOS`, `heroAvailable=true`, hero coordinates `-1678.583|56.446|-3711.768`, `heroInCombat=false`, `NpcHeroPetAlly=1`, and `NpcHeroSummon=1`.
- 2026-06-15: scanner files written at `2026-06-15 19:38:39` contained `5` candidate rows, `39` component rows, and `6` command dry-run rows. The reviewed proof target appeared once in `human-actor-candidates.csv` as `RuntimeLocation:Spec_Enemy_Generic_Tier1_Outlaw_1H:598`, coordinates `-1681.411|56.399|-3705.65`, distance `6.741`, `markedNotSaved=true`, `npcIsUnique=false`, `hasNpcElement=true`, `hasAlive=true`, `hasHeroPetAlly=true`, `behaviorApproved=false`, and `panelApproved=false`.
- 2026-06-15: `human-actor-components.csv` contained `25` target component rows, including `NpcElement`, `NpcHeroPetAlly`, and `RepetitiveNpcAttachment`. `human-actor-command-dry-run.csv` contained `6` target dry-run rows with `BAD_SAFETY_ROWS=0`; all rows stayed `blocked=true` and `liveAction=false`.
- 2026-06-15: caveat for the proof actor live capture gate: the current `LogOutput.log` excerpt did not include a manual `F3` trigger line for `20260615-193847`. The evidence is accepted from the valid playable-scene dump content and matching scanner CSV timestamp. Later dump `20260615-194153` reported `NpcHeroPetAlly=0` and is not used as the gate capture.
- 2026-06-15: next gate documented as proof actor lifecycle/no-persistence. It requires Dismiss removal, transition/long-move duplicate checks, and save/quit/reload absence checks before adding more human companion behavior.
- 2026-06-16: version 0.1.7 implemented lifecycle diagnostics/guardrails for the active proof actor. The guard keeps the actor not saved, discards it if the managed proof marker is lost, clears the tracked actor after Dismiss/discard, and writes `human-proof-lifecycle.csv` evidence rows. This is not persistence or recruitment.
- 2026-06-16: 0.1.7 load was confirmed, but the lifecycle/no-persistence gate did not pass. Live config had lifecycle diagnostics, panel, scanner, and target GUID enabled, but `human-proof-lifecycle.csv` was missing. The latest Avalon Human Companions scanner CSVs were still from `2026-06-16 06:28:55`, before the 0.1.7 deploy confirmation, with `0` rows for the reviewed target GUID and `BAD_SAFETY_ROWS=0`.
- 2026-06-16: dev FOA-Diagnostic Tool dumps from `20260616-065331` through `20260616-070419` were playable-scene dumps with `heroAvailable=true`. Most reported `NpcHeroPetAlly=0`; dump `20260616-070419` reported `NpcHeroPetAlly=1` and `NpcHeroSummon=1`, but the reviewed target GUID only appeared in template/spawner reference files and there was no matching Avalon Human Companions lifecycle CSV, scanner target row, or spawn/dismiss log line. This is not proof-actor lifecycle evidence.
- 2026-06-16: follow-up lifecycle evidence at `07:22` partially passed. `human-proof-lifecycle.csv` was written with `13` rows: one proof creation, three guard ticks, five recall rows, two mode-set rows, one dismiss-requested row, and one discarded row. All lifecycle rows kept `behaviorApproved=false` and `persistenceApproved=false`. Active rows for `Spec_Enemy_Generic_Tier1_Outlaw_1H[2bd34a05d1e1fb94f9770b9ee7f23be2]` had `markedNotSaved=true`, `hasNpcElement=true`, `npcIsUnique=false`, and `hasHeroPetAlly=true`. The discard row for `RuntimeLocation:Spec_Enemy_Generic_Tier1_Outlaw_1H:646` had `tracked=false`, `locationDiscarded=true`, and `hasHeroPetAlly=false`.
- 2026-06-16: `LogOutput.log` confirmed the proof panel opened, the scanner wrote rows at `07:22:14`, the proof actor spawned at `07:22:16`, panel commands ran, and the actor was discarded by panel Dismiss at `07:22:27`. Dev FOA-Diagnostic Tool manual F3 dump `20260616-072231` was playable with `heroAvailable=true`, `NpcHeroSummon=0`, and `NpcHeroPetAlly=0`, supporting no pet-ally/summon leftover after Dismiss.
- 2026-06-16: the lifecycle/no-persistence gate still did not pass because the scanner ran before proof actor spawn. The `07:22:14` scanner output had `11` candidates, `45` components, `6` dry-run rows, `BAD_SAFETY_ROWS=0`, and `0` rows for the reviewed target GUID. Missing evidence remains: scanner capture after spawn, scanner absence after Dismiss, transition/long-move duplicate check, and save/quit/reload absence check.
- 2026-06-16: version 0.1.8 adds proof-only Hold for the active one-session proof actor. Hold uses a plugin-owned runtime-only `ICanMoveProvider` element on the active proof actor's `NpcElement`, removes it on Follow, Come Close, Defend, Dismiss, or invalid-state guard cleanup, and skips follow catch-up while Hold is active. Build/deploy passed; in-game Hold validation is still required.
- 2026-06-16: version 0.1.9 reorganizes the proof panel flow and adds the Harmony/Rewired input lock used by Avalon Companions and FoA Mod Manager. Build/deploy passed; in-game panel focus/input validation is still required.
- 2026-06-16: dev FOA-Diagnostic Tool dump `20260616-090615` was used for the human NPC map. The dump reported `FOA-Diagnostic Tool 0.4.7`, scene `CampaignMap_HOS` / `Horns of the South`, `heroAvailable=true`, and hero coordinates `-2165.052|112.146|-3704.131`.
- 2026-06-16: `tools/New-HumanNpcDiagnosticMap.ps1` generated `docs/generated/human-npc-diagnostic-map-20260616-090615.csv` and `.md` with 544 review-only rows: 42 `ReviewQueueNonUniqueSpawnerBacked`, 102 `NeedsSpawnerEvidence`, and 400 `BlockedUniqueOrStory`. Safety approval check passed: every row kept `safeSpawnCandidate=false`, `rosterApproved=false`, `behaviorApproved=false`, and `persistenceApproved=false`.
- 2026-06-16: user smoke-tested the 0.1.9 proof panel focus/input lock in game and reported the test passed perfectly. `LogOutput.log` confirmed 0.1.9 load, panel input-lock patch application, proof panel open/close events, and Hold/Follow command events. Codex did not personally observe the camera/focus behavior on screen during this run, so the runtime pass is user-reported plus log-confirmed.
- 2026-06-16: user provided screenshot evidence at `C:/Users/kane0/Pictures/Screenshots/Screenshot (3687).png` showing the 0.1.9 proof panel was still too small and vertically clipped near the bottom, and reported button color looked wrong after using an action.
- 2026-06-16: version 0.1.10 increases the proof panel default size to `1320x860`, increases the minimum panel size and status column width, slightly increases text/button sizing, clears IMGUI hot/keyboard focus after actions, pins explicit button states, and changes selected command buttons to a calmer teal state. Build/deploy passed and local/live DLL hashes match. In-game 0.1.10 visual validation is still required.
- 2026-06-19: version 0.1.11 adds optional Tainted Interface shared dark-fantasy styling and shared custom UI scope for the disabled-by-default proof command panel. The local IMGUI skin and direct FoA Mod Manager scope fallback remain intact. Build/deploy passed and local/live DLL hashes match. In-game 0.1.11 visual/scope validation and no-layer fallback validation are still required.
- 2026-06-21: version 0.1.12 adds a runtime-only native `Companion` prompt bridge for the active one-session proof actor. When `HumanCommands.EnableNativeCommandActions=true` and `HumanCommandPanel.EnableProofCommandPanel=true`, the prompt opens the existing proof panel and removes the older separate quick command action elements from the native prompt surface. Release build passed; in-game native prompt validation is still required.
- 2026-06-21: version 0.1.13 makes `HumanCommands.EnableNativeCommandActions` and `HumanCommandPanel.EnableProofCommandPanel` default `true` for newly generated configs. Existing BepInEx configs are not overwritten by BepInEx default-value changes. Release build passed; clean config generation and in-game native prompt validation are still required.
- 2026-06-21: version 0.1.14 replaces the visible centered IMGUI proof panel with a companion-style vanilla Unity UI command surface. It uses right-side choices and a bottom dialogue text band like Avalon Companions, while routing choices to the existing proof backend. Release build passed; in-game visual/native-prompt validation is still required.
- 2026-06-21: version 0.1.15 corrects the 0.1.14 routing mistake. The native NPC `Companion` prompt opens the companion-style dialogue surface, and `End` opens the compact debug/control panel. Release build passed; in-game split validation is still required.
- 2026-06-21: version 0.1.21 makes Follow reselectable and clamps Follow-mode bounded catch-up recall to the closer proof range. Release build passed with 0 warnings and 0 errors.
- 2026-06-24: live 0.1.21 DLL deployment completed after the game process was closed. Local and live SHA-256 hashes matched: `411E6EEBB39BFCF6DDE978016E36CEDAF7E1E711BD674E71C29A63DADD254885`.
- 2026-06-24: log check confirmed `Avalon Human Companions 0.1.21 loaded`, runtime native `Companion` prompt attachment, dialogue open/direct-click handling, and repeated Follow-mode catch-up recalls. `human-proof-lifecycle.csv` contained 44 `recall,follow catch-up` rows for the tracked `Spec_Enemy_Generic_Tier1_OutlawArcher` proof actor with mode `Follow`. This is log/CSV validation only; visual confirmation is still required.

## Skipped checks

- Human NPC recruitment, cloning, conversion of existing NPCs, custom combat, dialogue, release-ready full panel behavior, true Stay/Wait, saved hold positions, and persistence are intentionally not implemented. The only behavior proof is a default-off one-session native ally marker test plus a default-on proof command panel gate for one explicit non-unique spawned proof actor. The actor scanner writes CSV evidence only.
- Per-command BepInEx log confirmation for the command-panel actions has not been captured yet.
- 0.1.4 cursor/input restoration after panel close has not been separately validated.
- 0.1.6 proof panel screenshot validation has not been run yet.
- Proof actor lifecycle/no-persistence gate has not passed yet; lifecycle CSV rows now exist, but scanner output after spawn, scanner output after Dismiss, transition/long-move evidence, and save/quit/reload evidence are still required.
- 0.1.8 proof-only Hold in-game validation has not been run yet.
- Independent screenshot/video capture of the 0.1.9 proof panel focus/input test has not been captured yet.
- In-game 0.1.10 proof panel size/button-state visual validation has not been run yet.
- In-game 0.1.11 Tainted Interface proof-panel style/scope validation and no-layer fallback validation have not been run yet.
- Clean config generation for 0.1.13 default gate values has not been run yet.
- In-game 0.1.14 native `Companion` prompt validation has not been run yet.
- In-game 0.1.14 visual validation for the companion-style command surface has not been run yet.
- In-game 0.1.15 native-dialogue-versus-debug-panel split validation has not been run yet.
- Independent visual 0.1.21 Follow validation has not been captured yet.
