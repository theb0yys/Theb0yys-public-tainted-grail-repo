# Pipeline Documentation

The four major custom-content projects — items, weapons, armour, and creatures — are maintained under `pipelines/` as staged, reproducible execution trees.

## Why pipelines are separate

A pipeline crosses several documentation classes at once. It may depend on:

- native ownership in Knowledge;
- unresolved or historical evidence in Research;
- shared infrastructure in Platform;
- runnable source in Examples;
- task-specific procedures in Guides.

The pipeline surface orders those pieces into a reproducible process. It does not replace the underlying canonical evidence.

## Required structure

A major pipeline must have its own domain directory and separate stage pages. Do not flatten the complete process into one long Markdown file.

Each stage page must state:

1. objective;
2. prerequisites/inputs;
3. exact operation;
4. output artifact or runtime state;
5. validation gate;
6. failure conditions where relevant;
7. what the stage does not prove;
8. next stage.

## Evidence rule

Never promote one stage into a later claim.

Examples:

- source suitability is not runtime compatibility;
- asset load is not template registration;
- template registration is not acquisition;
- a visible mesh is not native equip ownership;
- runtime success is not persistence proof;
- persistence is not release compatibility.

Use PASSED, FAILED, PARTIAL, BLOCKED, NOT_RUN, or NOT_APPLICABLE for stage status.

## Canonical-claim rule

Where a system fact already has a maintained owner under Knowledge or Research, link to it and keep the pipeline focused on execution order, gate criteria, and reconstruction.

## Navigation rule

Legacy or short-form importer pages may remain as landing pages, but they must point to the staged pipeline tree and must not become a competing single-file version of the full process.
