---
document_type: troubleshooting
scope: Harmony/native hook does not execute
last_verified: 2026-09-20
---

# Hook Did Not Fire

Work from the bottom of the stack.

1. Did the loader start?
2. Did the plug-in load?
3. Did the target type/method resolve in this runtime/build?
4. Did the patch install?
5. Does the game actually execute that target in the scenario?
6. Is another owner/path used instead?
7. Is the relevant object/lifecycle not ready yet?

Do not immediately add more patch targets. First establish whether the current target belongs to the real owner and lifecycle.

For IL2CPP, also verify interop state and the runtime representation before treating a source-compatible target as live-compatible.
