# 04 — Framework

A framework should make multiple mods or features easier to build and maintain.

Do not create a framework because one plug-in has several classes. Create one when you have repeated responsibilities that deserve stable contracts.

## Good framework responsibilities

Examples:

- logging abstraction;
- configuration ownership;
- feature registration;
- event routing;
- compatibility/capability checks;
- shared diagnostics;
- lifecycle management;
- stable public contracts.

## Poor framework responsibilities

Avoid a framework that:

- knows every feature's internal state;
- silently changes unrelated game state;
- requires every mod to depend on it without a clear benefit;
- exposes private implementation details as its public API;
- performs uncontrolled scanning or mutation during discovery.

## Services and contracts

A useful service owns one focused capability.

Example concepts:

- configuration service;
- logging service;
- game-version service;
- feature registry;
- compatibility service.

The names matter less than the boundary.

A feature should depend on a small contract, not the entire framework implementation.

## Avoid global mutable state

Global mutable state is easy at first and expensive later.

Prefer explicit ownership and lifecycle over static objects that every feature can change.

## Compatibility layer

Centralize version/runtime differences.

Preferred shape:

feature → compatibility contract → runtime/version-specific adapter

A feature should ask "is capability X available?" rather than duplicating exact version/type checks everywhere.

An adapter may own:

- target type discovery;
- version-specific method binding;
- Mono versus IL2CPP differences;
- safe capability detection.

This makes game updates easier to audit and repair.

## Public framework rule

If other modders are expected to depend on your framework, document:

- API stability expectations;
- versioning policy;
- compatibility policy;
- initialization order;
- failure behaviour;
- how extensions register/unregister.

A framework is infrastructure for other people's code, so ambiguity becomes somebody else's bug.
