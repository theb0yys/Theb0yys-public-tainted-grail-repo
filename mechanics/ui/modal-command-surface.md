---
document_type: mechanic
scope: plugin-owned modal command/dialogue-style UI
runtime: mono
evidence:
  static: SOURCE_INSPECTED
  runtime: PARTIAL_BY_CONSUMER
last_verified: 2026-09-20
---

# Modal Command Surface

A visible panel is not a functioning UI path.

A reliable plugin-owned command surface needs a complete ownership path:

```text
game-owned world entry/prompt
→ plugin UI host opens
→ cursor/focus ownership acquired
→ gameplay input suppressed
→ UI input path remains available
→ event dispatch reaches button
→ command handler runs
→ UI closes
→ cursor/input state restored
```

## Important split: IMGUI vs Unity UI

A debug IMGUI panel can disable Unity input modules because IMGUI receives events directly.

A Unity UI `Canvas` command/dialogue host **must keep its EventSystem/input modules available**, or buttons may render but never click.

The private companion route kept a strict debug-panel lock but allowed Rewired/UI event reads through while the Unity UI dialogue surface was visible.

## Do not call plugin-owned Unity UI “native dialogue”

True FoA dialogue/story surfaces depend on authored Story Graph / StoryBookmark data that was not proven safe to author/register at runtime in this route.

## Required close path

`Esc`, explicit close/goodbye and command completion should all release:

- custom UI scope;
- cursor ownership;
- world/player freeze;
- temporary input suppression.

See [UI opens but action does not fire](../../diagnose/ui-opens-but-does-not-work.md).
