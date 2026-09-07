<#
.SYNOPSIS
  Build, package and publish a playable Funstra demo in one command.

.DESCRIPTION
  This is the only manual step in the pipeline, and it is meant to be run by an
  agent as the last action of finishing a demo. It builds the Windows player,
  verifies the output is actually launchable, zips the portable package,
  publishes it as a GitHub Release and prunes old releases.

  Publishing the release fires the Site workflow, which regenerates the download
  page from the live release list. Nothing else has to be touched.

.EXAMPLE
  pwsh Tools/Release.ps1 -Name "A Bed & a Bandage" -NotesFile BED-AND-BANDAGE.md

.EXAMPLE
  pwsh Tools/Release.ps1 -SkipBuild -DryRun   # package an existing build, publish nothing
#>
[CmdletBinding()]
param(
    # Release version. Defaults to bundleVersion in ProjectSettings.
    [string]$Version,

    # Human title for the release, e.g. "A Bed & a Bandage".
    [string]$Name,

    # Markdown file used as the release notes; also becomes the blurb on the site.
    [string]$NotesFile,

    # Build folder under Build/ to package. Defaults to the newest one with a player in it.
    [string]$BuildDir,

    # Fixed owner policy; retained for compatibility with existing -Keep 5 calls.
    [ValidateSet(5)][int]$Keep = 5,

    # Package and publish whatever is already in Build/.
    [switch]$SkipBuild,

    # Mark the release as a prerelease.
    [switch]$PreRelease,

    # Do everything except publish.
    [switch]$DryRun,

    [string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe'
)

$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
Set-Location -LiteralPath $project

function Step($text) { Write-Host "`n== $text" -ForegroundColor Cyan }
function Note($text) { Write-Host "   $text" -ForegroundColor DarkGray }

# ---------------------------------------------------------------- 1. version

if (-not $Version) {
    $settings = Get-Content -LiteralPath 'ProjectSettings/ProjectSettings.asset' -Raw
    if ($settings -notmatch '(?m)^\s*bundleVersion:\s*(\S+)\s*$') {
        throw 'Could not read bundleVersion from ProjectSettings.asset; pass -Version.'
    }
    $Version = $Matches[1]
}
if ($Version -notmatch '^\d+\.\d+\.\d+') { throw "Version '$Version' is not semver-shaped." }
$tag = "v$Version"
Step "Releasing $tag"

# ------------------------------------------------------------------ 2. build

if ($SkipBuild) {
    Note 'Skipping the Unity build (-SkipBuild).'
} else {
    Step 'Building the Windows player'
    & (Join-Path $PSScriptRoot 'Build.ps1') -Unity $Unity
    if ($LASTEXITCODE -ne 0 -and $null -ne $LASTEXITCODE) { throw "Build.ps1 failed ($LASTEXITCODE)." }
}

# ------------------------------------------------------- 3. locate and verify

if (-not $BuildDir) {
    $candidate = Get-ChildItem -LiteralPath (Join-Path $project 'Build') -Directory -ErrorAction Stop |
        Where-Object { Test-Path (Join-Path $_.FullName 'Funstra.exe') } |
        Sort-Object LastWriteTime -Descending |
        Select-Object -First 1
    if (-not $candidate) { throw 'No folder under Build/ contains Funstra.exe.' }
    $BuildDir = $candidate.FullName
} elseif (-not [System.IO.Path]::IsPathRooted($BuildDir)) {
    $BuildDir = Join-Path $project $BuildDir
}

Step "Verifying $([System.IO.Path]::GetFileName($BuildDir))"
foreach ($required in @('Funstra.exe', 'UnityPlayer.dll', 'Funstra_Data')) {
    $path = Join-Path $BuildDir $required
    if (-not (Test-Path -LiteralPath $path)) { throw "Build is incomplete: $required is missing from $BuildDir." }
}
$player = Get-Item (Join-Path $BuildDir 'Funstra.exe')
if ($player.Length -lt 100KB) { throw "Funstra.exe is only $($player.Length) bytes; that build did not succeed." }
Note "Player built $($player.LastWriteTime.ToString('yyyy-MM-dd HH:mm'))"

$buildResult = Join-Path $project 'Evidence/build-result.txt'
if (Test-Path -LiteralPath $buildResult) {
    $result = Get-Content -LiteralPath $buildResult -Raw
    if ($result -notmatch 'Succeeded') { throw "Evidence/build-result.txt does not report success:`n$result" }
    Note ($result.Trim() -replace "`r?`n", ' · ')
}

# ---------------------------------------------------------------- 4. package

Step 'Packaging the portable zip'
$slug = ([System.IO.Path]::GetFileName($BuildDir) -creplace '(?<!^)([A-Z])', '-$1').ToLowerInvariant()
$releases = Join-Path $project 'Releases'
New-Item -ItemType Directory -Force -Path $releases | Out-Null
$zip = Join-Path $releases "Funstra-$slug-windows.zip"
if (Test-Path -LiteralPath $zip) { Remove-Item -LiteralPath $zip -Force }

Compress-Archive -Path (Join-Path $BuildDir '*') -DestinationPath $zip -CompressionLevel Optimal
$zipItem = Get-Item -LiteralPath $zip
if ($zipItem.Length -lt 1MB) { throw "$($zipItem.Name) is only $($zipItem.Length) bytes." }
$hash = (Get-FileHash -LiteralPath $zip -Algorithm SHA256).Hash.ToLowerInvariant()
Note "$($zipItem.Name) · $([math]::Round($zipItem.Length / 1MB, 1)) MB"
Note "sha256 $hash"

# ------------------------------------------------------------------ 5. notes

if (-not $Name) { $Name = "Funstra $Version" }

$notes = Join-Path ([System.IO.Path]::GetTempPath()) "funstra-notes-$Version.md"
if ($NotesFile) {
    if (-not (Test-Path -LiteralPath $NotesFile)) { throw "Notes file not found: $NotesFile" }
    # Take the prose above the first level-2 heading: the summary, not the whole guide.
    $raw = Get-Content -LiteralPath $NotesFile -Raw
    $body = ($raw -split '(?m)^##\s')[0].Trim()
    $body = ($body -replace '(?m)^#\s+.*$', '').Trim()
} else {
    $body = "Playable Windows demo, version $Version. This is a development prototype, not a finished game."
}
@"
$body

**Download** ``$($zipItem.Name)`` below, extract the whole archive and run ``Funstra.exe``.
Unsigned build: Windows SmartScreen will warn on first launch.

- Version: $Version
- Platform: Windows x64, Unity 6000.4.0f1
- SHA-256: ``$hash``
- Source: MIT licensed, built from this tag
"@ | Set-Content -LiteralPath $notes -Encoding utf8

# ---------------------------------------------------------------- 6. publish

if ($DryRun) {
    Step 'Dry run — nothing published'
    Note "Would create release $tag ""$Name"" with $($zipItem.Name)"
    Note "Notes drafted at $notes"
    return
}

Step "Publishing release $tag"
$existing = & gh release view $tag --json tagName 2>$null
if ($LASTEXITCODE -eq 0 -and $existing) {
    Note 'Release exists; replacing its notes and asset.'
    & gh release edit $tag --title $Name --notes-file $notes
    if ($LASTEXITCODE -ne 0) { throw 'gh release edit failed.' }
    & gh release upload $tag $zip --clobber
    if ($LASTEXITCODE -ne 0) { throw 'gh release upload failed.' }
} else {
    $arguments = @('release', 'create', $tag, $zip, '--title', $Name, '--notes-file', $notes, '--latest')
    if ($PreRelease) { $arguments += '--prerelease' }
    & gh @arguments
    if ($LASTEXITCODE -ne 0) { throw 'gh release create failed.' }
}

# ------------------------------------------------------------------ 7. prune

Step 'Retaining the newest five published releases'
& node (Join-Path $PSScriptRoot 'Prune-Releases.mjs')
if ($LASTEXITCODE -ne 0) { throw 'Release retention failed; inspect GitHub before declaring publication complete.' }

Step 'Done'
Note "The Site workflow is now regenerating the download page."
Note (& gh release view $tag --json url --jq '.url')
