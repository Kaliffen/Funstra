# The Funstra pipeline

How a finished demo becomes a download on the website, and what runs where.

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
exists. `release-check.yml` is the gate that keeps a bad local build from being
advertised.

If this ever needs to move into CI, the route is GameCI's `game-ci/unity-builder`
with `UNITY_LICENSE`, `UNITY_EMAIL` and `UNITY_PASSWORD` as secrets, and the local
script becomes a thin `workflow_dispatch` trigger.

## Releasing

```powershell
# The normal case: build, package, publish, prune.
pwsh Tools/Release.ps1 -Name "A Bed & a Bandage" -NotesFile BED-AND-BANDAGE.md

# Rehearse without publishing.
pwsh Tools/Release.ps1 -DryRun

# Package a build that already exists.
pwsh Tools/Release.ps1 -SkipBuild -Name "Hot Cargo"
```

The version comes from `bundleVersion` in `ProjectSettings.asset` unless you pass
`-Version`. The tag is `v<version>`. Release notes are the prose above the first
`##` heading of `-NotesFile`, plus the download instructions, size and SHA-256.

The script refuses to publish when `Funstra.exe`, `UnityPlayer.dll` or
`Funstra_Data` are missing, when the player is implausibly small, or when
`Evidence/build-result.txt` does not say `Succeeded`.

### Keeping only the newest releases

`-Keep 5` (the default) deletes older releases and their tags after a successful
publish, so autopublishing does not accumulate dozens of 38 MB archives. Pass
`-Keep 0` to disable pruning. The site lists whatever survives; the "Earlier
builds" section shrinks with it.

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
