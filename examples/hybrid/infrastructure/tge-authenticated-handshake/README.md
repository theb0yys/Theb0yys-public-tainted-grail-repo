# TGE Authenticated Handshake

A bounded client example for an **already running** Tainted Grail Extender host.

It demonstrates:

~~~text
TGE_SDK_KEY from process environment
→ exact host identity/version handshake
→ service discovery
→ same-request replay
→ unknown-service rejection
→ wrong-key rejection
→ final healthy discovery
~~~

It does not install TGE, write game configuration, launch FoA, or invoke gameplay services.

## Requirements

- a reviewed TGE host already running on IPv4 loopback;
- its actual listener port;
- the matching 32-byte session key in TGE_SDK_KEY;
- the FOA-SDK tools directory containing tge_sdk_transport.py and tge_sdk_client.py.

## Run

~~~powershell
$env:TGE_SDK_KEY = "<64 hex characters supplied to the running host>"
python .\probe.py --sdk-tools "C:\Path\To\FOA-SDK\Gems\TaintedGrailModdingSDK\Tools" --port 12345
~~~

The script never prints the key.

## Boundary

The accepted live proof additionally exercised stale-session rejection and listener-PID ownership in its operational harness. This small example stays on the public client API and does not reproduce process/deployment ownership checks.

Guide: [Connect to Tainted Grail Extender safely](../../../../guides/tasks/interoperability/connect-to-tge-safely.md)  
Evidence: [Authenticated TGE live handshake](../../../../research/case-studies/frameworks/tge-live-handshake.md)
