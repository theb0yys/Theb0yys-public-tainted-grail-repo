# Merchant Stock Tweaks Public Template

Source family: `merchant-stock-tweaks`

This is a public, source-only starter distilled from the Merchant Stock Tweaks mod family. It keeps feature intent in shared code and loader-specific entry points in separate Mono and IL2CPP hosts.

## Reusable mechanisms

- shop-open hook
- restock cooldown
- restockable-stock filtering
- merchant ownership preservation

## Build layout

```text
shared/Feature.cs
mono/Plugin.cs
mono/MerchantStockTweaks.Mono.csproj
il2cpp/Plugin.cs
il2cpp/MerchantStockTweaks.IL2CPP.csproj
```

Rename the plug-in identity before publishing. Add game/Harmony/interop references only for verified owners. The starter uses `Tainted.Abstractions` only for the shared runtime-kind boundary and does not inherit private runtime, persistence, compatibility, or release proof.
