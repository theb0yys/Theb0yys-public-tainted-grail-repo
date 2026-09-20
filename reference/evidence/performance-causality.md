# Performance Evidence and Causality

Use increasingly strong evidence for increasingly specific claims.

| Evidence | Safe claim |
| --- | --- |
| Frame-time sample | “This window had these frame-time characteristics.” |
| Allocation/GC counters | “GC/allocation pressure was present in this window.” |
| Loaded plugin list | “These plugins were loaded.” |
| Disabled vs enabled same-save comparison | “The measured scenario changed with this component toggled.” |
| Controlled mod-stack isolation | “This mod/config correlates with the measured difference under the controlled test.” |
| Owned-code timers / profiler | “This code path consumed measured time/allocations under the captured workload.” |
| Native profiler / traced subsystem | Narrow native causality claim, bounded to the capture |

**Loaded ≠ causal.**

A single report does not establish broad hardware compatibility, save behaviour, or a universal performance budget.
