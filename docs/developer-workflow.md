# Developer workflow

## Local quality gates

Repo uses **Husky.Net** for local Git hooks.

- `pre-commit` runs:
  - `gitleaks git --staged`
  - `dotnet restore Tesseract.slnx`
  - `dotnet format Tesseract.slnx --verify-no-changes --no-restore`
  - `dotnet build Tesseract.slnx --configuration Release --no-restore`
  - `dotnet test Tesseract.slnx --configuration Release --no-build`
- Repo-level `.gitleaks.toml` ignores `matrix-docs.md`, because file contains protocol examples that trigger false positives.
- `commit-msg` auto-adds ticket prefix from branch name when missing.
- `pre-push` blocks direct pushes to `main` and `master`.

### Commit message rule

Branch example:

```text
13-fix-navigation
```

Commit message after hook:

```text
[ES-13] Fix dropdown menu
```

Also supported:

- `feature/13-fix-navigation`
- `feat/ES-13-domain-discovery-information`

Merge, revert, `fixup!`, and `squash!` commits bypass auto-prefix.

## Setup on macOS

```bash
brew install gitleaks
dotnet tool restore
dotnet husky install
```

Run infrastructure only:

```bash
docker compose up -d postgres redis
```

Run full stack:

```bash
docker compose up --build
```

## Setup on Windows

```powershell
choco install gitleaks -y
dotnet tool restore
dotnet husky install
```

Run infrastructure only:

```powershell
docker compose up -d postgres redis
```

Run full stack:

```powershell
docker compose up --build
```

## Docker defaults

- Postgres host for local app: `localhost`
- Postgres host inside Compose network: `postgres`
- Database: `tesseract`
- Username: `admin`
- Password: `admin`
- Redis: `localhost:6379`

`launchSettings.json` matches local Docker credentials for local `dotnet run` against Compose-backed Postgres.

## GitHub Actions

Workflows added:

- `Pull request validation`
  - gitleaks
  - restore
  - format + analyzers
  - build
  - tests
- `Pull request validation comment`
  - updates one sticky PR comment with pass/fail summary

## Protect `main` on GitHub

Local hook blocks direct push, but real remote protection must be enabled in GitHub repository settings.

Recommended branch protection / ruleset for `main`:

1. Require pull request before merging.
2. Require status checks to pass before merging.
3. Add required check: `Validate codebase`.
4. Restrict direct pushes to administrators only if team policy needs it.
