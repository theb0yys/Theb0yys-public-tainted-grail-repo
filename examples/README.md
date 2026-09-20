# Examples

Use this section when you want a **small piece of code that demonstrates one modding technique clearly**.

Examples are intentionally narrower than complete mods. They are useful for answering questions such as:

- What does a basic Harmony guard look like?
- How do I observe damage without changing it?
- How should an audio replacement fail open?
- What does a small runtime UI overlay own?
- How do I structure a shared-infrastructure integration?

## Choose the environment first

- [Mono](mono/README.md) — BepInEx 5 / managed Mono examples.
- [IL2CPP](il2cpp/README.md) — BepInEx 6 / IL2CPP-compatible examples.
- [Merlin](merlin/README.md) — examples for Questline's Merlin Workshop content-authoring workflow.
- [Hybrid](hybrid/README.md) — examples that combine authored content and runtime code, or otherwise cross more than one environment.

Inside each environment, examples are grouped by what they do: combat, items, audio, UI, rendering, infrastructure, and similar tasks.

## Example vs guide vs template

- **Example** — demonstrates one technique in a small amount of source.
- **Guide** — walks you through a task and explains the decisions.
- **Template** — a starter project you copy and build on.
- **Case study** — explains what happened in a real mod, including failures and corrections.

For lessons from complete mod work, use [Case Studies](../research/case-studies/README.md). For reusable starting projects, use [Templates](../templates/README.md).
