# Avalon AI FoA Host — Dual-Runtime Starter

Use this starter when you are building the FoA host side of Avalon AI across both Mono and IL2CPP.

Keep AI/package-facing logic shared, and put loader/runtime-specific host wiring in the separate Mono and IL2CPP projects so native integration differences do not leak through the whole codebase.
