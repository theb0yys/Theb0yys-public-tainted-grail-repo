# Recipe: External Local SDK Tool

Use TGE when the client runs **outside** the BepInEx process and needs a controlled local bridge.

Public SDK repository:

<https://github.com/theb0yys/FOA-SDK>

## Flow

```text
read-only preflight
→ verify exact local install/profile
→ start a reviewed SDK development session
→ inject a fresh in-memory SDK key into the owned game process
→ authenticate loopback transport
→ verify host + session identity
→ discover service
→ issue service-specific bounded request
→ validate response
→ disconnect / clean up only owned session resources
```

The reviewed development-session tooling supports bounded read-only commands such as service discovery, player vitals and position where the corresponding service is present.

## Security

- loopback only;
- authenticated transport;
- never print/store the shared key;
- do not expose the listener remotely;
- authentication does not make every service safe;
- unknown-outcome mutation requests must not be blindly retried.

For an ordinary in-process BepInEx mod, prefer direct shared infrastructure instead of TGE.
