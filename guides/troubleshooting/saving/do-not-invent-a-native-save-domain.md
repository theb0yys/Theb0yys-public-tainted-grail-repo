# Do Not Invent a Native Save Domain

Use this when a mod wants its own FoA save section/domain and you are tempted to create a new `<name>.data` entry and assume the game will restore it.

Working lesson: [Native Save-Domain Boundary](../../../research/case-studies/persistence/native-save-domain-boundary.md).

## Symptom / design question

```text
I can place custom data in the save/archive
→ therefore FoA will deserialize it as my new domain later
```

That inference is not supported by the inspected build.

## What static research found

For the inspected `TG.Main.dll` hash, no supported mutable registrar for arbitrary new native save Domains was found.

The native save coordinator uses a fixed domain sequence.

Unknown entries may be cached/round-tripped, but native restoration does not automatically discover arbitrary names and deserialize them into a new mod domain.

## Consequence

Passive archive round-trip is **not** proof of custom-domain restoration.

Do not publish:

- "native custom save domain";
- "automatic mod domain restore";

unless a real registration/restoration owner is proven.

## Safer architecture

Move mod persistence to a separate sidecar contract when native-domain registration is unavailable.

Then prove separately:

```text
native save identity/slot observed
→ mod sidecar written
→ sidecar associated with exact save
→ load event observed
→ sidecar restored
→ missing/corrupt sidecar handled
→ uninstall/rollback policy defined
```

## Verification

A persistence design must distinguish:

- bytes were written;
- bytes round-tripped;
- native game restored them;
- mod restored them;
- restoration timing;
- save identity binding;
- failure/migration policy.

Those are different claims.

## Evidence boundary

The inspected build provided negative static evidence for arbitrary native-domain registration. This page is a design correction, not proof that every possible sidecar architecture is already validated.
