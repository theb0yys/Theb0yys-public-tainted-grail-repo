---
document_type: framework
scope: Tainted Grail Extender manifest discovery format
runtime: mono
evidence:
  parser: FIXTURE_VALIDATED
  activation: BOUNDED_RUNTIME_RECEIPTS
last_verified: 2026-09-20
---

# TGE Extension Manifest V1

Tainted Grail Extender uses an explicit opt-in manifest:

`tge-extension.manifest`

The host scans **top-level plugin directories only**; it does not recursively discover arbitrary child manifests.

## Required keys

```text
manifestVersion=1
extensionId=<non-empty>
enabled=true|false
loadOrder=<Int32>
assembly=<local .dll filename only>
```

The parser rejects:

- missing required keys;
- unsupported manifest version;
- duplicate keys;
- malformed lines;
- invalid booleans/integers;
- path-qualified or non-DLL assembly values.

Unknown keys are currently ignored and cannot be used to claim feature negotiation.

## Separate versions

Manifest schema version is not the same thing as:

- TGE product version;
- public API version;
- extension implementation version;
- capability contract version;
- persisted schema version.

Keeping those version axes separate avoids accidental compatibility claims.

## Activation boundary

A valid manifest means only that the package can enter the host's discovery/activation pipeline. It does not prove save safety, gameplay compatibility or release readiness.
