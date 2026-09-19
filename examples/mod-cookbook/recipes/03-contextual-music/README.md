# Recipe — Contextual Music Routing

**Category:** audio / state routing  
**Source-path evidence:** RUNTIME_EVIDENCED for selected lane decisions  
**Public recipe:** NOT_RUN

The owner-side music system produced runtime lane evidence for at least:

- daytime open world;
- open-world night returning to no custom lane;
- Wyrdness overriding normal routing after confirmation.

Its later full-library/native-suppression matrix remained incomplete.

## Minimal router

Treat context selection and audio playback as separate systems.

### 1. Read context

Useful inspected inputs include:

- active Unity scene;
- FoA `SceneService` native/display scene names;
- open-world/interior state;
- native time/night state;
- hero availability;
- Wyrdness exposure.

### 2. Resolve one lane

Example priority:

```text
Wyrdness
Scary place
Settlement
Day open world
Interior
None
```

Keep the result as an enum/state value and log transitions.

### 3. Require stability where needed

The owner-side route required Wyrdness to remain exposed for a confirmation period before switching lanes. That prevents one-frame context noise from restarting music.

### 4. Playback owns only mod audio

- keep one mod-owned audio root;
- fade old/new voices;
- clean them on scene/state changes;
- package only audio you have redistribution rights for.

### 5. Native suppression is a separate gate

Do not stop native music just because your router selected a lane.

Suppress native audio only after your replacement has actually started and only for contexts you have tested. Fail open when replacement playback fails.
