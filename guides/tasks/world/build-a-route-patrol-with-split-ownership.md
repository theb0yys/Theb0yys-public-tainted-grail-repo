# Build a Route Patrol with Split Ownership

**Evidence status: PARTIAL.** The ownership split is established; the case does not itself contain a complete end-to-end runtime patrol proof.

Working lineage: [Wyrd Route Patrol: Three Owners, One Feature](../../../research/case-studies/encounters/wyrd-route-patrol-ownership.md).

## Ownership model

Do not make one mod own every concern.

The researched design separates:

- Wyrd Hunt — whether strict exposure is confirmed;
- Avalon Core — which exact route is reviewed/authorized;
- Living Avalon — which actor is owned, where it is placed, how it is bound and cleaned up;
- native FoA AI — detection/combat.

## Process

```text
read strict exposure
→ resolve one authorized route
→ spawn/own one reviewed actor
→ bind actor to route policy
→ native AI owns detection/combat
→ owner cleans up exact actor
```

Keep cross-system dependencies one-way/read-only where possible.

## Verification

Prove separately:

- exposure source;
- route identity/authorization;
- actor identity;
- placement;
- route following;
- native combat handoff;
- cleanup;
- scene transition behaviour.

## Current proof boundary

The responsibility split is the established lesson. The full patrol runtime lifecycle still needs explicit validation before a generic route-patrol implementation is called proven.
