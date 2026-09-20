# Rich Merchant Public Template

Use this starter for merchant wealth, stock, or trading changes that need cross-runtime support.

Keep configuration and feature rules shared, while each runtime host connects them to the exact native merchant/shop owners required by the implementation.

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
