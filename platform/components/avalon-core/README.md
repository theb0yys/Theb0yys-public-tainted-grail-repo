# Avalon Core

**Status:** Read-only discovery by default unless a documented capability says otherwise.

Use Avalon Core when your mod needs shared:

- host/trust report readback;
- capability/adapter discovery;
- service-contract version checks;
- evidence/catalog lookup;
- reviewed shared planning/authority metadata.

## If your mod references Avalon Core directly

- reference the Avalon Core host assembly;
- declare a hard BepInEx dependency on `kane.tgfoa.avalon-core`;
- make each optional integration configurable;
- fall back to `action=none` or `action=read-only` when the required capability is unavailable or incompatible.

Useful public APIs include:

- `AvalonCore.Plugin.TrustReports`
- `AvalonCore.Plugin.Registry.TryGet(...)`
- the documented adapter registry's descriptor/capability/version/readiness queries.

## Discovery does not grant execution access

The existence of a capability descriptor does not authorize your mod to:

- call private adapters;
- spawn actors;
- mutate scenes;
- load arbitrary assets;
- write saves;
- bypass a service or capability that is intentionally unavailable.

See [Capability discovery](capability-discovery.md).
