# Tainted Grail Extender

**Posture: Advanced/SDK**

Tainted Grail Extender is for cases where a normal BepInEx plugin dependency is not enough:

- extension packages hosted by TGE;
- authenticated local development/SDK clients;
- reviewed service calls exposed by the extender;
- external tooling that needs a controlled bridge into a running game.

## Main routes

- [Extension manifests](extensions.md)
- [Local SDK transport](sdk.md)
- [Owned encounter service](encounters.md)

## Security rule

The SDK transport is local-loopback and authenticated.

Do not expose it as a remote/general network API and do not persist/print its secret key.
