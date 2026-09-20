# Status Providers

A mod can expose a cheap read-only health/status snapshot through:

`FoAModManagerApi.RegisterStatusProvider(...)`

The snapshot supports:

- status level;
- summary;
- detail;
- schema/version string;
- updated time;
- additional lines.

## Use it for

- dependency readiness;
- selected operating mode;
- feature enabled/disabled state;
- compatibility warnings;
- last evidence/refresh state;
- a concise “why blocked” message.

## Do not use it for

- polling expensive world scans;
- mutating the feature;
- triggering repairs;
- writing files every refresh.

Status providers should be cheap and read-only.
