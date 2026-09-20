# Temporary Native Actor

The smallest public example of **creation plus cleanup**, using the reviewed TGE encounter owner.

It creates one allowlisted Wyrdspirit, waits until the native actor and visual are observed ready by the service, keeps the opaque owned handle, then removes exactly that owned actor and waits for confirmed cleanup.

## Run

~~~powershell
$env:TGE_SDK_KEY = "<matching 64-hex session key>"
python .\temporary_actor.py --sdk-tools "C:\Path\To\FOA-SDK\Gems\TaintedGrailModdingSDK\Tools" --port 12345
~~~

The TGE host must already be installed, enabled, authenticated and running.

## Why this is separate from the owned-encounter example

This file is intentionally single-purpose:

~~~text
create one
→ observe one
→ remove the same one
~~~

It is a lifecycle/cleanup teaching example, not a composition or encounter-director example.

Guide: [Spawn and clean up a temporary native actor](../../../../guides/tasks/world/spawn-and-clean-up-a-temporary-native-actor.md)  
Evidence: [Temporary native actor](../../../../research/case-studies/encounters/temporary-native-actor.md)
