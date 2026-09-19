# Persistence Architecture: Native Saves and Mod-Owned Sidecars

> **Reference/process page.** The current research establishes an important negative result: FoA's native save-domain system is **not** a generic mod-owned serialization registry in the inspected build.

## What this system is

FoA owns its native save archive and a fixed set of native save domains.

Current-binary research found:

- Gameplay domain participation;
- active scene-derived domains;
- Metadata handled separately;
- no recovered mutable registrar for arbitrary mod-owned save domains.

Unknown extra data can be cached/round-tripped in some circumstances, but native restore does not automatically deserialize an arbitrary mod-owned domain.

Therefore:

> Do not design a generic mod save system by patching yourself into FoA's native domain list without new evidence.

The current candidate architecture for independent mod state is an **external sidecar**, not native archive injection.

## Who owns it in FoA

### Native game owns

- save guards;
- slot selection;
- native domain serialization;
- provider/cloud/local writes;
- native cache/load;
- Gameplay/scene restoration;
- native save UI/archive format.

### Proposed shared sidecar owner would own

- mod namespace registration;
- deterministic capture;
- slot/generation binding;
- atomic external file commit;
- integrity/schema/migration;
- staged load;
- one-time apply;
- quarantine/backup/diagnostics.

### Consumer mod owns

- stable namespace ID;
- payload schema;
- capture of its own state;
- migrations;
- prerequisites;
- idempotent apply;
- absent/corrupt-state policy.

## Important identities, types, and methods

Native/candidate surfaces include:

- `DomainUtils.SaveSlotDomainsInUse()`
- `LoadSave.Save(SaveSlot, bool)`
- `LoadSave.QuickSave()`
- `SaveSlot.SaveFileName`
- `SaveSlot.ID`
- provider `BeginSave` / concrete `EndSave(string)`
- `SaveInProgressHandle.MarkSucceeded` candidate
- `LoadSave.LoadSaveSlotToCache(...)`
- `GameplayConstructor.RestoreGameplay`
- scene restore/lifecycle callbacks
- `SceneLifetimeEvents.Events.AfterSceneStoriesExecuted` candidate

## Where it exists in the lifecycle

A robust sidecar concept is **not** one `OnSave` callback.

### Save

~~~text
native save request
→ capture immutable mod state for this generation
→ stage sidecar bytes
→ native save proceeds
→ correlate native success to the same generation
→ atomically commit sidecar
~~~

### Load

~~~text
native slot selected
→ stage/validate sidecar
→ native Gameplay restore
→ native scene restore
→ required templates/services/mods ready
→ validate dependencies/migration
→ apply each namespace once
~~~

Capture, stage, commit and apply are different states.

## How we interact with it

### Never modify the native save archive for generic mod state

Current evidence does not justify it.

### Bind sidecar state to native slot identity carefully

Human-visible save name is presentation, not a sufficient storage key.

Candidate identity needs provider + exact native slot ID plus mismatch guards/generation evidence.

### Write only inside mod-owned storage

Never traverse/write into the FoA save archive directory as part of sidecar storage.

### Use strict schemas

Do not deserialize arbitrary CLR type metadata from user/mod files.

### Keep native save authoritative

Sidecar failure must not cancel or corrupt native save/load.

### Apply only after dependencies are ready

A sidecar that references custom templates cannot apply before those templates are registered.

## Why this route

The research explicitly looked for a mutable native domain-registration path and did not find one.

That negative result matters: it prevents an attractive but unsupported architecture based on injecting arbitrary mod state into the native save archive.

It also exposed that persistence needs transaction semantics:

- overlapping saves;
- provider success/failure;
- crash windows;
- slot rename/delete/copy;
- autosave rotation;
- migration;
- missing consumer mod;
- corrupted payload;
- load ordering.

## What goes wrong

### One callback = save support

Rejected. Capture and durable commit occur at different points.

### Slot ID alone without generation correlation

Can associate a late completion with the wrong save request if overlapping/retry semantics exist.

### Commit before native success

Can make mod state newer than the native save.

### Apply before native/game dependencies are ready

Can fail to resolve templates/actors/scenes and create false corruption.

### Sidecar failure blocking native load

Rejected. Native save must continue independently.

### Automatically moving/deleting sidecars on rename/delete assumptions

Blocked until native slot semantics are proven.

### Native archive mutation

Rejected as the generic path in the inspected build.

## How to verify

A shared sidecar system would need a full matrix covering:

- manual save;
- quicksave/autosave rotation;
- native save denied/failure;
- process crash at staging/replace/manifest points;
- missing/corrupt/unknown-version sidecar;
- migrations;
- consumer absent/reinstalled;
- service absent;
- rename/delete/slot reuse;
- copied save without sidecar;
- wrong-slot sidecar;
- new-game/load/scene transition ordering;
- template readiness before apply;
- namespace dependencies/failure isolation;
- repeated load events;
- game-update review.

## Current proof boundary

**Proven/static negative:** no supported mutable native mod-domain registrar was recovered in the inspected binary; native domain membership is fixed around native Gameplay/scene ownership.

**Strong candidate architecture:** external namespace-isolated sidecar with staged/transactional lifecycle.

**Not yet a public working process:** the shared sidecar implementation and its crash/save/load compatibility matrix remain under evaluation and runtime validation is incomplete.

For ordinary custom item/actor proofs, prefer explicit native save behavior or deliberate `NOT_SAVED` session ownership until persistence is proven.
