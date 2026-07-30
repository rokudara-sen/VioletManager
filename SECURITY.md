# Security Policy

VioletManager is a work in progress and has not been security-reviewed or hardened. It is not
ready to hold real contact data. Please treat any deployment as a development environment.

## Supported versions

There are no released versions yet. Only the tip of `master` receives fixes.

| Version | Supported |
| --- | --- |
| `master` | yes |
| everything else | no |

## Reporting a vulnerability

**Do not open a public issue for a security problem.**

Report it privately through GitHub: open the repository's
[**Security** tab → *Report a vulnerability*](https://github.com/rokudara-sen/VioletManager/security/advisories/new)
(GitHub private vulnerability reporting). If that is unavailable to you, contact the repository
owner ([@rokudara-sen](https://github.com/rokudara-sen)) directly and do not include exploit
details in a public channel.

Please include:

- what the issue is and which project/file it affects,
- the version or commit SHA you tested,
- reproduction steps or a proof of concept,
- the impact you believe it has.

This is a small hobby-scale project, so there is no guaranteed response time and no bug bounty.
Expect a best-effort acknowledgement within a couple of weeks. Please give a reasonable window
for a fix before disclosing publicly, and let us know if you intend to publish.

## Scope

In scope: the code in this repository — the API host, application and domain layers, the
Dockerfile, and the GitHub Actions workflows under `.github/`.

Out of scope: findings against third-party services (GitHub, GHCR, NuGet), issues that require
an already-compromised host or CI runner, missing hardening in features that are documented as
not implemented yet, and reports from automated scanners without a demonstrated impact.

## Known gaps

These are already known and do not need to be reported:

- No authentication or authorisation anywhere in the API.
- No persistence layer, so no encryption at rest and no data-retention handling for what is
  inherently personal data.
- `Microsoft.OpenApi` 2.0.0 arrives transitively via `Microsoft.AspNetCore.OpenApi` 10.0.10 and
  carries advisory `NU1903`. The CI lint job reports vulnerable packages as an advisory step.
- No `/health` endpoint, so deployments are not properly verified.

## Handling secrets

Never commit credentials, tokens, connection strings or real personal data. Deployment secrets
live in GitHub **environment** secrets (`development` / `production`), never at repository
level; the split and the workflow permission model are documented in
[`.github/CI_CD.md`](.github/CI_CD.md). If you believe a secret has leaked, rotate it first and
then report it.
