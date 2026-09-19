# Proven-Path Templates

These are **clean-room templates**, not stripped copies of finished mods.

The maintainer's working mod workspace was inspected for paths that had more than source code or a successful build. A path was promoted here only when there was useful runtime evidence for the underlying mechanism.

The public examples then reimplement only the reusable mechanism with neutral, self-owned demo targets.

## Testing-status rule

A source mechanism that worked in the maintainer's environment does **not** make a newly written public template automatically runtime-proven.

For every example below:

- source mechanism: checked in the maintainer's working environment;
- public clean-room template: **NOT_RUN** until somebody builds and tests this exact example;
- any game target you substitute: **NOT_PROVEN** until you verify that exact target on your installed build.

For the exact meanings of those formal labels, see [Testing and Evidence Status](../../docs/EVIDENCE.md).

## First batch

| Template | Teaches | Why this shape was promoted |
| --- | --- | --- |
| [01-harmony-result-postfix-mono](01-harmony-result-postfix-mono/README.md) | Let native/original logic run, then narrowly adjust its returned decision | The owner-side HUD path has live load/patch evidence and historical visual evidence using this result-postfix shape |
| [02-harmony-action-guard-mono](02-harmony-action-guard-mono/README.md) | Block an action only when a guard fails; otherwise preserve the original path | The owner-side interaction guard has live plugin evidence and passed allow/block user checks |
| [03-runtime-ui-overlay-mono](03-runtime-ui-overlay-mono/README.md) | Small BepInEx-owned IMGUI surface with explicit toggle and no game-state ownership | Owner-side UI mods have repeated live load/deploy and screenshot evidence; this template keeps only the UI lifecycle |
| [04-audio-replacement-gate-mono](04-audio-replacement-gate-mono/README.md) | Attempt replacement audio, suppress native/original only after replacement starts, fail open on error | The owner-side audio replacement path logged real replacement playback and proved the filtered-prefix/suppress-after-success pattern |
| [05-skybox-runtime-ownership-mono](05-skybox-runtime-ownership-mono/README.md) | Capture previous Unity state, apply only mod-owned runtime objects, restore and destroy them on unload | The owner-side skybox path has live apply/active markers and same-session visual evidence |

## Why the demos patch themselves

The template's demo target is code inside the template itself.

That is intentional.

It proves the **shape** without publishing a private feature target or pretending an old symbol is valid on your current game build. To turn a demo into a real mod:

1. research the current game target;
2. add only the local assembly reference you actually need;
3. replace the demo target with the exact verified type/method;
4. keep the guard/fail-open/cleanup invariants;
5. build;
6. confirm patch ownership/load;
7. test the exact behavior in game.

## Not promoted yet

These categories have useful work in the maintainer's workspace, but the current evidence does not justify publishing them here as end-to-end proven templates:

- **custom weapon runtime registration/presentation** — live promotion gates remain incomplete;
- **armour runtime import/equip** — strong source/importer work exists, but this pass did not establish a compact end-to-end runtime path fit for a beginner template;
- **creature/companion spawn/follow/combat** — available companion scaffolds explicitly keep behavior blocked or unproved;
- **save backup behavior** — plugin load/config/folder creation reached a higher state than actual snapshot-archive proof;
- **jump/fall-damage/economy tweaks** — current feature validation is incomplete;
- **camera-relative movement/orbit** — useful partial live evidence exists, but fallback/context matrices remain incomplete.

Those should be promoted only when their exact path has sufficient proof. Do not fill the gaps by copying code and calling it proven.

## Runtime lane

This first proven-path batch is **Mono / BepInEx 5**, because that is where these mechanism paths have direct owner-side evidence.

Do not mechanically port them to IL2CPP and label the result proven. The repository already has a separate IL2CPP starter under \`templates/il2cpp-basic/\`; equivalent IL2CPP mechanism templates should be added only after direct IL2CPP evidence exists.
