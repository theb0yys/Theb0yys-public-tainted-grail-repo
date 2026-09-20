# Connect to Tainted Grail Extender Safely

Use this guide when you genuinely need an **external local SDK client** talking to a running FoA process through Tainted Grail Extender (TGE).

For an ordinary in-process BepInEx mod, prefer the direct plugin/framework routes first. TGE is an advanced boundary for external tooling, reviewed services and local development clients.

Canonical platform docs:

- [Tainted Grail Extender](../../../platform/components/tainted-grail-extender/README.md)
- [Local SDK Transport](../../../platform/components/tainted-grail-extender/sdk.md)

Working lineage: [Authenticated TGE Live Handshake](../../../research/case-studies/frameworks/tge-live-handshake.md).

## What you will build

A local client that performs:

```text
read-only preflight
→ launch/connect to exact local FoA/TGE host
→ authenticate host + API + session
→ discover services
→ issue one bounded read-only request
→ exercise negative controls
→ stop cleanly
```

The working proof used the **read-only identity service**. Do not generalise that proof into permission to call every service.

## Security boundary

The supported transport is:

- IPv4 loopback only;
- authenticated;
- tied to exact host/session identity;
- intended for local tooling.

Do **not**:

- expose it as a remote network service;
- commit the SDK secret;
- print the secret to logs;
- persist the secret into receipts/config;
- reuse stale session credentials.

Treat the key as ephemeral process/session state.

## Step 1 — prove you need TGE

Use TGE only if the task requires an external process or TGE-hosted service.

If your code already runs inside BepInEx and can consume the published plugin/framework contract directly, use that instead.

## Step 2 — perform read-only preflight

Before connecting, identify:

- exact game installation/profile;
- expected TGE host identity;
- expected version/API level;
- intended service;
- whether your client is read-only or mutating.

Do not launch an execution workflow against an unknown installation.

## Step 3 — create a fresh in-memory key/session

The reviewed development flow uses a fresh SDK key for the launched session.

Keep it:

- in memory;
- scoped to the local session;
- out of source;
- out of logs;
- out of durable config.

## Step 4 — authenticate exact host/session identity

After connection, verify the host response matches what you expected:

```text
host identity
+ host version
+ API version
+ current session
+ authenticated transport
```

A socket connection alone is not success.

If identity/session mismatches, stop.

## Step 5 — discover services before calling them

Ask the host which reviewed services are available.

Do not assume a service exists merely because your client knows its name.

For the first proof, keep the request read-only.

## Step 6 — issue one bounded request

Use one service-specific request whose contract you understand.

The accepted handshake proof used the read-only identity service.

Record:

- request identity;
- service selected;
- response success/failure;
- session identity;
- no secret material.

## Step 7 — test negative controls

The working proof deliberately checked:

- identical-request replay handling;
- stale-session rejection;
- unknown-service rejection;
- wrong-key rejection;
- healthy request after negative tests.

These matter because a transport that accepts every request is not a useful security boundary.

## Step 8 — verify process ownership

The live proof also checked listener PID ownership.

Confirm you are connected to the intended FoA/TGE process rather than an unrelated local listener.

## Step 9 — shut down cleanly

Stopping the client should not:

- kill an unrelated FoA process;
- leave temporary proof files behind;
- persist secrets;
- leave a stale client session pretending to be current.

Remove temporary validation artifacts you own.

## Verification checklist

A bounded transport pass should show:

1. exact host/version/API/session matched;
2. authentication succeeded;
3. service discovery succeeded;
4. one read-only service call succeeded;
5. replay handling behaved as expected;
6. stale session was rejected;
7. unknown service was rejected;
8. wrong key was rejected;
9. a valid request still succeeded after negative controls;
10. listener PID matched the intended process;
11. cleanup removed temporary proof material.

## Common mistakes

### Treating connectivity as gameplay proof

A successful handshake proves transport/session/authentication. It does not prove a gameplay provider or mutation service is correct.

### Logging the key

Never put the authentication secret in normal diagnostics.

### Skipping service discovery

A client should not hard-assume arbitrary service availability.

### Reusing stale sessions

Session identity is part of the trust boundary.

## Evidence boundary

**Proven:** authenticated local-loopback transport, expected host/session identity, service discovery, replay/stale-session/unknown-service/wrong-key controls, listener ownership and a healthy read-only identity query after negative tests.

**Not claimed:** general SDK readiness, arbitrary service safety, gameplay provider correctness or remote-network use.
