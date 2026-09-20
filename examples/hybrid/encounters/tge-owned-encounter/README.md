# TGE Owned Encounter

A minimal external-client example for the proven TGE owned encounter service.

## Lifecycle

~~~text
authenticated existing session
→ preview one allowlisted template
→ receive planId + fingerprint
→ send exact spawn request once
→ poll owned encounterId
→ wait until exact actor handle is ready
→ request removal by encounterId
→ wait until the same handle is removed
~~~

## Requirements

The reviewed TGE host must already be running with SdkEncounters.Enabled=true. The example does not install, configure, or launch it.

## Run

~~~powershell
$env:TGE_SDK_KEY = "<matching 64-hex session key>"
python .\owned_encounter.py --sdk-tools "C:\Path\To\FOA-SDK\Gems\TaintedGrailModdingSDK\Tools" --port 12345 --template wyrdspirit
~~~

Use a disposable test save and suitable open ground.

## Important retry rule

Once the spawn request is dispatched, a lost response has an **unknown outcome**. Do not construct a second spawn request. Reconcile the exact original invocation instead.

## Boundary

This example demonstrates one allowlisted actor. The accepted production proof also promoted a two-Wyrdspirit composition after the single-actor lifecycle succeeded.

Guide: [Build an owned native encounter](../../../../guides/tasks/world/build-an-owned-native-encounter.md)  
Evidence: [TGE owned encounter lifecycle](../../../../research/case-studies/frameworks/tge-owned-encounter.md)
