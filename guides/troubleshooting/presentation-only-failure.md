---
document_type: troubleshooting
scope: gameplay works but only presentation is incorrect
last_verified: 2026-09-20
---

# Presentation-Only Failure

A useful failure classification is:

```text
identity works
+ acquisition works
+ gameplay/combat works
+ presentation fails
= presentation lane investigation
```

Do not reopen every upstream system.

Check the presentation owner, asset key resolution, renderer lifetime, visibility/culling, FPP/TPP route, preview-vs-world representation and cleanup.

The Evil Greatsword work is a representative example: runtime weapon/equip routing and Drake key serving could be proven independently from full visual/lifecycle acceptance.
