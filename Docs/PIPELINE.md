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
# Demo06: use the hash recorded by final validation and the panel.
$reviewedAssemblySha256 = (Get-Content -LiteralPath Evidence/Demo06/validation-summary.json -Raw | ConvertFrom-Json).assemblySha256
pwsh Tools/Package-Crew.ps1 -ExpectedAssemblySha256 $reviewedAssemblySha256 -ValidateOnly

# Create its review package without rebuilding. Extract and test it before upload.
pwsh Tools/Package-Crew.ps1 -ExpectedAssemblySha256 $reviewedAssemblySha256

# After review integration and source push, upload the exact verified archive.
gh release create v0.6.0 Releases/Funstra-nobody-gets-home-alone-0.6.0-windows.zip --repo Kaliffen/Funstra --target <pushed-source-commit> --title "Nobody Gets Home Alone" --notes-file Docs/release-notes-demo06-0.6.0.md --latest

# Inspect retained releases without deleting anything.
node Tools/Prune-Releases.mjs --dry-run
```

These are pending release operations, not a claim that Demo06 has passed or been published. Prepare the named notes file after the review outcome is known. Replace the source placeholder with the pushed commit containing the gameplay code that produced the reviewed executable. Record the build's source commit and dirty state separately from its binary hash: the current runners and packager do not prove source-to-binary correspondence. Documentation-only commits after the build do not require rebuilding the reviewed player; any gameplay change requires a newly identified candidate and affected checks/reviews.

Write complete release notes before publication, including coverage limits and the verified archive's size and SHA-256 from `Evidence/Demo06/package-verification.json`. The selected packager checks player files, successful build evidence, matching runtime/reviewer identities and required documents; it does not publish or replace the build. Do not use `Tools/Release.ps1` for this reviewed archive: even `-SkipBuild` creates another package, and its default path can select a different build.

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

For that historical increment, use `Package-PressureEscape.ps1` after its integrated candidate and four-reviewer dossier are complete. Extract the ZIP to a separate directory, check its assembly against the reviewed hash and run the Pressure route using `-PlayerPath <extracted-folder>/Funstra.exe -EvidenceRoot Evidence/PressureEscape/portable-check`. Record the extracted receipt and ZIP hash before uploading that exact ZIP with `gh release create`. Preserve historical local archives and dossiers.

Finalize `site/content/site.json` with actual panel verdicts, the selected dossier and `releaseVersion: v0.4.2` before building the site. While review is pending, its prepared content must not be deployed. Build and inspect the completed site only after the final dossier exists; `node site/verify.mjs` rejects a newest release that differs from the advertised version. After publication, verify the live index, dossier, evidence links, retained releases and downloaded archive hash. A successful workflow alone does not complete those public checks.

## Demo06 reviewed package and publication

`Tools/Package-Crew.ps1` reads the existing `Build/NobodyGetsHomeAlone` player. It requires fresh matching assembly identities, PASS files, complete logs and screenshots under `Evidence/Demo06/{crew,approach,arms,residents,streets,police,pressure,legacy}-final`. Pressure additionally requires `pressure-profile.json`. It also requires the frozen `review-outcomes.json`, `validation-summary.json`, sanitized `editor-checks.txt`, all four `review-{dag,priya,marcus,nell}` Crew runs, each persona's `demo06.md` and `demo06-meeting.md` with that candidate hash, and `Docs/funstra-review-dossier-demo06.html` identifying the same assembly. It verifies every packaged player file by SHA-256. `-ValidateOnly` writes no archive; `-Force` replaces an existing ZIP only after validation. Preserve an earlier candidate archive separately before replacement when it is still needed as evidence.

Use `Test-Crew.ps1 -ApproachOnly` for the complete approach route. `-FightOnly`, `-OppositionOnly` and `-MovementOnly` are focused diagnostics and must not substitute for `approach-final` or a complete reviewer Crew run. They share the `Crew` identity mode, so the packager also checks representative full-route result assertions. Both unseen theft branches must return the same component and survive reload. Full-crew combat must show actual three-gun use and yard hostility, then record a recognized outcome in `crew-combat-outcome.json`: component returned, survivor withdrawn with the operation unresolved, or total defeat with existing emergency recovery. The gate retains raw before/after positions and requires exact identities, custody, wounds, ammunition and coordinates except the existing resume placement: an uncarried protagonist may move upward by 0 through 0.1201 world units. Player X/Z, every companion coordinate and a carried protagonist remain exact. A mission loss is valid evidence and must not be reported as completion. Inspect that receipt, `crew-approach-evidence.json` and method/result text before crediting full coverage. The Demo06 pressure gate uses `Test-Streets.ps1 -Mode Pressure -WithCrew`; a baseline Pressure run or `-ReplaySnapshot` is supporting evidence. Packaging verifies the full stress samples and actual rifle-dispatch assertions.

After packaging, extract the ZIP to a new directory, compare its gameplay assembly with `$reviewedAssemblySha256`, then run the same binary through isolated portable checks:

```powershell
pwsh Tools/Test-Crew.ps1 -PlayerPath <extracted-folder>/Funstra.exe -EvidenceRoot Evidence/Demo06/portable-crew
```

The full portable Crew route checks extracted launch, gameplay resources and save/reload. The package's every-file hash comparison ties the wider validated routes to these same player bytes; rerun additional routes if extraction exposes a new issue. Preserve the ZIP from which the check ran. Record its receipt and archive hash without repackaging to insert later receipts. Push matching source, full reviews, dossier and required evidence links before the release command above. Repository remotes must still resolve to `Kaliffen/Funstra`; source tags and older review dossiers are retained.

For the website, update `site/content/site.json` only with actual review findings: set `releaseVersion` to `v0.6.0`, `dossierFile` to `funstra-review-dossier-demo06.html`, and retain the former selected dossier in `previousDossiers`. All HTML evidence links must resolve to checked-in local files. The current generator can preview from its existing release cache with `node site/build.mjs --offline`; this does not create a release or prove that the public download matches. Do not fabricate a published release in the cache. If the source is pushed before the release exists, the version-matching verification deliberately prevents a premature deployment; the release event triggers a fresh build after upload.

After publishing the exact ZIP, run and inspect:

```powershell
node Tools/Prune-Releases.mjs
node site/build.mjs
node site/verify.mjs
gh release view v0.6.0 --repo Kaliffen/Funstra --json tagName,name,isDraft,assets,url
gh run list --repo Kaliffen/Funstra --workflow release-check.yml --limit 5
gh run list --repo Kaliffen/Funstra --workflow site.yml --limit 5
```

The Site workflow itself also enforces the latest-five retention policy. If its event did not run, dispatch `gh workflow run site.yml --repo Kaliffen/Funstra --ref main` after the release and matching site source exist. Verify the actual live index and `dossier.html` at `https://kaliffen.github.io/Funstra/`, its evidence links, `releases.json`, Previous versions and the release notes. Download the public ZIP to a fresh directory and compare it with the recorded archive SHA-256. Close publication only after those checks; keep human acceptance separate.

### Demo06 presentation integration after the original panel

The original eight-route validation and four independent reviews identify assembly A, recorded in `Evidence/Demo06/candidate-source.json`. Preserve that manifest, all original results and reviewer scores. The reviewed build exposed three display defects in `FunstraUI.cs`: the idle-officer marker, Mara's attribution dash, and the stale title footer.

A narrow integration export B may change only those three pinned expressions. `review-source/FunstraUI.cs` retains A's exact source bytes; `ui-replacements.json` lists the exact old/new expressions; `integration-source.json` records all 226 B source hashes. Unity also regenerates the scene's editor object identifiers on export. A conditional exception for that scene source requires full 145-file A/B player manifests: every compiled scene, asset, executable and engine file must be byte-identical; only the proven managed DLL and a boot configuration differing solely in its build GUID may differ. Original scene YAML equality is not claimed. The other 224 source files must match A exactly. `integration-validation.json` ties both assemblies to these proofs. The packager rejects any other source or runtime asset change. This exception is specific to this identified candidate and these corrections.

Run full Crew, Police and Legacy routes on B under `integration-crew`, `integration-police` and `integration-legacy`. Inspect corrected title, police markers and ending attribution; Marcus and Priya record the affected rendered replays in `demo06-replay.md`. B's three routes and unchanged-source proof accompany A's eight full routes. Do not call the five unrerun routes B results or overwrite original scores.

Package with `Tools/Package-Crew.ps1 -ExpectedAssemblySha256 <B> -ReviewedAssemblySha256 <A>`. The current player, fresh successful build result, B route receipts and original A evidence are all required. Extract and run the resulting B package before publishing those exact bytes. The dossier and final validation index identify both builds and the specific coverage split. Any change outside the pinned presentation correction requires a new validation/review decision; this is not a general stale-evidence bypass.
