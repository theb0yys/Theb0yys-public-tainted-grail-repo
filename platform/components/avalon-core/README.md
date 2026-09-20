# Avalon Core

Use Avalon Core when your mod needs shared discovery information such as trust reports, capability descriptors, adapter/service versions, evidence catalogs, or reviewed planning metadata.

The normal public baseline is read-only discovery. Do not treat the existence of a capability or adapter as permission to execute it unless that capability is explicitly documented for consumers.

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
