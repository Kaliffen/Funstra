# The Funstra pipeline

The operational reference for building and publishing. [README](../README.md#read-and-work) owns the documentation map; [DESIGN](../DESIGN.md) owns release scope. The [CD skill](../.agents/skills/funstra-creative-director/SKILL.md) and [reviewer skill](../.agents/skills/funstra-reviewer/SKILL.md) own agent instructions; `site/content/process.json` is the maintained public process summary.

The supervised loop is ticketed planning → identified tested candidate → four guided actual-build reviews → one dossier → CD integration/replay → publication verification → stop for owner direction. Human acceptance is recorded separately. A planning-only task ends at docs/tickets. Future release plans are not permission to execute every cycle automatically.

Before packaging, use Evidence/VALIDATION.md and the selected release's runner instructions for test commands and save isolation. A package completeness check does not replace gameplay validation, and the post-publication release-check workflow cannot establish that a game is playable.

## The cycle

```
  agent finishes a demo
          │
          ▼
  selected Tools/Package-*.ps1 ──── local, existing reviewed player
          │  verifies the exact assembly, evidence and dossier,
          │  packages without rebuilding
          ▼
  verify extracted archive, push matching source
          │  gh release create uploads that exact ZIP
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
# Validate the current reviewed player without creating an archive.
pwsh Tools/Package-PressureEscape.ps1 -ExpectedAssemblySha256 <reviewed-assembly-sha256> -ValidateOnly

# Create its review package without rebuilding. Extract and test it before upload.
pwsh Tools/Package-PressureEscape.ps1 -ExpectedAssemblySha256 <reviewed-assembly-sha256>

# After review integration and source push, upload the exact verified archive.
gh release create v0.4.2 Releases/Funstra-pressure-escape-0.4.2-windows.zip --target <pushed-source-commit> --title "Pressure and Escape" --notes-file Docs/release-notes-pressure-escape-0.4.2.md --latest

# Inspect retained releases without deleting anything.
node Tools/Prune-Releases.mjs --dry-run
```

Use the version and source commit belonging to the reviewed executable. Write complete release notes before publication, including coverage limits and the verified archive's size and SHA-256. The selected packager checks player files, successful build evidence, matching runtime/reviewer identities and required documents; it does not publish or replace the build. The generic `Tools/Release.ps1` remains available for other release flows, but must not rebuild or repackage an already-reviewed archive.

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

## Police Response reviewed package

`Tools/Package-PoliceResponse.ps1` validates and packages the existing v0.4.1 player, its three core runtime routes and four guided reviewer records. The extracted package has its own runtime receipt under `Evidence/PoliceResponse/portable-check`. Publish the already-verified `Releases/Funstra-police-response-0.4.1-windows.zip` with `gh release create v0.4.1 <archive> --target <pushed-source-commit> --title "Police Response" --notes-file Docs/release-notes-police-response-0.4.1.md --latest`. Do not invoke the generic repackaging path on this reviewed archive.

The site generator relocates the selected and previous dossiers with their locally linked evidence. It rewrites only published HTML, preserving original dossiers and reviewer records. `node site/test-build.mjs` tests relocation and containment; `node site/verify.mjs` checks generated local targets alongside release retention and previous-version links.

## Pressure and Escape reviewed package

`Tools/Package-PressureEscape.ps1` packages the existing `Build/PressureEscape` v0.4.2 player. It requires the same assembly SHA-256 in the Pressure, Police, Streets and Legacy core routes under `Evidence/PressureEscape/{pressure,police,streets,legacy}-final`, the sustained `pressure-profile.json`, all four `review-*` Pressure runs and each reviewer's `pressure-escape*.md` records. It includes `PRESSURE-AND-ESCAPE.md`, the single `Docs/funstra-review-dossier-pressure-escape.html`, validation, credits and linked review artifacts. Pending review records or missing final evidence are packaging blockers, not placeholder release content.

After the integrated candidate and four-reviewer dossier are complete, validate and package with the commands above. Extract the ZIP to a separate directory, check its assembly against the reviewed hash and run the Pressure route using `-PlayerPath <extracted-folder>/Funstra.exe -EvidenceRoot Evidence/PressureEscape/portable-check`. Record the extracted receipt and ZIP hash before uploading that exact ZIP with `gh release create`. Preserve the approved v0.4.1 archive and dossier.

Finalize `site/content/site.json` with actual panel verdicts, the selected dossier and `releaseVersion: v0.4.2` before building the site. While review is pending, its prepared content must not be deployed. Build and inspect the completed site only after the final dossier exists; `node site/verify.mjs` rejects a newest release that differs from the advertised version. After publication, verify the live index, dossier, evidence links, retained releases and downloaded archive hash. A successful workflow alone does not complete those public checks.
