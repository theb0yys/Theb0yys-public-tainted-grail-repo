# Ownership and Content Terms

## Ownership

**Native owner** — the FoA system that actually owns a responsibility, state or lifecycle.

**Presentation owner** — the system that owns the visible/audio/rendered representation. It may differ from the gameplay/model owner.

**Shared owner** — a reviewed mod-infrastructure component that centralizes a cross-mod responsibility, such as shared UI scope or a single AI host.

**Consumer** — code that uses a public contract/service owned elsewhere.

**Provider** — code that exposes domain state/capability through a supported contract while retaining ownership of its domain truth.

## FoA model terms

**Model** — FoA logical game object registered to `World`.

**Element** — functionality/state whose lifecycle is owned by a parent Model.

**View** — Unity-facing presentation associated with a FoA Model.

**Service** — shared runtime owner resolved by type/key; lifetime may be application/domain/scene scoped.

**Template** — reusable FoA definition resolved through native template registries/providers.

## Content and asset terms

**Addressables key/address** — lookup identity used by Unity Addressables. Resolving it proves asset lookup, not gameplay registration.

**Catalogue / locator** — Addressables discovery data that makes locations available to later lookups.

**AssetBundle** — Unity asset transport package. Successful bundle loading does not register gameplay content automatically.

**Native identity** — game-owned GUID/reference/type identity used by FoA systems.

**Mod-owned identity** — stable identifier created and owned by a mod/project. Do not confuse it with a native game GUID.

**Semantic asset ID** — stable public identifier exposed by an infrastructure owner instead of requiring consumers to hardcode physical asset paths.

## Dependency terms

**Hard dependency** — the advertised feature cannot function correctly without the provider.

**Optional dependency** — enhancement-only integration; the base feature remains correct when the provider is absent.

**Public contract** — documented consumer-facing API/DTO/capability surface.

**Promoted capability** — a specific framework service that has been explicitly made available to consumers; internal neighboring services remain gated.

See [API Stability and Capability Promotion](../../tooling/ecosystem/api-stability.md).
