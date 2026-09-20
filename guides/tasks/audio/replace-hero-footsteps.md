# Replace the Hero's Footsteps

Use this guide to build a Mono/BepInEx footstep replacement mod while keeping FoA's other audio untouched.

The working route is intentionally narrow: intercept the native FMOD one-shot path, prove the request belongs to `VHeroFootsteps`, choose a replacement from FoA's own surface parameters, and suppress the native footstep **only when the replacement actually starts**.

The proven lineage is [Native Hero Footstep Replacement](../../../research/case-studies/audio/footsteps.md).

## What you will build

```text
native FMOD one-shot request
→ is the source VHeroFootsteps?
    no  → leave it alone
    yes
      → read FoA footstep surface parameters
      → select replacement
      → try custom playback
          success → suppress this native footstep
          failure → allow native footstep
```

A runnable source example already exists at [Hero Footstep Replacement](../../../examples/mono/audio/footstep-replacement/README.md).

## Prerequisites

1. complete the [first Mono plug-in](../../getting-started/first-mono-plugin.md);
2. build and run the provided footstep example unchanged first;
3. confirm its generated test sound can replace a hero footstep;
4. only then introduce your own audio files.

Read [Audio, FMOD, Event Identity, and Safe Replacement Boundaries](../../../knowledge/systems/presentation/audio-fmod-integration.md) before broadening the hook.

## Step 1 — start from the working example

Build:

```powershell
dotnet build .\FootstepReplacement.csproj -c Release -p:FoAGameRoot="C:\Path\To\Tainted Grail FoA"
```

The public example generates a small tone rather than redistributing audio assets.

Your first goal is not "perfect sound". It is:

> prove the owner, hook and suppression gate on your game build.

## Step 2 — filter to the real footstep owner

FoA's hero footstep path eventually reaches the FMOD one-shot overload used by the working mod.

Do **not** replace every FMOD one-shot.

Filter the call so your code handles only requests whose source/debug object is `VHeroFootsteps`.

That single condition prevents a footstep mod from accidentally becoming a global weapon/UI/world-audio replacement mod.

## Step 3 — use FoA's surface parameters

FoA already reports useful footstep context such as:

- `FTS_Grass`
- `FTS_Gravel`
- `FTS_Ground`
- `FTS_Mud`
- `FTS_Puddle`
- `FTS_Snow`
- `FTS_Stone`
- `FTS_Metal`

Use those native parameters to choose a replacement.

Do not immediately add a second raycast/material-classification system. The game has already done that work for this path.

A simple selector can be:

```text
Stone parameter active → choose one stone clip
Grass parameter active → choose one grass clip
...
no supported match      → do not replace
```

## Step 4 — load and cache your audio

For a real sound pack:

1. validate the configured audio directory;
2. load supported files once;
3. keep decoded/usable sound data cached;
4. map each surface category to one or more clips;
5. do not repeatedly recreate the same sound resource on every footstep.

Keep copyrighted/commercial game audio out of the public repository unless you have redistribution rights.

## Step 5 — apply the replacement gate

The critical invariant is:

> Never suppress the native event merely because you intended to replace it.

Use this order:

```text
matching hero footstep
→ select replacement
→ attempt custom playback
→ custom playback started?
    yes → suppress native event
    no  → native event continues
```

This makes missing files, unsupported surfaces and playback failures fail open to normal game audio.

The generic version of this pattern is documented in [Audio Replacement Gate](../../../examples/mono/audio/replacement-gate/README.md).

## Step 6 — keep volume and diagnostics bounded

For a first release, expose one master replacement volume.

Also:

- cap repetitive diagnostic logging;
- do not log every footstep forever;
- reuse cached resources;
- release mod-owned audio resources on shutdown;
- keep unrelated FMOD events completely untouched.

## Verify in game

Walk across several surfaces and check:

1. the plug-in loads with no audio initialization error;
2. only hero footsteps enter your replacement path;
3. supported surfaces play the expected replacement family;
4. unsupported/missing replacements fall back to native audio;
5. unrelated combat, UI, ambience, dialogue and world audio remain normal;
6. repeated walking does not produce unbounded allocations/log spam;
7. disabling/removing the mod restores native footsteps;
8. no custom audio resource remains owned after shutdown.

The working case recorded a walking trace with **57 custom footstep plays while native footstep suppression was enabled**.

## Common mistakes

### Patching all FMOD one-shots

This is too broad. Prove the request belongs to `VHeroFootsteps`.

### Suppressing before playback succeeds

A bad/missing file then becomes silence. Suppress only after the replacement has actually started.

### Re-detecting the ground yourself

Start with FoA's supplied footstep parameters. Add a custom classifier only if you have a specific unsupported requirement.

### Forcing Unity AudioSource

The tested FoA audio lane is FMOD-owned. Use the actual owner rather than forcing a parallel Unity audio path.

### Treating filenames as native event identity

Your file called `stone.wav` is your policy. It does not establish a native FMOD event identity.

## Evidence boundary

**Proven:** narrowly scoped hero footstep replacement through the FMOD one-shot path, native-surface parameter selection, custom playback, and conditional native suppression.

**Not claimed:** a universal FMOD replacement API, native Studio-event recreation, weapon/creature/music replacement, or arbitrary persistent/parameterized event replacement.

## Next steps

Once this works reliably:

- add multiple clips per surface;
- add deterministic/random selection policy;
- expose per-surface volume;
- add configuration reload;
- study [Audio/FMOD ownership](../../../knowledge/systems/presentation/audio-fmod-integration.md) before attempting other audio domains.
