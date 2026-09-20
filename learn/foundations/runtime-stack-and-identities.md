# Runtime Stack, Identity, and Ownership

This is the minimum mental model to carry into every FoA mod.

## Runtime stack

Think from the bottom upward:

```text
game build
→ Unity/runtime lane
→ BepInEx loader
→ Harmony / interop support
→ shared infrastructure
→ your plug-in
→ feature behavior
```

When a feature fails, prove each lower layer before debugging the next one.

## Mono vs IL2CPP

Treat them as separate runtime lanes.

Differences can include:

- loader package/API;
- target framework;
- generated interop assemblies;
- base plugin type;
- reflection/patch signatures;
- Unity type representation;
- runtime-specific packaging.

Shared infrastructure such as Tainted Framework can provide a common contract boundary, but your feature still needs runtime-specific validation.

## Identity types

Do not collapse these:

- BepInEx plugin GUID;
- native FoA template GUID;
- mod-owned template/content GUID;
- Unity asset GUID;
- Addressables key/address;
- semantic infrastructure asset ID;
- display name.

A display name is usually not a durable identity.

## Ownership question

For every feature, ask:

1. what exact state am I changing?
2. who owns it?
3. when is that owner ready?
4. what consumes the result?
5. who cleans it up?
6. does it persist?

If those questions are unknown, investigate before patching.

## Useful routes

- [Finding the native owner](../../investigate/finding-the-native-owner.md)
- [Intervention Selection](../../mechanics/intervention-selection/README.md)
- [Runtime compatibility](../../tooling/ecosystem/runtime-compatibility.md)
- [Identity reference](../../reference/identities/README.md)
