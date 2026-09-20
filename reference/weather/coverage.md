# Weather Stack Coverage

Current extracted private evidence:

| Lane | Result |
| --- | --- |
| Weather truth: Rain | PASS in bounded live stack |
| Time bucket: Day | PASS |
| Sky request → Skybox consumer | PASS |
| Contextual sky catalog/apply | PASS for cited Day/Rain candidate |
| Water plan → Immersive Water | PASS |
| GreenShallows apply to ocean/lake/river surfaces | PASS in cited scene |
| Same-session sky/weather visual proof | PARTIAL / accepted for active stack |
| Water-visible visual proof | NOT_RUN in cited record |
| Clear, Fog, Storm, WyrdHeavy, Hail, Sleet, Snow stack matrix | NOT_RUN in cited record |
| Morning, Evening, Night matrix | NOT_RUN |
| Other water presets | NOT_RUN |
| stale/session/scene negative controls | NOT_RUN |
| disable/unload/restore | NOT_RUN |
| performance/soak/release | NOT_RUN |

Do not turn one weather-state integration pass into “weather system fully validated”.
