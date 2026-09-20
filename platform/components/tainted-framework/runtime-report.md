# Runtime Report

`framework.runtime-report` is a read-only diagnostics surface.

It can communicate framework/runtime identity, compatibility/readiness and blocked-capability information without granting mutation authority.

The Tainted Diagnostic Tool is the reference diagnostics consumer pattern.

## Important boundary

A runtime report does not mean:

- runtime mutation is approved;
- game API access is approved;
- a Harmony broker is available;
- all framework services are consumer-ready.

Treat each capability independently.
