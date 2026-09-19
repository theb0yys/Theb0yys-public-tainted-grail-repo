# Security

## Reporting

Do not publish credentials, private paths, personal data, exploit details, or proprietary game content in an issue.

If a report contains sensitive material, use GitHub private vulnerability reporting when available. Otherwise open a minimal issue asking for a private contact without including the sensitive details.

## Repository rules

- Never commit secrets or signing keys.
- Never commit private/local game paths.
- Do not add download-and-execute behavior to examples.
- Do not add persistence, credential access, antivirus exclusions, or unrelated machine scanning.
- Keep file writes inside explicit mod-owned directories.
- Treat external input as untrusted and validate sizes/paths.
- Do not distribute game or third-party binaries from this repository.

The CI public-surface guard is a backstop, not a substitute for review.
