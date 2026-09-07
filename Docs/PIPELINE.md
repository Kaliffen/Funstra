# The Funstra pipeline

The operational reference for building and publishing. [README](../README.md#read-and-work) owns the documentation map; [DESIGN](../DESIGN.md) owns release scope. The [CD skill](../.agents/skills/funstra-creative-director/SKILL.md) and [reviewer skill](../.agents/skills/funstra-reviewer/SKILL.md) own agent instructions; `site/content/process.json` is the maintained public process summary.

The supervised loop is ticketed planning → identified tested candidate → four guided actual-build reviews → one dossier → CD integration/replay → publication verification → stop for owner direction. Human acceptance is recorded separately. A planning-only task ends at docs/tickets. Future release plans are not permission to execute every cycle automatically.

Before packaging, use Evidence/VALIDATION.md and the selected release's runner instructions for test commands and save isolation. A package completeness check does not replace gameplay validation, and the post-publication release-check workflow cannot establish that a game is playable.

## The cycle

```
  agent finishes a demo
          │
          ▼
  Tools/Release.ps1  ──── local, Windows, licensed Unity editor
          │  builds the player, verifies it, zips it,
          │  writes notes, publishes a GitHub Release,
          │  prunes to the newest 5 releases
          ▼
  GitHub Release published
          │
          ├──▶ .github/workflows/release-check.yml
          │      is a real zip attached? notes long enough?
          │      semver tag? fails loudly if not
          │
          └──▶ .github/workflows/site.yml
                 node site/build.mjs reads the live release list
                 and regenerates site/dist, then deploys to Pages
                          │
                          ▼
                 https://kaliffen.github.io/Funstra/
```

Nobody edits the download page. The newest release *is* the page.

## Why the Unity build is local

A batch-mode Unity build needs an activated editor licence. GitHub's free runners
have neither, and putting a Unity serial into repository secrets on a public repo
is not something worth doing for a hobby prototype. So the build runs on the
machine that already has the editor, and CI takes over the moment the artifact
exists. `release-check.yml` checks release structure after publication. Local candidate validation and review must establish playability before publishing.

If this ever needs to move into CI, the route is GameCI's `game-ci/unity-builder`
with `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD` as secrets, and the local
script becomes a thin `workflow_dispatch` trigger.

## Releasing

```powershell
# Publish an already-tested player; verify packaged identity before release.
# Use a release-specific packaging path when guides/evidence are required.
pwsh Tools/Release.ps1 -SkipBuild -Name "<reviewed release title>" -NotesFile "<prepared notes file>" -BuildDir "<tested build directory>"

# Rehearse without publishing.
pwsh Tools/Release.ps1 -DryRun

# Inspect retained releases without deleting anything.
node Tools/Prune-Releases.mjs --dry-run
```

The version comes from `bundleVersion` in `ProjectSettings.asset` unless you pass
`-Version`. The tag is `v<version>`. Release notes are the prose above the first
`##` heading of `-NotesFile`, plus the download instructions, size and SHA-256.

The script refuses to publish when `Funstra.exe`, `UnityPlayer.dll` or
`Funstra_Data` are missing, when the player is implausibly small, or when
`Evidence/build-result.txt` does not say `Succeeded`.

### Keeping only the newest releases

The owner-approved policy is exactly the latest **five published releases**, or all available releases while fewer than five exist. Sort by publication date, including published prereleases and excluding drafts. `Tools/release-policy.mjs` supplies the same selection to the site and `Tools/Prune-Releases.mjs`.

The local release command and every non-PR Site deployment enforce GitHub retention, including releases published directly with `gh`. Older release records and attached assets are deleted; source tags, local builds, saves and review dossiers remain. Cleanup verifies the retained set and fails deployment if it cannot establish the required result. `-Keep 5` remains accepted; other values are rejected under this policy.

The current download plus up to four **Previous versions** entries appear on the site, with direct downloads and release notes. `releases.json` contains the same retained set. The download section links directly to the previous release and separately to the retained archive. Use `node Tools/Prune-Releases.mjs --dry-run` to inspect cleanup without writes.

## The website

| Path | What it is |
|---|---|
| `site/content/site.json` | All the prose. Edit this, not the HTML. |
| `site/assets/styles.css` | The design system, shared with the review dossier. |
| `site/build.mjs` | Generator. No dependencies; Node 20+. |
| `site/dist/` | Output. Git-ignored, rebuilt every deploy. |

```bash
node site/build.mjs             # fetch the live release list and build
node site/build.mjs --offline   # rebuild from site/.cache/releases.json
```

`site.yml` redeploys on a published release, on a push to `main` touching
`site/**`, on manual dispatch, and weekly (to refresh download counts). Pull
requests build the site and check the output but never deploy.

## One-time setup, already done

- Repository: public, MIT.
- Pages: **Settings → Pages → Source: GitHub Actions**.
- No secrets. The workflows use the automatic `GITHUB_TOKEN`; `Tools/Release.ps1`
  uses the developer's `gh` login.

## Demo 03 reviewed package

`Tools/Package-Demo03.ps1` packages the already-tested Demo 03 player together with its guide, validation, build identity, standing captures and single review dossier. It refuses results older than the gameplay assembly. Publish this archive with `gh release create v0.3.0 Releases/Funstra-demo-03-windows.zip --target <verified-source-commit> --title "Keep the Lights On" --notes-file <prepared-notes-file> --latest` after pushing the verified source. This invokes the same release-check and Pages workflows without rebuilding or replacing the richer package with a player-only zip. The Site workflow applies the five-release retention policy.

`site/content/site.json` selects the current versioned dossier through `dossierFile`, and lists prior dossiers in `previousDossiers`. Demo 02 retains its original file; Demo 03 has `Docs/funstra-review-dossier-demo03.html`. Each demo owns one dossier; the site's `dossier.html` is the published copy of the selected one.
