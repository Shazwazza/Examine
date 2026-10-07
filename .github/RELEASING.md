# Releasing

Pushing a `v*` tag runs `.github/workflows/build.yml`. After the build, test and pack steps succeed, the `publish` job waits for approval in the `nuget` environment. Once it's approved, the job:

1. Pushes the `.nupkg` packages and their `.snupkg` symbol packages to nuget.org. It uses `--skip-duplicate`, so a re-run is safe.
2. Creates a GitHub Release for the tag with generated notes and the packages attached. If the version has a prerelease suffix (for example `-beta.1`), the release is marked as a prerelease.

If you don't approve the deployment, or you reject it, nothing is published.

## Release steps

1. Tag the commit: `git tag v3.x.y && git push origin v3.x.y`
2. Open the workflow run in the Actions tab, then **Review deployments** → `nuget` → **Approve and deploy**.

## One-time setup

1. **GitHub environment**: Settings → Environments → **New environment**, named `nuget`.
   - Under **Required reviewers**, add yourself or any maintainers who can approve.
   - Under **Deployment branches and tags**, choose **Selected branches and tags** and add a tag rule `v*`.
   - Under **Environment variables**, add `NUGET_USER` with your nuget.org profile name. This is the profile name, not an email.
2. **NuGet trusted publishing**: on nuget.org, go to your account → **Trusted Publishing** → add a policy:
   - Repository owner: `Shazwazza`
   - Repository: `Examine`
   - Workflow file: `build.yml`
   - Environment: `nuget`

The workflow uses OIDC (`NuGet/login`) to exchange the GitHub token for a short-lived NuGet API key, so no API key secret is stored in the repository.
