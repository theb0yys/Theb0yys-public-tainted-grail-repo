# Mipmap Demand and Material Registration

Use this page when a material/texture appears correct but streams at the wrong detail level or behaves differently under proprietary renderers.

Canonical overview: [Shared Mipmap Streaming](README.md).

## Demand is indirect

The shared path is:

```text
renderer-specific provider
→ MaterialId demand
→ material-to-texture expansion
→ TextureId demand
→ Texture2D.requestedMipmapLevel
→ Unity residency/streaming
```

The renderer does not necessarily set texture mip levels directly.

## Registration is not residency

A material being known to the shared registry proves only that the material/texture relationship can participate in demand calculation.

It does not prove:

- the material is currently visible;
- the renderer requested high detail;
- the texture is resident at the requested mip;
- a camera/provider contributed demand this frame.

## Modding boundary

When swapping or introducing a material on a proprietary renderer:

1. identify the owning renderer;
2. determine whether it participates in the shared mipmap service;
3. preserve the material/texture registration path used by that renderer;
4. avoid forcing global texture settings as a substitute for missing provider integration;
5. verify distance/camera transitions rather than checking only one close-up frame.

## Failure patterns

- material looks correct nearby but blurs incorrectly at distance;
- replacement texture never receives expected demand;
- one renderer works because it uses Unity's normal path while another depends on Questline demand;
- a mod assumes `requestedMipmapLevel` is a permanent state rather than a per-frame/resulting demand.

For renderer-specific ownership see [Presentation Systems](../README.md).
