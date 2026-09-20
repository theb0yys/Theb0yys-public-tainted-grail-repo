# Poise Is Not Stagger

Decompilation showed that NPC poise accumulation/poise-break and stamina-driven stagger are separate.

The project therefore scaled incoming poise damage around the poise-processing path instead of changing the `PoiseThreshold` limited stat as though it were a simple threshold setting.

## Lesson

A name like “threshold” may describe an accumulated runtime meter rather than a configuration scalar. Inspect how the value is used before tweaking it.
