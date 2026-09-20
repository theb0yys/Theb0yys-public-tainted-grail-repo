# Hybrid Templates

Hybrid templates combine **Merlin-authored content** with a separate **runtime integration plug-in**.

The key rule is lifecycle separation: the Merlin project owns authored/exported content; the BepInEx project owns runtime code. Do not turn either side into an implicit build dependency on the other's private workspace.

- [Mono + Merlin](mono-merlin/)
