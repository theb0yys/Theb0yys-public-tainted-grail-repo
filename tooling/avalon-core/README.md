# Avalon Core

**Posture: Read-only/discovery baseline unless a named capability says otherwise**

**Obtain:** Tainted Core public package — https://www.nexusmods.com/taintedgrailthefallofavalon/mods/193

**Version signal:** current public source/plugin line is `0.8.4`.

**Stability boundary:** documented trust/registry/capability discovery is the supported read-only surface. Discovery does not promote private adapters or gameplay mutation into public APIs.

See [distribution/versioning](../ecosystem/distribution-and-versioning.md) and [API stability](../ecosystem/api-stability.md).

Use Avalon Core when your mod needs shared:

- host/trust report readback;
- capability/adapter discovery;
- service-contract version checks;
- evidence/catalog lookup;
- reviewed shared planning/authority metadata.

## Runtime dependency baseline

A directly referencing consumer should:

- reference the Avalon Core host assembly;
- declare a hard BepInEx dependency on `kane.tgfoa.avalon-core`;
- config-gate each optional integration;
- fail closed to `action=none` or `action=read-only` when unavailable/incompatible.

Useful public surfaces include:

- `AvalonCore.Plugin.TrustReports`
- `AvalonCore.Plugin.Registry.TryGet(...)`
- the documented adapter registry's descriptor/capability/version/readiness queries.

## Critical boundary

Discovery is not execution.

The existence of a capability descriptor does not authorize your feature mod to:

- call private adapters;
- spawn actors;
- mutate scenes;
- load arbitrary assets;
- write saves;
- bypass a blocked downstream owner.

See [Capability discovery](capability-discovery.md).
