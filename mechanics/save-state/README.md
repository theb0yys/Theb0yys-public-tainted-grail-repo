---
document_type: mechanic
scope: mod-owned durable state
runtime: mono
evidence:
  static: PARTIAL
  runtime: NOT_RUN_FOR_GENERIC_SIDECAR
  persistence: BLOCKED_FOR_GENERIC_IMPLEMENTATION
last_verified: 2026-09-20
---

# Mod-Owned Save State

There is currently **no supported public mechanic here for injecting an arbitrary mod-owned FoA save domain**.

## Current-binary static result

For the inspected `TG.Main.dll` SHA-256:

`749AABBFBEC121BB69BDA0AE226223154406D2C990DF3312AD12365D513FA982`

the native save-domain set is fixed. The inspected code exposes no supported mutable arbitrary-domain registrar.

An unknown `<name>.data` file may be cached and round-trip into a later archive, but the native restoration path does not construct an arbitrary matching Domain and does not deserialize it as game/mod state.

**Passive round-trip is not restoration.**

## Consequence

Do not publish a recipe that:

- fabricates a Domain through reflection;
- appends arbitrary domains to `DomainUtils.SaveSlotDomainsInUse`;
- patches save readers/writers to inject an unsupported domain;
- writes arbitrary files into native FoA save archives.

## Research direction

A **mod-owned sidecar** is the current design direction, with separate capture/commit and stage/apply transactions. That design still requires runtime/save/crash/rename/delete/copy validation before becoming a public production mechanic.

See [Persistence investigation](../../investigate/persistence/README.md) and [Native save-domain negative evidence](../../case-studies/persistence/native-save-domain-boundary.md).
