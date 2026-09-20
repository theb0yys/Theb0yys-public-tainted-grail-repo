# Local SDK Transport

TGE can expose an authenticated local development transport.

## Boundary

- IPv4 loopback only;
- authenticated request/response frames;
- exact host/session identity;
- service discovery;
- service-specific requests;
- no secret in source/config/receipts.

Transport authentication proves the client is inside the local SDK trust boundary. It does not mean every service is safe to call.

## Development workflow

The reviewed SDK development launcher follows:

```text
read-only preflight
→ verify exact install/profile
→ explicit execute approval
→ start game with fresh in-memory SDK key
→ authenticate host/session
→ discover services
→ issue bounded read-only/service-specific calls
→ stop client without force-closing unrelated game state
```

For ordinary mods running inside BepInEx, prefer the direct mod/plugin infrastructure first. Use TGE when you genuinely need an external SDK/extension boundary.
