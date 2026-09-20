# TGE Extension Manifests

TGE discovers explicit top-level extension manifests named:

`tge-extension.manifest`

Required v1 fields include:

- `manifestVersion=1`
- `extensionId=...`
- `enabled=true|false`
- `loadOrder=<int>`
- `assembly=<local dll filename>`

## Safety rules

- assembly is a local DLL filename, not a path;
- duplicate required keys are invalid;
- duplicate enabled extension IDs are rejected;
- unsupported manifest versions are rejected;
- no recursive arbitrary child-directory discovery.

A valid manifest means **discoverable package**, not “gameplay-safe extension”.
