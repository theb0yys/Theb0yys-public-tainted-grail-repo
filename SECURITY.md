# Security

## Reporting

The normal bug-report form is **not** for sensitive material.

Do not publish credentials, private paths, personal data, exploit details, or proprietary game content in an issue, pull request, discussion, screenshot, or log excerpt.

If GitHub private vulnerability reporting is available for this repository, use the repository **Security** area to submit the report privately.

If private vulnerability reporting is unavailable, open only a minimal public issue asking for a private reporting contact. Do **not** include the vulnerability details, reproduction, secrets, private paths, or proprietary material in that issue.

## Repository rules

- Never commit secrets or signing keys.
- Never commit private/local game paths.
- Do not add download-and-execute behavior to examples.
- Do not add persistence, credential access, antivirus exclusions, or unrelated machine scanning.
- Keep file writes inside explicit mod-owned directories.
- Treat external input as untrusted and validate sizes/paths.
- Do not distribute game or third-party binaries from this repository.

The CI public-surface guard is a backstop, not a substitute for review.

## What a useful security report should contain

When a private channel is available, include only what is necessary to evaluate the issue:

- affected repository path or component;
- impact;
- smallest safe reproduction;
- relevant version/commit;
- whether the issue exposes secrets, writes outside mod-owned paths, executes untrusted content, or broadens machine access;
- suggested mitigation if known.

Avoid attaching unrelated game files, saves, binaries, or full machine diagnostics.
