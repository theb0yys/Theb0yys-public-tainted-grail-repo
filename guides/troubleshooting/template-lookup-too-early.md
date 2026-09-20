---
document_type: troubleshooting
scope: template lookup/creation before TemplatesProvider readiness
last_verified: 2026-09-20
---

# Template Lookup Is Too Early

A recurring lifecycle precondition is:

`TemplatesProvider.AllLoaded == true`

If exact template-backed creation or lookup fails early in startup, first determine whether the provider is present **and fully loaded**.

## Symptoms

- exact GUID resolves later but not during startup;
- custom registration retries indefinitely;
- item/recipe creation sees null/missing source templates;
- a one-shot scan returns zero rows before gameplay state is ready.

## Diagnosis

1. Confirm `World.Services` exists.
2. Resolve `TemplatesProvider`.
3. Check `AllLoaded`.
4. For domain-specific content, verify the relevant rows are actually loaded; `AllLoaded=true` does not guarantee every UI/station-specific runtime surface is populated in the current scene/state.
5. Retry through a researched readiness event or bounded retry—not an unbounded per-frame mutation loop.

See [Item template resolution](../../knowledge/mechanics/items/template-resolution-and-grant.md).
