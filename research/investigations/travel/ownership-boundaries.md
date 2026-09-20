# Travel Ownership Boundaries

A travel feature sits between several existing owners.

| Concern | Owner |
| --- | --- |
| canonical road/location truth | Avalon Core Road Atlas / reviewed game knowledge |
| route traffic/population execution | Living Avalon |
| fatigue/hardship effects | Tainted Survival |
| money/supplies/economy | Tainted Economy |
| mount mechanics | Avalon Mounts |
| Wyrd threat | Wyrd Hunt |
| crime/guard/legal pressure | Crime & Consequences |
| settings/warnings | FoA Mod Manager |
| shared presentation | Tainted Interface |
| evidence capture | diagnostics |

A future travel layer may own **travel policy/preview** while consuming those truths.

It should not quietly duplicate them.

## Preview is safer than execution

A useful early product can calculate/display:

- candidate route;
- estimated travel time;
- estimated cost;
- estimated hardship/danger;

without moving the player or mutating any adjacent system.

Each actual effect should later be delegated to the owner that already owns that domain.
