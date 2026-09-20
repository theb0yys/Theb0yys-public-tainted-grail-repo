# Merchant Stock Tweaks Public Template

Use this starter for merchant-stock or restock changes that need cross-runtime support.

Keep filtering/configuration rules shared, while runtime hosts connect them to the exact Shop, stock, and UI lifecycle used on Mono or IL2CPP.

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
