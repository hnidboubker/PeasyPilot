# CI/CD Workflows

This document describes the GitHub Actions workflows used in PeasyPilot for continuous integration and deployment.

## Overview

PeasyPilot uses multiple GitHub Actions workflows to ensure code quality, test coverage, and reliable releases.

| Workflow | File | Trigger | Purpose |
|----------|------|---------|---------|
| Build and Test | `build-and-test.yml` | Push/PR on main, develop, phase/* | Run comprehensive tests on multiple .NET versions |
| Coverage | `coverage.yml` | Push on main | Generate and report code coverage |
| Publish | `publish.yml` | Release tag | Publish packages to NuGet |
| Release | `release.yml` | Release tag | Create GitHub releases with notes |
| Test Failures | `test-failures-report.yml` | build-and-test completion | Report test failures as GitHub issues |

## Detailed Workflows

### Build and Test (`build-and-test.yml`) ⭐

**Primary workflow** - Validates every push and pull request.

**Triggers:**
- Push to `main`, `develop`, or `phase/**` branches
- Pull requests targeting `main` or `develop`

**Matrix Testing:**
- .NET 8.0
- .NET 9.0
- .NET 10.0

**Steps:**
1. **Checkout** – Clone the repository
2. **Setup .NET** – Install the specified .NET version
3. **Restore** – Restore NuGet dependencies
4. **Build** – Compile the solution (Release configuration)
   - `continue-on-error: true` – Doesn't block test step if build has warnings
5. **Run Tests** – Execute all tests with console output logging
   - Output captured to `test-output.log`
   - `continue-on-error: true` – Allows failure capture for reporting
6. **Capture Test Output** – Extract last 50 lines of test results
7. **Check for Test Failures** – Analyze output for failures
   - Creates `.test-errors/test-failures.md` if failures detected
8. **Upload Test Failures** – Archive failure report as artifact
9. **Display Failure Report** – Print failures to console

**Secondary Job - Logs:**
- Runs if workflow completes (success or failure)
- Downloads test failure artifacts
- Prints CI/CD execution summary

### Coverage (`coverage.yml`)

**Triggers:**
- Push to `main` branch

**Purpose:**
- Generate code coverage reports (Cobertura XML format)
- Upload artifacts to coverage services
- Track coverage trends

### Publish (`publish.yml`)

**Triggers:**
- GitHub release creation with tag

**Purpose:**
- Build all packages in Release mode
- Push `.nupkg` and `.snupkg` files to NuGet
- Skip duplicate versions

### Release (`release.yml`)

**Triggers:**
- GitHub release creation with tag

**Purpose:**
- Auto-generate release notes from commits
- Attach NuGet packages (`.nupkg`, `.snupkg`)
- Create professional release on GitHub

### Test Failures (`test-failures-report.yml`)

**Triggers:**
- When `build-and-test.yml` completes

**Purpose:**
- Parse test failure artifacts from build job
- Create GitHub issues for failures
- Label issues automatically with `test-failure` and `automated`

## Best Practices

### Branch Naming
- `main` – Production-ready code
- `develop` – Integration branch for features
- `phase/**` – Feature branches with automated testing
- Feature branches do NOT trigger workflows unless prefixed with `phase/`

### Pull Requests
- Always target `main` or `develop`
- Workflows run automatically
- Must pass all checks before merge

### Tags and Releases
- Tag format: `v*.*.*` (e.g., `v1.2.3`)
- Push tag → Publish and Release workflows trigger
- Packages published to NuGet automatically

### Error Handling
- Build failures do NOT block test execution
  - Use `continue-on-error: true` in build step
  - Allows analysis of test results even with build warnings
- Test failures do NOT block workflow completion
  - Captured and reported
  - Issues created for tracking

## Monitoring

### Check Workflow Status
1. Go to **Actions** tab in GitHub repository
2. Click on workflow name to see recent runs
3. Click on specific run to see logs

### Common Issues

**Build fails but tests run:**
- This is expected with `continue-on-error: true`
- Check build output for warnings/errors
- Tests may still pass if compilation errors are in test infrastructure

**Tests timeout:**
- Check Ubuntu runner logs (test step)
- May indicate infinite loop or hanging async operation

**Coverage not updating:**
- Verify `.github/workflows/coverage.yml` is correct
- Check if push is to `main` branch

**Publish fails:**
- Verify NuGet API token is valid
- Check `.nupkg` file names match expected format
- Ensure version is not already published

## Maintenance

### Updating Workflow Actions
Workflows use pinned action versions for reproducibility:
```yaml
- uses: actions/checkout@v4
- uses: actions/setup-dotnet@v4
- uses: actions/upload-artifact@v4
```

To update:
1. Check GitHub Actions marketplace for latest version
2. Update version in workflow file
3. Test in feature branch before merging to main

### Adding New Steps
When adding steps to workflows:
1. Test locally with `act` tool (if possible)
2. Create feature branch for workflow changes
3. Push to `phase/` branch to trigger test run
4. Verify workflow succeeds before merging

## Example: Local Testing with `act`

You can simulate workflows locally using the `act` tool:

```bash
# Install act: https://github.com/nektos/act

# Run build-and-test workflow locally
act push -j build-and-test

# Run specific workflow
act -l  # List all workflows
```

## Secrets and Environment Variables

### Required Secrets
- `NUGET_API_KEY` – For publishing to NuGet (set in repository settings)

### Environment Variables
Set in workflow files:
```yaml
env:
  SOLUTION: easy-peasy.slnx
  CONFIGURATION: Release
```

## Future Improvements

- [ ] Add performance benchmarking workflow
- [ ] Add security scanning (SAST)
- [ ] Add dependency update checks (Dependabot)
- [ ] Add code quality checks (SonarQube)
- [ ] Add load testing workflow
