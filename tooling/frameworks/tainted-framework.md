---
document_type: framework
scope: Tainted Framework relationship to Avalon Core and consumers
runtime: mono
evidence:
  source: ACCEPTED_PROJECT_DECISIONS
last_verified: 2026-09-20
---

# Tainted Framework Boundary

Tainted Framework and Avalon Core are complementary project layers rather than two competing global owners.

## Separation

- **Avalon Core** is strongest at shared contracts, evidence/discovery, catalogs and cross-project coordination.
- **Tainted Framework** owns runtime-facing reusable mod services/importer/framework implementation where a concrete shared runtime capability is actually proven.
- **Feature mods** own their own gameplay state and consume narrow services/contracts.
- **FoA Mod Manager** owns settings/custom-UI/controller/status integration, not gameplay frameworks.

A shared runtime framework should not absorb a capability merely because several mods use similar code. It should do so only when the ownership contract, lifecycle, compatibility and validation are mature enough to justify one common owner.

See [Native item registrar ownership](native-item-registrar.md) for a concrete contract lane.
