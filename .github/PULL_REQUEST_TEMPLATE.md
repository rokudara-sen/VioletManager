<!--
Target branch:
  feature/fix work -> develop
  develop          -> master   (release promotion only)
A merge into master deploys to production. Keep the diff reviewable.
-->

## Summary

<!-- What changes and why. One paragraph is usually enough. -->

## Related issues

<!-- "Closes #123" / "Part of #456". Write "none" if this stands alone. -->

Closes #

## Type of change

- [ ] Bug fix
- [ ] New feature
- [ ] Refactor (no behaviour change)
- [ ] Test-only change
- [ ] Build / CI / CD
- [ ] Documentation
- [ ] Breaking change

## Affected projects

- [ ] `VioletManager.API`
- [ ] `VioletManager.Application`
- [ ] `VioletManager.Domain`
- [ ] `VioletManager.Infrastructure`
- [ ] Tests
- [ ] Build / CI / CD

## How this was verified

<!-- Commands you ran and what you observed. Screenshots or request/response samples for API changes. -->

```
dotnet format VioletManager.sln --verify-no-changes
dotnet build VioletManager.sln -c Release
dotnet test VioletManager.sln -c Release
```

## Checklist

- [ ] `dotnet format VioletManager.sln --verify-no-changes` passes
- [ ] `dotnet build VioletManager.sln -c Release` passes with no new warnings
- [ ] `dotnet test VioletManager.sln -c Release` passes, and new behaviour is covered by tests
- [ ] Targets the right branch (`develop` unless this is a release promotion)
- [ ] Public API / configuration changes are documented
- [ ] No secrets, tokens, connection strings or personal data in the diff

## Deployment notes

<!--
Anything that must happen around the merge. Delete the lines that don't apply.
-->

- New or changed secrets/variables (name them; never paste values):
- Migrations or manual steps:
- Rollback plan:
- [ ] Safe to deploy without coordination
