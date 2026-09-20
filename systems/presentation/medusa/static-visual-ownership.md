# Medusa Static Visual Ownership

Use this page when a large static environment visual cannot be manipulated through an ordinary runtime `MeshRenderer`.

Canonical overview: [Medusa](README.md).

## Visuals may be compiled away from authored objects

The build path can convert authored static renderer data into compact Medusa scene payloads.

At runtime:

```text
authoring MeshRenderer/LODGroup
→ baked Medusa records
→ mounted scene payload
→ compact CPU/GPU state
→ BatchRendererGroup
```

The authored renderer hierarchy is not necessarily the runtime owner.

## Colliders can remain separate

Gameplay collision may remain represented by ordinary Unity scene objects while the visual side is owned by Medusa.

Therefore:

```text
raycast/collider hit
≠
visual renderer owner
```

A replacement workflow must prove the bridge from the hit/subject to the Medusa visual record rather than assuming the collider's GameObject owns the visible material.

## Safe investigation order

1. identify scene/static subject;
2. find collision/gameplay owner if relevant;
3. identify Medusa visual record/owner;
4. resolve material/mesh identity;
5. test visibility/LOD/culling;
6. apply only the presentation change;
7. verify scene unload/reload.

Do not attempt to move a Medusa static visual as though it were a normal movable prefab.
