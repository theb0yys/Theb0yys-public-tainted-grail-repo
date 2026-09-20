---
document_type: framework
scope: authenticated local FOA-SDK ↔ TGE transport
runtime: mono
evidence:
  managed: PASSED
  installed_game_handshake: PASSED_BOUNDED
  gameplay_services: SEPARATE
last_verified: 2026-09-20
---

# TGE Local SDK Transport

TGE exposes an optional **authenticated IPv4 loopback** development transport.

It is not a remote network API and it is disabled by default.

## Security boundary

- binds only `127.0.0.1`;
- shared 32-byte key comes from process environment;
- HMAC-SHA256 authenticates request/response frames;
- server/client nonces prevent replay across connections;
- exact host identity/version/session are checked;
- keys are never transmitted/logged;
- transport is authenticated, **not encrypted**.

Possession of the key places the process inside this local trust boundary; individual providers still own operation policy.

## Execution boundary

Network IO/authentication may run on workers, but registered service execution is pumped on the active host/core thread with bounded queue/admission rules.

A timeout after dispatch begins has **unknown outcome** and must not be blindly retried with a fresh invocation ID.

## Live proof

An installed Steam Mono build 24270691 / BepInEx 5.4.23.5 probe passed:

- authenticated host identity;
- service discovery;
- same-request replay behaviour;
- stale-session rejection;
- unknown-service rejection;
- wrong-key rejection;
- recovery with a later healthy request.

That handshake proves this transport/session on the recorded build. It does not itself attest gameplay services, save safety or cross-build support.
