# Repository Taxonomy

Organize first by content class, then by domain or component, then by specific subject.

    content class
      → domain / component
        → specific subject

| Surface | Ownership |
| --- | --- |
| `pipelines/` | End-to-end ordered reconstruction for major content-import projects; stage gates and cross-surface orchestration |
| `platform/` | Shared infrastructure, supported contracts and integration recipes |
| `guides/` | Learning, bounded tasks, troubleshooting and shipping |
| `knowledge/systems/` | Native FoA ownership and execution |
| `knowledge/mechanics/` | Reusable modding capabilities and intervention boundaries |
| `knowledge/reference/` | Exact lookup |
| `research/` | Methods, investigations, case studies and provenance |
| `examples/` | Runnable mechanism demonstrations |
| `templates/` | Reusable project starters |

Use consistent domain names across surfaces. Do not create competing synonyms for an existing domain. Multiple routes may point to a subject, but one maintained explanation owns each system/evidence claim.

Pipeline pages own execution order, stage prerequisites, produced artifacts, validation gates, failure boundaries, and reconstruction flow. They should link to the canonical Knowledge/Research/Platform/Examples material that owns deeper system or evidence claims.

Do not flatten a major multi-stage pipeline into one long page. See [Pipeline documentation](authoring/pipelines.md).
