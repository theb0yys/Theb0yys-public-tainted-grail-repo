# Template Readiness and Registration

Use this page when a mod needs to resolve or introduce a FoA template.

Canonical architecture: [Templates and Registries](../../systems/core/templates-registries.md).

## Readiness

Normal template lookup should occur only after native loading is ready.

Relevant researched surfaces:

- `TemplatesProvider.AllLoaded`
- `TemplatesLoader.FinishedLoading`
- `TemplatesProvider.Get<T>(guid)`

Do not catch a readiness failure and continue with null/default data.

## Lookup contract

For an existing template:

1. wait for readiness;
2. use the exact native GUID;
3. request the expected concrete/type-compatible template;
4. fail closed when the identity or type does not match.

Do not substitute display names for native identity.

## Custom registration boundary

A Unity object existing in memory does not make it a FoA template.

The researched direct item route uses native template-map insertion and then verifies the new GUID through ordinary `TemplatesProvider` lookup.

That direct route is patch-sensitive because it reaches private/native internals. It is **not** a blanket public registrar for every template type.

## Collision checklist

Before a custom identity enters a registry:

- source GUID and custom GUID differ;
- custom GUID is stable and mod-owned;
- existing provider lookup does not already resolve another owner;
- source object remains unchanged;
- clone/definition has the required native shape;
- insertion is idempotent or duplicate-safe;
- normal lookup returns the expected custom object.

## Persistence warning

Registration in the current session does not prove save/load safety.

A saved object that serializes a template GUID may require that custom GUID to be resolvable early during a future restore.

See [Saving and Persistence](../../systems/world/saving-persistence.md).

## Verification levels

Separate these claims:

```text
definition constructed
→ native registration succeeded
→ provider lookup succeeded
→ downstream consumer used it
→ save/load restored it
```

Each arrow needs its own evidence.
