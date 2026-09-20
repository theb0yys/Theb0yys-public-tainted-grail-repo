# Asset Transport, Registration, and Presentation

Use this page when an asset loads successfully but the gameplay feature still does not appear.

Canonical overview: [Assets, Addressables, and Presentation](README.md).

## Three separate problems

```text
asset transport
→ gameplay registration/identity
→ presentation binding
```

They have different owners.

### Asset transport

Answers:

- can the bundle/catalogue/address be found?
- can the mesh/prefab/icon/material be loaded?
- are dependencies/shaders/scripts present?

### Gameplay registration

Answers:

- does the native gameplay provider/registry know the custom identity?
- can normal game lookup resolve it?
- is the definition available at the required lifecycle point?

A loaded prefab alone does not solve this.

### Presentation binding

Answers:

- which native renderer/equip/UI owner consumes the asset?
- is the asset bound through that owner?
- is ownership released/restored correctly?

A registered item with no presentation binding may exist logically but render incorrectly.

## Debug order

When “the custom asset does not work”:

1. prove transport;
2. prove gameplay identity/registration separately;
3. prove the definition points to the intended presentation identity;
4. prove the native presentation owner consumes it;
5. prove cleanup/unload.

Do not compensate for failed registration by directly spawning a presentation object.

## Example boundaries

- custom item identity → [Templates and Registries](../../systems/core/templates-registries.md)
- rigid mesh presentation → [Drake](../../systems/presentation/drake/README.md)
- clothing/skinned presentation → [Kandra](../../systems/presentation/kandra/README.md)
- Addressables transport → [Addressables](../addressables/README.md)
- external asset rights → [Modder Resources](../modder-resources/README.md)

## Proof vocabulary

Report separately:

- asset resolves;
- definition registers;
- provider lookup succeeds;
- presentation binds;
- runtime behavior works;
- persistence works;
- release package works.

Those statements are not interchangeable.
