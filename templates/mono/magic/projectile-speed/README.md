# Mono Magic Projectile Speed

Use this starter when you want to change the speed of player-owned magic projectiles without taking over spell targeting or damage.

The default multiplier is `1.0`, so the starter makes no gameplay change until you configure it.

Build:

```powershell
dotnet build .\MagicProjectileSpeed.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

When adapting it, keep the ownership checks narrow so unrelated projectiles remain unchanged.
