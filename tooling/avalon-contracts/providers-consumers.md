# Providers and Consumers

## Provider

A provider owns one domain's contract truth and explicitly hands a provider instance to the shared host/API.

Registration is explicit; the host does not scan the game or assemblies to invent providers.

## Consumer

A consumer may query read-only:

- discovery;
- catalog;
- state;
- evidence;
- preview;
- validation.

A consumer must not treat provider presence as permission to call provider internals directly.

## Fail closed

Unknown/missing provider:

`no provider → no feature integration`

not:

`no provider → reflect into whatever looks similar`

Lifecycle command routes, provider callbacks and mutation are separate promoted lanes and should be treated according to their exact current evidence/readiness state.
