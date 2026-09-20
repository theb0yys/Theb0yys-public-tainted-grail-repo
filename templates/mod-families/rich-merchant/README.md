# Rich Merchant Public Template

Use this starter when you are building merchant-economy changes such as wealth or stock tuning across both runtimes. Keep configuration and feature rules shared, while each host connects them to the exact merchant/shop owners.

Source family: `rich-merchant`

It keeps reusable feature logic in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- merchant gold floor
- dual-runtime build boundary
- narrow merchant patch

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/RichMerchant.Mono.csproj
il2cpp/Plugin.cs
il2cpp/RichMerchant.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
