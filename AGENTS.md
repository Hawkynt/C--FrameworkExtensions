# Agent guide — C--FrameworkExtensions

Working agreement for **all** coding agents (Claude Code, Codex, Copilot, …)
and human contributors working in this repository. These rules are not
optional. The full house spec lives in the `Hawkynt/project-template` repo
(`STANDARD.md`); this file is the per-repo distillation.

## What this is

The **FrameworkExtensions** package family: BCL gap-fillers
(`Corlib.Extensions` et al.) and the `Backports` polyfills that let modern
C# run on runtimes back to .NET Framework 2.0. One folder per package, each
csproj carrying its **own `<Version>`** — untouched packages compose the
identical version on the next release, so `--skip-duplicate` re-uses the
published artifact instead of re-uploading. This repo's
[CONTRIBUTING.md](CONTRIBUTING.md) is the **canonical syntax/style guide**
for the whole Hawkynt family — read it before writing code here.

## Commits

- **Group changes semantically/logically** — one extension-area/package per
  commit; small changesets over huge ones.
- **Every subject line starts with a prefix**: `+` added · `-` removed ·
  `*` changed · `#` bug fixed · `!` critical todo.
- Never start a subject with "fix"/"bugfix"/"changed"/"modified".
- **No AI traces anywhere**: no `Co-Authored-By` AI lines, no "Generated
  with" footers, no agent mentions in messages, comments, or authorship.

## The loop (always, in this order)

1. **Before committing**: build the solution and run the required test tier
   under `Tests/` until green (equivalence classes + boundaries — "not too
   many, but enough"); performance-sensitive paths get benchmark coverage
   per CONTRIBUTING. Update the package table / Readme.md of the touched
   package when its surface changes; `CHANGELOG.md` is generated — never
   edit it by hand.
2. **Commit** (rules above) and **push**.
3. **Wait for CI**; on `main` a green CI triggers the nightly (prerelease +
   GFS prune, same-day replace). Fix and loop until everything is green.

Stable releases are **manual** (`gh workflow run release.yml`) — per-package
versions come straight from the csprojs; the repo-level tag is the dated
`vyyyyMMdd` marker. Never cut a release unless explicitly asked.

## Code conventions (CONTRIBUTING.md is canonical — highlights)

- `public static partial class TypeExtensions`, file per type
  (`Type.cs`), first parameter `@this`; folders mirror namespaces.
- Latest C# features behind `SUPPORTS_*`/feature conditions; T4 templates
  (`*.T4.tt`) for number-type fan-out; never depend on more than Backports
  + Corlib.Extensions.
- Guard clauses first (`Guard.Against.*`), throw-helpers in `AlwaysThrow`
  (`[DoesNotReturn]`, no inlining), validate public surface only.
- **YAGNI does not apply here** — you're enabling consumers you don't know;
  over-engineering for performance/memory is welcome, KISS on the public
  surface; never make code slower or hungrier while refactoring.

## README & repo conventions

- Standard frame: title → badges → one-line `>` blockquote; fixed emoji
  mapping for the standard sections (`## 🧩 Packages`, `## 🛠️ Building`,
  `## ❤️ Support`, `## 📜 License`); the package table (package · purpose ·
  NuGet badge) is the library-archetype anchor — keep it complete.
- License is LGPL-3.0-or-later; the `## ❤️ Support` section and
  `.github/FUNDING.yml` stay intact.
