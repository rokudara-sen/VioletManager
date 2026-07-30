# VioletManager

A contact manager web application: store people and organisations, their work and private
contact details, and organise them with categories and subcategories.

> **Status: in progress.** The domain model and the first application command are in place.
> There is no HTTP API for contacts yet (the API project still serves the template
> `/weatherforecast` endpoint), no persistence implementation, and no front end. Treat
> everything below as the shape of the project rather than a finished product.

## What works today

- `Contact` aggregate with name normalisation, work/private `ContactDetails`, `PostalAddress`,
  company name, notes, and case-insensitive category/subcategory sets.
- `CreateContactCommand` / `CreateContactHandler` / `CreateContactResult` in the application
  layer, writing through the `IContactRepository` abstraction.
- Unit tests for the domain entity and value objects.
- CI/CD: lint, test, build, container build, GHCR publish, SSH deploy, tagged releases.

## Not built yet

- `IContactRepository` has no implementation — `VioletManager.Infrastructure` references EF
  Core but contains no `DbContext`, entity configuration or migrations.
- No contact endpoints, no read/update/delete operations, no validation pipeline.
- No database is configured; `appsettings.json` has no connection string.
- No web front end and no `/health` endpoint.

## Architecture

Clean-architecture layering; dependencies point inwards only.

| Project | Role |
| --- | --- |
| `VioletManager.Domain` | Entities and value objects. No dependencies. |
| `VioletManager.Application` | Use cases (commands, handlers) and port interfaces. Depends on Domain. |
| `VioletManager.Infrastructure` | Adapters — persistence and external services. Depends on Application. |
| `VioletManager.API` | ASP.NET Core minimal-API host. Depends on Application. |
| `VioletManager.Domain.UnitTests` | Domain unit tests (MSTest). |
| `VioletManager.Application.UnitTests` | Application unit tests (MSTest). |
| `VioletManager.IntegrationTests` | End-to-end / infrastructure tests (MSTest). |

The API does not reference Infrastructure yet; wiring the two together is part of the
persistence work still to be done.

## Requirements

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (`net10.0`)
- Docker (optional — only for the container image and `compose.yaml`)

## Getting started

```bash
git clone https://github.com/rokudara-sen/VioletManager.git
cd VioletManager
dotnet restore VioletManager.sln
dotnet build VioletManager.sln -c Release
dotnet test VioletManager.sln -c Release
dotnet run --project VioletManager.API
```

In `Development`, the OpenAPI document is served at `/openapi/v1.json`.

### With Docker

```bash
docker compose up --build
```

## Development commands

```bash
dotnet format VioletManager.sln                      # apply formatting
dotnet format VioletManager.sln --verify-no-changes   # what CI's lint job checks
dotnet build  VioletManager.sln -c Release
dotnet test   VioletManager.sln -c Release
```

`dotnet format` is the linter — there is no separate tool. Note that no `.editorconfig` exists
yet, so only SDK defaults are enforced.

## CI/CD

Feature branches → PR into `develop` → promote `develop` into `master` for production. Pushes
to `develop`/`master` deploy to the matching GitHub environment; a `v*.*.*` tag cuts a
release. Full details, required secrets and the security model are in
[`.github/CI_CD.md`](.github/CI_CD.md).

## Contributing

See [CONTRIBUTING.md](CONTRIBUTING.md). For vulnerabilities, see [SECURITY.md](SECURITY.md).

## License

No license has been chosen yet — [`LICENSE`](LICENSE) is intentionally empty. Until it is
filled in, all rights are reserved and the code is not licensed for reuse.
