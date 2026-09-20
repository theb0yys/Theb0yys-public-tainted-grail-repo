# Tainted Grail Extender

**Posture: Advanced/SDK**

**Distribution:** **NOT-PUBLISHED-STANDALONE** for the TGE runtime host as of 2026-09-20.

**FOA-SDK source:** https://github.com/theb0yys/FOA-SDK — public pre-alpha source; its repository explicitly does not claim a supported public release.

**Stability boundary:** documented extension manifests and authenticated loopback service contracts are the advanced contract shape. They do not create a generic remote/admin API or an author-ready standalone host distribution.

See [distribution/versioning](../ecosystem/distribution-and-versioning.md) and [API stability](../ecosystem/api-stability.md).

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
