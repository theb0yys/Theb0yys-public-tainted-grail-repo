# Tainted Framework

**Posture: Capability-gated**

Tainted Framework is the shared **runtime-facing service layer**.

Use it only when the exact service you need is explicitly documented as consumer-ready.

## Current author rule

Do not treat internal framework assemblies as a general gameplay SDK.

The public capability catalog includes many candidate/blocked surfaces. The historically promoted diagnostics surface is `runtime-report`; other services must be checked individually.

## Relationship to Avalon Core

- Avalon Core: discovery/evidence/contracts/planning authority.
- Tainted Framework: concrete runtime-facing reusable service implementations where promoted.

A feature mod should not depend on both simply because both exist. Choose the owner that actually owns the capability.

See:

- [Runtime report](runtime-report.md)
- [Native item registrar ownership](native-item-registrar.md)
