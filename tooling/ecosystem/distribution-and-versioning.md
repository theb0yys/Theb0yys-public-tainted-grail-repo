# Public Distribution and Versioning

Checked: **2026-09-20**

This page answers two separate questions:

1. **Where can an author obtain this infrastructure?**
2. **Which version number should a consumer use when declaring compatibility?**

Do not collapse these into one number. A Nexus page/file version, BepInEx plugin version, assembly version, package version and API-contract version can all differ.

## Distribution states

- **PUBLIC** — a verified public acquisition route exists.
- **SOURCE-ONLY** — public source exists, but no supported public binary/package release is currently claimed.
- **NOT-PUBLISHED-STANDALONE** — the component is documented, but no verified independent public acquisition route was found.
- **GATED** — an implementation may exist, but authors should not build a new dependency on it until a public distribution and contract are promoted.

## Current acquisition map

| Component | Distribution state | Verified acquisition route | Verified version signal | Consumer note |
| --- | --- | --- | --- | --- |
| FoA Mod Manager | **PUBLIC** | Nexus Mods: https://www.nexusmods.com/taintedgrailthefallofavalon/mods/167 | current engineering source line is `0.6.62`; verify the exact downloaded Main file before setting a minimum dependency | Use plugin GUID `kane.tgfoa.mod-manager`; ordinary BepInEx config discovery does not require a compile-time API reference |
| Tainted Interface | **PUBLIC** | Nexus Mods: https://www.nexusmods.com/taintedgrailthefallofavalon/mods/190 | public plugin declaration `0.3.4` | Depend on semantic/public API, not embedded-pack paths |
| Tainted Core / Avalon Core | **PUBLIC** | Nexus Mods: https://www.nexusmods.com/taintedgrailthefallofavalon/mods/193 | public source/plugin version `0.8.4` | Install the complete runtime package; do not copy individual Core DLLs into feature releases |
| Tainted Framework | **PUBLIC** | Nexus Mods: https://www.nexusmods.com/taintedgrailthefallofavalon/mods/312 | no independent API-semver guarantee is asserted by this repository | The public package exists; only named promoted consumer surfaces are author contracts |
| Avalon AI FoA Host | **PUBLIC** | Nexus Mods: https://www.nexusmods.com/taintedgrailthefallofavalon/mods/220 | public Host version `0.8.2` | AI packages reference the versioned Contracts surface, not the host implementation |
| Avalon Contracts | **NOT-PUBLISHED-STANDALONE** | no verified standalone public package route recorded here | not published as a standalone author dependency | Do not publish a third-party hard dependency until an acquisition route and compatible contract version are published |
| Tainted Grail Extender host | **NOT-PUBLISHED-STANDALONE** | no verified standalone public host package route recorded here | not published as a standalone author dependency | Treat TGE host integration as advanced/gated until its host distribution is public |
| FOA-SDK | **SOURCE-ONLY** | GitHub: https://github.com/theb0yys/FOA-SDK | Developer Preview source state; repository explicitly says no supported public release | Clone/build only when following the FOA-SDK repository's own governed process |
| Tainted Diagnostic Tool | **PUBLIC** | Nexus Mods: https://www.nexusmods.com/taintedgrailthefallofavalon/mods/182 | current source-side plugin line is `0.4.56`; verify the downloaded package before declaring a minimum dependency | This is primarily an installed research tool, not a library that feature mods should reference |

## Runtime-package rule

When a Nexus page offers runtime-specific Main files:

1. use the Main file for the runtime actually installed;
2. follow the requirements attached to that exact file;
3. do not infer the Mono package layout from the IL2CPP file or vice versa;
4. record the installed plugin/package version in support reports;
5. never combine individual DLLs from different package versions.

Public descriptions for several components have evolved faster than older source-side release manifests. The **selected current public package** controls installation; source-side manifests remain useful evidence for the implementation lane they actually describe, but they do not override a newer published runtime package.

## Version vocabulary

### Package version

Version of the distributed component as a whole.

Use it for release notes, support reports, installation/update decisions and exact package compatibility.

### BepInEx plugin version

Version declared by the installed plugin.

Use it for BepInEx dependency checks only when the component actually maintains that declaration as the compatibility boundary.

### Assembly version / file version

Build metadata for a particular DLL.

Do not use it as the ecosystem compatibility contract unless the component explicitly says to.

### API/contract version

Version of a public consumer contract.

This is the strongest dependency boundary when one exists. Example: `AvalonAI.Contracts.V2`.

A package may update without breaking a versioned contract. Conversely, two assemblies loading successfully does not prove their contracts are compatible.

## Minimum-version rule for mod authors

Only declare a minimum version when you can identify **why** that version is required.

Good:

```text
requires FoA Mod Manager >= X
because RegisterStatusProvider(...) entered the supported public surface in X
```

Bad:

```text
requires latest
```

If the first supported version of a surface is not documented:

- record the exact package version you tested;
- do not invent an older minimum;
- fail closed on missing API members;
- update the compatibility statement after a reviewed lower-bound test.

## Do not vendor the ecosystem

The acquisition route belongs to the infrastructure owner.

A feature mod should normally ship its own feature assembly plus its own README/changelog/licence material, not duplicate copies of shared infrastructure DLLs.

See [Dependency and packaging](dependency-and-packaging.md).

## Distribution blocker rule

A documented API without a public acquisition route is **not author-ready distribution**.

For Avalon Contracts and the TGE host, this repository can document the contract shape, but third-party authors should not be told to take a new hard dependency until a verified standalone distribution route exists.

See [API stability and capability promotion](api-stability.md) for what is safe to call after installation.