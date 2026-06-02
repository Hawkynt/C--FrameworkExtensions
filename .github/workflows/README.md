# CI/CD Pipeline — C# Framework Extensions

Event-driven pipeline (no cron). Workflows live here; their helper scripts live
in `scripts/`.

| File | Trigger | Purpose |
|------|---------|---------|
| `ci.yml` | push + PR on `master` + `workflow_call` | Build cross-platform packages on Linux; run the test suite on Windows |
| `release.yml` | **manual dispatch** | Run CI, pack + **push** packages to NuGet, then cut the dated `vyyyyMMdd` Release |
| `nightly.yml` | successful CI on `master` + manual | Publish `nightly-yyyyMMdd` prerelease (no NuGet push) and prune old ones |
| `_build.yml` | `workflow_call` (internal) | Pack the 6 publishable packages; optionally push to NuGet |
| `scripts/version.pl` | invoked by workflows | Stamp each package's own `<Version>` + commit count into its csproj (`--stamp`) |
| `scripts/update-changelog.mjs` | invoked by workflows | Bucketise commits into `CHANGELOG.md` |
| `scripts/prune-nightlies.mjs` | invoked by workflows | GFS retention: 7 daily + 4 weekly + 3 monthly |

```
        push / PR (master)
              │
              ▼
        ┌───────────┐   build on ubuntu (6 cross-platform packages)
        │  ci.yml   │   + test on windows (multi-SDK x cfg x arch)
        └─────┬─────┘
 dispatch ───┤────────── on success on master
        ▼     │           ▼
  ┌──────────┐│      ┌───────────┐
  │ release  ││      │ nightly   │
  └────┬─────┘│      └─────┬─────┘
       │   (both call _build.yml to pack)
       ▼                   ▼
  push to NuGet +     nightly-yyyyMMdd
  tag vyyyyMMdd +  prerelease, then prune
  GitHub Release
```

## Notes

- **Tests are Windows-only on purpose** — the test runner misbehaves on ubuntu
  (not our code). Carried over from the original `Tests.yml`.
- **Packages built/published:** Backports, Corlib, PresentationCore,
  System.Drawing, System.Windows.Forms, DirectoryServices. ASP.NET / Office /
  Win32 are intentionally not built here (Windows-only surfaces).
- **`secrets.NUGET_TOKEN`** must be set in repo settings for `release.yml` to
  push to nuget.org.
- **Versioning — files drive, never tags.** Each package carries its **own**
  `<Version>` in its csproj; `version.pl --stamp` appends the git commit count,
  so packages version **independently** (e.g. `Corlib 1.0.2.N`, others `1.0.0.N`)
  and bump as each one changes. There is **no `VERSION` file** and no single repo
  version — so the repo-level GitHub Release/tag is the date marker
  `vyyyyMMdd`. To bump a package, edit its csproj `<Version>`; the build
  number follows the commit count automatically.
- **`version.pl` is identical in every repo.** For a non-.NET repo (no csproj) it
  falls back to a root `VERSION` file — the only place that file is ever used.
- **Changelogs are automatic:** nightlies and releases generate their notes from
  commits; `release.yml` also refreshes and commits `CHANGELOG.md`.
- **Manual vs automatic:** stable releases (NuGet packaging + publish) are cut on
  demand via dispatch; nightlies and changelog notes happen automatically.
