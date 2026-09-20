# Tainted Grail Extender

Use Tainted Grail Extender for advanced integrations that cannot be handled cleanly by a normal in-process BepInEx dependency.

Typical uses include TGE-hosted extensions, authenticated local SDK clients, reviewed extender services, and external development tools that need a controlled bridge into the running game.

## Main routes

- [Extension manifests](extensions.md)
- [Local SDK transport](sdk.md)
- [Owned encounter service](encounters.md)

## Security rule

The SDK transport is local-loopback and authenticated.

Do not expose it as a remote/general network API and do not persist/print its secret key.
