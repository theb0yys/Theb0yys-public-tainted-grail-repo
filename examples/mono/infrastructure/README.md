# Mono Infrastructure Examples
- [Performance telemetry](performance-telemetry/README.md)
- [Native save completion observer](save-completion-observer/README.md)
- [Save completion observer + backup](save-observer-backup/README.md)
- [Smart save backup](smart-save-backup/README.md)

These examples show how a normal Mono/BepInEx mod can consume the shared infrastructure documented under [tooling](../../../platform/README.md).

- [FoA Mod Manager baseline](mod-manager-baseline/README.md)
- [Shared custom UI: Mod Manager + Tainted Interface](shared-ui/README.md)
- [Avalon Core read-only consumer](core-readonly/README.md)
- [Avalon AI V2 package contracts](ai-package-contracts/README.md)
- [Optional cross-mod API](optional-cross-mod-api/README.md)

The examples do **not** redistribute infrastructure DLLs. Point the MSBuild properties at your installed/released infrastructure package.

These are integration examples, not proof that every infrastructure capability is runtime-ready.
