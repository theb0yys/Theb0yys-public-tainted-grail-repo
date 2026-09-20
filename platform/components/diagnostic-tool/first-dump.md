# First Diagnostic Dump

1. Install the Diagnostic Tool release into its BepInEx plugin folder.
2. Confirm the plugin loaded in `BepInEx/LogOutput.log`.
3. Load the save/scene relevant to your question.
4. Stand near the object/NPC/route/station when location context matters.
5. Trigger the configured manual dump hotkey.
6. Open the newest timestamped dump folder.
7. Read `summary.txt` and `runtime_snapshot.txt` first.
8. Then open the CSV matching your question.

## Useful files

| Question | File |
| --- | --- |
| item/template identity | `item_templates.csv`, `templates.csv` |
| recipe membership | `recipe_visibility.csv` |
| creature/spawner evidence | `creature_templates.csv`, `spawner_refs.csv` |
| actor authority refs | `actor_authority_refs.csv` |
| scene/route context | `scene_entity_context.csv`, `patrol_path_context.csv` |
| progression source data | `proficiency_progression.csv` |
| knowledge gaps | `game_knowledge_records.csv`, `game_knowledge_gaps.csv` |

A row is evidence, not approval.
