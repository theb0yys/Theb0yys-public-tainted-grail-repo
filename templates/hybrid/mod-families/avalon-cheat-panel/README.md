# Avalon Cheat Panel — Dual-Runtime Starter

Use this starter for a cross-runtime cheat or debug panel that shares feature logic while keeping Mono and IL2CPP host code separate.

The shared project should own the panel's feature model; runtime-specific projects should own only the loader, interop, and exact FoA access needed on that runtime.
