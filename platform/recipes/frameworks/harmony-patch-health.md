# Recipe: Fail-Closed Harmony Feature Health

Use this pattern when a mod feature depends on one or more required Harmony targets.

## Contract

~~~text
exact target identity
→ resolve target
→ install owned patch
→ inspect live patch info
→ expected owner present?
    yes → enable feature
    no  → remove this owner's partial patch + keep feature disabled
~~~

The feature flag should default to disabled and become enabled only after every required target for that feature has passed its installation check.

## Scope failures narrowly

Prefer:

~~~text
optional vendor-price feature loses its target
→ vendor-price feature disabled
→ rest of plug-in continues
~~~

over:

~~~text
one optional target missing
→ whole plug-in continues pretending the feature works
~~~

or:

~~~text
target missing
→ patch a similar-looking replacement automatically
~~~

## Cleanup

When an installation attempt is incomplete, remove only the current Harmony owner's patches from the affected original method.

Do not globally unpatch other owners.

Normal plug-in teardown may remove the plug-in's own remaining patches.

## Diagnostics

Log:

- feature ID;
- exact resolved target identity;
- owner ID;
- installation result;
- reason when disabled.

Do not treat "plug-in loaded" as equivalent to "feature patch installed".

See the runnable [Mono patch-health example](../../../examples/mono/harmony/patch-health/README.md).
