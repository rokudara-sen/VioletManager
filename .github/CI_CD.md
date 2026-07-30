# CI/CD

How VioletManager is validated, deployed and released. Everything lives under `.github/`.

## Branch model

| Branch | Role | On push |
| --- | --- | --- |
| feature branches | day-to-day work | nothing; open a PR into `develop` |
| `develop` | integration / testing | full CI, then deploy to the `development` environment |
| `master` | production | full CI, then deploy to the `production` environment |
| tag `v*.*.*` | release | CI on the tagged commit, then build artifacts and publish a GitHub Release |

`develop` does not exist yet in this repository. Create it once and treat it as the default
target for pull requests:

```bash
git switch -c develop master
git push -u origin develop
```

## Workflows

| File | Trigger | What it does |
| --- | --- | --- |
| `workflows/ci.yml` | `pull_request` into `develop`/`master`, plus `workflow_call` | lint, test, build, publish the API, build the container image |
| `workflows/cd.yml` | push to `develop` or `master` | calls `ci.yml`, pushes the image to GHCR, deploys to the matching environment |
| `workflows/release.yml` | push of a `v*.*.*` tag | calls `ci.yml` on the tagged commit, builds archives + checksums, pushes versioned images, creates the GitHub Release |
| `actions/setup-dotnet/action.yml` | — | shared composite action: install SDK, restore NuGet cache, restore solution |
| `actions/deploy/action.yml` | — | shared composite action: pull the image onto a Docker host over SSH and restart it |

`ci.yml` is the single source of truth for validation. `cd.yml` and `release.yml` both *call*
it rather than re-implementing it, so a deployment can never run checks weaker than a PR.

### Commands the workflows run

Derived from the solution as it stands (.NET 10, MSTest 4.0.2, 7 projects):

```bash
dotnet restore VioletManager.sln
dotnet format  VioletManager.sln --verify-no-changes --no-restore          # lint
dotnet build   VioletManager.sln -c Release --no-restore                  # build
dotnet test    VioletManager.sln -c Release --no-build \
  --logger trx --results-directory artifacts/test-results \
  --collect "Code Coverage;Format=cobertura"                              # test
dotnet publish VioletManager.API/VioletManager.API.csproj -c Release \
  --no-restore -o artifacts/publish/api                                   # publish
docker build -f Dockerfile .                                              # image
```

There is no npm/yarn step and no separate linter package: `dotnet format` is the linter,
shipped with the SDK. The test projects currently contain no `[TestMethod]`s, so the test job
reports zero tests and still succeeds — it will start enforcing as soon as tests are added.

## Environments

Create two environments under **Settings → Environments**.

### `development`

- Deployment branches: `develop` only.
- No reviewers — this is meant to deploy on every merge.

### `production` (protected)

- Deployment branches: `master` only.
- **Required reviewers**: at least one. The deploy job waits for approval before it runs, so
  production secrets are only read after a human signs off.
- Optionally a wait timer.

## Secrets and variables

Set these **per environment**, not at repository level, so `development` and `production`
never share values. Nothing here is required for CI to pass — an unconfigured environment
deploys nothing and logs a warning instead of failing.

### Environment secrets

| Secret | Required for deploy | Purpose |
| --- | --- | --- |
| `DEPLOY_SSH_HOST` | yes | Hostname/IP of the Docker host. Empty ⇒ deployment is skipped with a warning. |
| `DEPLOY_SSH_USER` | yes | SSH user on that host (must be able to run `docker`). |
| `DEPLOY_SSH_KEY` | yes | PEM private key for that user. Empty ⇒ deployment is skipped. |
| `DEPLOY_SSH_KNOWN_HOSTS` | recommended | Output of `ssh-keyscan -p 22 your-host`. Without it the host key is trusted on first use and a warning is emitted. |
| `APP_ENV_FILE` | optional | `KEY=VALUE` lines (connection strings, API keys). Written to a `0600` file on the host and passed to the container via `--env-file`. |
| `REGISTRY_USERNAME` | optional | Overrides `github.actor` for the registry login on the host. |
| `REGISTRY_PASSWORD` | optional | Overrides the job's `GITHUB_TOKEN`. Use a PAT with `read:packages` if you would rather not forward the job token to the host. |

### Environment variables

| Variable | Default | Purpose |
| --- | --- | --- |
| `ENVIRONMENT_URL` | — | Shown as the deployment link on the run and in the Environments tab. |
| `HEALTH_CHECK_URL` | — | Polled for HTTP 200 for up to 30s after deploying. Unset ⇒ no health check. |
| `CONTAINER_NAME` | `violetmanager-api-development` / `-production` | Container name on the host. |
| `APP_PORT` | `8080` | Host port mapped to the container's `8080`. |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production` | Passed into the container. |
| `DEPLOY_SSH_PORT` | `22` | SSH port. |

### Repository-level secrets

None. Image pushes use the built-in `GITHUB_TOKEN` against `ghcr.io`, and releases are
created with the same token.

If the image push fails with a permissions error, check
**Settings → Actions → General → Workflow permissions** and make sure Actions is allowed to
write packages, and that the package's *Manage Actions access* lists this repository.

## Security model

- `permissions: {}` at the top of every workflow; each job re-grants only what it needs
  (`contents: read` for build jobs, `packages: write` only for the image publish job,
  `contents: write` only for the job that creates the release).
- Pull requests run on the `pull_request` event, never `pull_request_target`, so a fork PR
  gets a read-only token and cannot run code with write access.
- `ci.yml` never declares an `environment:`, so **no job that a pull request can trigger is
  able to read development or production secrets**. Only the two deploy jobs in `cd.yml`
  declare an environment, and both are branch-restricted by `if:` plus the environment's own
  deployment-branch rule.
- On PRs the container image is built with `push: false` by a job that has no registry
  permission at all.
- Deployment concurrency: `cd.yml` allows one run per branch (`cancel-in-progress: false`),
  and each deploy job additionally holds a per-environment lock (`deploy-development` /
  `deploy-production`) so two runs can never deploy to the same environment at once. PR
  validation runs *are* cancelled when superseded; deployment-gating runs are not.
- Images are deployed by digest (`name@sha256:...`), so the artifact that was tested is the
  artifact that runs.

Third-party actions are pinned to major tags for readability. For a stricter supply-chain
posture, repin them to full commit SHAs (`actions/checkout@<sha> # v4.2.2`) and let
Dependabot bump them.

## Branch protection

Add rules for `develop` and `master`:

- Require a pull request before merging.
- Require status checks to pass → **`CI status`** (the aggregate job in `ci.yml`; it fails if
  any of lint/test/build/container did not succeed).
- Require branches to be up to date before merging.
- For `master`, also restrict who can push.

On CD and release runs the same check appears as `Validate / CI status` and
`Verify / CI status` respectively, because reusable-workflow jobs are prefixed with the
calling job's name.

## Releasing

```bash
git switch master
git pull
git tag -a v1.0.0 -m "v1.0.0"
git push origin v1.0.0
```

That runs `release.yml`, which:

1. Re-runs lint/test/build against the exact commit the tag points at.
2. Publishes four archives of `VioletManager.API`:
   - `VioletManager.API-<version>-portable.zip` — framework-dependent, needs .NET 10 installed
   - `VioletManager.API-<version>-linux-x64.tar.gz` — self-contained single file
   - `VioletManager.API-<version>-linux-arm64.tar.gz` — self-contained single file
   - `VioletManager.API-<version>-win-x64.zip` — self-contained single file
3. Writes `SHA256SUMS.txt` covering every asset (verify with
   `sha256sum --check --ignore-missing SHA256SUMS.txt`).
4. Pushes `ghcr.io/<owner>/violetmanager:<version>`, `:<major>.<minor>`, `:sha-<sha>` and —
   for stable tags only — `:latest`.
5. Creates the GitHub Release with auto-generated notes, prefixed by the image reference and
   checksum instructions.

A tag containing a hyphen (`v1.0.0-rc.1`) is published as a pre-release and does not move
the `latest` image tag. The self-contained archives are ~100 MB each; trim the matrix in
`release.yml` if you do not need them.

## Adapting the deploy step

`actions/deploy/action.yml` implements the common case: SSH to a host, `docker pull`, replace
the container. Swap the **Deploy over SSH** step for your platform if you deploy elsewhere —
the surrounding contract (image reference in, health check and summary out) stays the same:

- **Azure Web App**: `azure/webapps-deploy@v3` with `images: ${{ inputs.image }}`.
- **Kubernetes**: `kubectl set image deployment/violetmanager-api api=${{ inputs.image }}`
  plus `kubectl rollout status`.
- **Cloud OIDC** (no long-lived cloud keys): add `id-token: write` to the deploy job's
  `permissions` and use the provider's login action instead of the SSH secrets.

The app has no dedicated health endpoint yet. Until one exists, point `HEALTH_CHECK_URL` at
an endpoint that returns 200 (e.g. `https://…/weatherforecast`) or leave it unset.

## Known follow-ups

- `dotnet restore` reports `NU1903`: `Microsoft.OpenApi` 2.0.0 has a known high-severity
  advisory (pulled in transitively by `Microsoft.AspNetCore.OpenApi` 10.0.10). The lint job
  surfaces it via `dotnet list package --vulnerable` as an advisory step. Bump the package
  and consider making the build `-warnaserror` once it is clean.
- No `.editorconfig` exists, so `dotnet format` enforces SDK defaults only. Adding one makes
  the lint job meaningfully stricter.
- Consider adding a `/health` endpoint so deployments can be verified properly.
