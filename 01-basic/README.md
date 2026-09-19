# 01 — Basic

This level is about becoming comfortable with the everyday mod-development loop.

## Runtime plug-in workflow

Use this loop:

edit → build → copy your DLL → launch → inspect the log → test one behaviour → repeat

Keep source outside the game directory. For a basic deployment, only your compiled plug-in DLL belongs in BepInEx/plugins.

A small plug-in startup should usually:

1. bind configuration;
2. initialize small services if needed;
3. install patches;
4. log one useful startup line.

Do not perform huge scans or broad state mutation during startup unless the feature genuinely requires it.

## Configuration

Good configuration values are:

- clearly named;
- documented;
- safe by default;
- bounded where practical.

Do not use a mod config as a secret store.

## Logging

Your logs should answer:

- Did the mod load?
- Which version loaded?
- Which important feature initialized?
- Why was a feature disabled?
- What failed first?

Avoid per-frame log spam and avoid dumping saves, proprietary content, credentials, or unnecessary private paths.

## Harmony basics

The common patch types are:

**Postfix** — runs after the original method. Often the safest first option.

**Prefix** — runs before the original method and can inspect or alter inputs.

**Transpiler** — rewrites method instructions. Powerful, but more fragile across updates.

Beginner preference:

1. postfix;
2. prefix;
3. transpiler only when the first two cannot express the change safely.

The existing examples/mono-harmony-self-test project demonstrates the mechanics by patching only its own method.

## Basic content-authoring habits

For items, weapons, armour, and creatures:

- use a unique mod-owned identity;
- understand the logical data separately from the visual representation;
- make one definition work before tuning everything;
- keep worn/equipped representations separate from world-drop representations when the pipeline does;
- validate each stage before adding the next one.

## When to move to Foundational

Move to 02-foundational when you can repeat the build/test loop reliably and can normally identify which layer failed.
