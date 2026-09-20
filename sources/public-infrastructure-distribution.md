# Public Infrastructure Distribution Sources

Checked: **2026-09-20**

This ledger supports the public acquisition claims in [Tooling and Shared Infrastructure](../tooling/README.md).

A public download page proves that an acquisition route exists. It does not by itself prove runtime compatibility, API stability, persistence safety, or that every statement on an older source-side manifest still describes the current published package.

## FoA Mod Manager

Nexus Mods:

https://www.nexusmods.com/taintedgrailthefallofavalon/mods/167

Checked claims:

- public acquisition route exists;
- the public page documents Mono and IL2CPP support and runtime-specific Main files;
- the public page documents mod-author integration for custom UI scope, controller actions and read-only status providers;
- the current private engineering release manifest records source-side version `0.6.62` and plugin GUID `kane.tgfoa.mod-manager`.

The source-side version does not prove which public Main file a user downloaded. Inspect the exact public package before declaring a minimum package version.

## Tainted Interface

Nexus Mods:

https://www.nexusmods.com/taintedgrailthefallofavalon/mods/190

Checked claims:

- public acquisition route exists;
- plugin GUID is `kane.tgfoa.tainted-interface`;
- public plugin version declaration is `0.3.4`;
- the public page documents Mono and IL2CPP Main files;
- the author-facing surface documents shared styles/resources and semantic lookup;
- the page does not advertise arbitrary Main/Pause Menu registration as a current public consumer API.

## Tainted Core / Avalon Core

Nexus Mods:

https://www.nexusmods.com/taintedgrailthefallofavalon/mods/193

Checked claims:

- public acquisition route exists;
- public name is Tainted Core while technical package identity remains Avalon Core;
- plugin GUID is `kane.tgfoa.avalon-core`;
- current public source/plugin version is `0.8.4`;
- the public page documents Mono and IL2CPP runtime support and requires the Main file matching the runtime;
- the package is intended to be installed as a complete runtime package rather than selected individual DLLs.

## Tainted Framework

Nexus Mods:

https://www.nexusmods.com/taintedgrailthefallofavalon/mods/312

Checked claims:

- public acquisition route exists;
- plugin GUID is `kane.tgfoa.tainted-framework`;
- public description explicitly states support for BepInEx 5 Mono and BepInEx 6 IL2CPP through one shared framework boundary;
- common assemblies include `Tainted.Abstractions`, `Tainted.Contracts`, and `Tainted.Core`;
- current engineering evidence includes the IL2CPP host/package line through `0.1.38`; this is separate from the Nexus display/file version.

Tainted Framework is therefore **not IL2CPP-blocked**. Runtime support is public; individual framework capabilities still require their own promotion/consumer evidence. The Nexus page exposes a site/file version separately from source-side semantic versions, so this repository does not manufacture an API-semver guarantee from the Nexus display version.

## Avalon AI FoA Host

Nexus Mods:

https://www.nexusmods.com/taintedgrailthefallofavalon/mods/220

Checked claims:

- public acquisition route exists;
- plugin GUID is `kane.tgfoa.avalon-ai-foa-host`;
- current Host version stated in the public description is `0.8.2`;
- the public page advertises the Host for Mono and IL2CPP and lists Tainted Framework for the IL2CPP lane;
- packages are expected to use the shared Host rather than ship separate competing schedulers;
- matching Host/framework/package DLLs should be kept together.

The public page's top-level support statement is cross-runtime, while detailed package examples remain more Mono-oriented. Treat Mono + IL2CPP as the public support posture, but take exact loader/package requirements from the selected runtime-specific Main file.

## Tainted Diagnostic Tool

Nexus Mods:

https://www.nexusmods.com/taintedgrailthefallofavalon/mods/182

Checked claims:

- public acquisition route exists;
- the public page documents Mono and IL2CPP Main files;
- the tool is presented as read-only runtime evidence collection;
- public documentation separates observed diagnostic evidence from gameplay approval.

The private engineering source currently carries a newer source-side plugin line than some public description text. The distribution guide therefore requires checking the downloaded package before declaring a minimum version.

## FOA-SDK

GitHub:

https://github.com/theb0yys/FOA-SDK

Checked claims from the public repository README:

- source repository is public;
- status is pre-alpha;
- it is not a finished SDK;
- it explicitly does not provide a supported public release;
- generated installer/release artifacts are governed output, not implied by the existence of source.

GitHub Releases was empty when checked on 2026-09-20.

## Avalon Contracts

No verified independent public package/download page was found during this check.

The public knowledge base may document its provider/consumer contract shape, but that does not make it an independently obtainable author dependency.

Status: **NOT-PUBLISHED-STANDALONE**.

## Tainted Grail Extender host

No verified independent public host package/download page was found during this check.

FOA-SDK is public source, but its repository explicitly states that it has no supported public release. That is not a substitute for a published TGE runtime-host package.

Status: **NOT-PUBLISHED-STANDALONE**.

## Source-side corroboration

Private engineering release manifests were inspected only to avoid overstating public package/version claims. Where source-side and public distribution descriptions differ, the public guide records the distinction rather than silently treating one lane as proof of the other.

Relevant source-side version signals observed during this check include:

- FoA Mod Manager `0.6.62`;
- Avalon Core `0.8.4`;
- Tainted Framework `0.1.33` in its Mono-side scaffold/release manifest;
- Avalon AI Runtime/FoA Host `0.8.2`;
- Tainted Diagnostic Tool `0.4.56`.

Those source-side values do **not** automatically prove the exact contents/version of a currently downloadable public archive.