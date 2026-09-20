# Native-Owner-First Modding Pattern

The most reusable lesson across the working mods is simple: patch or call the system that already owns the behaviour.

## Examples

| Goal | Native owner used by working mod |
| --- | --- |
| magic projectile speed | native projectile configuration |
| vendor price | `TradeUtils.Price` return value |
| hero footsteps | `VHeroFootsteps` → FMOD one-shot call |
| mount speed | `VMount.RunningVelocity` / `TurningVelocity` |
| bonfire storage/cooking/alchemy | `FireplaceUI` service methods |
| character hit VFX | character damage lifecycle |
| terminal death VFX | `HealthElement.OnDeathEvents` |
| companion defend | `NpcHeroPetAlly.EnterCombat()` |

## Three useful shapes

### Adjust a result

Let native logic run, then modify the returned value.

Good for:

- price;
- speed;
- other scalar calculations.

### Guard an action

Check extra mod conditions, then either allow the normal native action or stop it cleanly.

Good for:

- theft authorization;
- optional command gates.

### Attach a sidecar

Observe the native lifecycle and run mod-owned presentation/telemetry.

Good for:

- damage numbers;
- blood/VFX;
- audio;
- receipts.

## What to avoid

Do not replace a whole controller because you need to change one scalar or add one effect.

Preserving native ownership gives the mod the game's existing:

- lifecycle;
- compatibility behaviour;
- persistence rules;
- events;
- cleanup.
