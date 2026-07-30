# Contributing to VioletManager

Thanks for taking the time to contribute. VioletManager is an in-progress contact manager web
application built on .NET 10 with a clean-architecture layout — see the
[README](README.md) for the current state of the code.

## Before you start

- Search the [issues](https://github.com/rokudara-sen/VioletManager/issues) first. For anything
  larger than a small fix, open an issue (bug report or feature request template) so the
  approach can be agreed before you write code.
- Never report a security vulnerability as a public issue — follow [SECURITY.md](SECURITY.md).

## Setting up

```bash
git clone https://github.com/rokudara-sen/VioletManager.git
cd VioletManager
dotnet restore VioletManager.sln
dotnet build VioletManager.sln -c Release
dotnet test VioletManager.sln -c Release
```

You need the .NET 10 SDK. Docker is optional and only needed for the container image.

## Branches

| Branch | Purpose |
| --- | --- |
| feature branches | your work — branch off `develop` |
| `develop` | integration; deploys to the `development` environment |
| `master` | production; deploys to the `production` environment |

Open pull requests against `develop`. Only release promotions target `master`. If `develop`
does not exist yet in your clone, create it from `master` (see `.github/CI_CD.md`).

Name branches by type: `feature/…`, `fix/…`, `refactor/…`, `chore/…`, `docs/…`.

## Commits

Follow the existing history: a lowercase type prefix, a colon, then an imperative summary.

```
add: createContact with handler, application level
refactor: contact entity refactoring
chore: added new unit tests
```

Common prefixes: `add`, `fix`, `refactor`, `chore`, `docs`, `test`. Keep commits focused —
one logical change each.

## Coding guidelines

Match the surrounding code rather than introducing a new style. In practice that means:

- Nullable reference types and implicit usings are enabled in every project; keep them clean.
- Domain objects are immutable from the outside: `private set` properties, private constructors,
  static `Create` factories, and behaviour-named mutators (`SetWorkDetails`, `AddCategory`).
- Validate in the domain and throw `ArgumentException` / `ArgumentNullException` with a clear
  message and the parameter name. Normalise strings (trim, empty ⇒ `null`) at the boundary.
- Expose collections as `IReadOnlyCollection<T>` over a private backing list.
- Dependencies point inwards: Domain depends on nothing, Application on Domain, Infrastructure
  and API on Application. Do not add a reference that reverses this.
- Application handlers depend on interfaces (`IContactRepository`), never on a concrete adapter.
- Keep EF Core concerns out of Domain and Application. The parameterless private constructor on
  `Contact` is the one concession, and it is commented as such.
- Run `dotnet format VioletManager.sln` before committing; CI fails on formatting differences.

## Tests

- MSTest across all three test projects. Unit tests mirror the source layout
  (`VioletManager.Domain.UnitTests/Entities/ContactTest.cs`), named `<TypeName>Test.cs`.
- Every behaviour change needs a test. Bug fixes need a test that fails before the fix.
- Put infrastructure- or HTTP-level tests in `VioletManager.IntegrationTests`.

## Pull requests

1. Rebase or merge the latest `develop` into your branch.
2. Make sure all three of these pass locally:
   ```bash
   dotnet format VioletManager.sln --verify-no-changes
   dotnet build  VioletManager.sln -c Release
   dotnet test   VioletManager.sln -c Release
   ```
3. Fill in the pull request template — summary, related issues, affected projects, how you
   verified it, and deployment notes.
4. Keep the diff reviewable. Split unrelated changes into separate PRs.
5. The aggregate **`CI status`** check must be green. It covers lint, test, build and the
   container image build.
6. Never commit secrets, tokens, connection strings, or real personal data. Contact records are
   personal data — use obviously fake values in tests and fixtures.

## Documentation

Update the README when you change how the project is built, run or configured, and
`.github/CI_CD.md` when you change a workflow, secret or environment.

## License

The project has no license yet (`LICENSE` is intentionally empty). By contributing you agree
that your contribution may be released under whichever license the maintainers later adopt.
