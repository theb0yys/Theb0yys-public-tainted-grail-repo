# Recipe: External Local SDK Tool

Use TGE only when the tool runs **outside** the BepInEx process and needs a controlled local bridge.

```text
preflight exact local install
→ start/attach through reviewed SDK session
→ authenticate loopback transport
→ verify host/session identity
→ discover service
→ issue service-specific bounded request
→ validate response
→ disconnect/cleanup owned session resources
```

Do not turn the authenticated loopback into a remote/general command server.

For a normal in-process mod, prefer direct BepInEx/shared infrastructure instead.
