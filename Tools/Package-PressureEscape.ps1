<#
.SYNOPSIS
Package the reviewed, already-tested Pressure and Escape player. Never builds or publishes.
.DESCRIPTION
Requires the assembly SHA-256 recorded for the reviewed candidate and fresh,
matching Pressure, Police, Streets and Legacy evidence. Screenshots and logs
are evidence, not a substitute for the four reviews or the CD's visual inspection.
Publish the resulting archive through the documented GitHub release path. The
Site workflow enforces the shared latest-five policy; this script does not prune
releases, source tags, earlier local builds, saves or dossiers.
.EXAMPLE
pwsh Tools/Package-PressureEscape.ps1 -ExpectedAssemblySha256 <reviewed-candidate-hash> -ValidateOnly
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-fA-F]{64}$')][string]$ExpectedAssemblySha256,
    [string]$DemoEvidence,
    [switch]$ValidateOnly,
    [switch]$Force
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$project=Split-Path -Parent $PSScriptRoot
$build=Join-Path $project 'Build\PressureEscape'
$evidence=Join-Path $project 'Evidence'
if(-not $DemoEvidence){$DemoEvidence=Join-Path $evidence 'PressureEscape'}
$assembly=Join-Path $build 'Funstra_Data\Managed\Assembly-CSharp.dll'
foreach($path in @($assembly,(Join-Path $build 'Funstra.exe'),(Join-Path $build 'UnityPlayer.dll'))){
    if(-not(Test-Path -LiteralPath $path -PathType Leaf)){throw "Required player file missing: $path"}
}
$hash=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash
if($hash -ne $ExpectedAssemblySha256){throw 'Player assembly differs from the identified tested candidate.'}
$assemblyTime=(Get-Item -LiteralPath $assembly).LastWriteTimeUtc
$buildResult=Join-Path $evidence 'build-result.txt'
if((Get-Content -LiteralPath $buildResult -TotalCount 1) -ne 'Succeeded' -or (Get-Item -LiteralPath $buildResult).LastWriteTimeUtc -lt $assemblyTime){throw 'No successful build result for the current assembly.'}

$routes=@(
    @{folder=(Join-Path $DemoEvidence 'pressure-final');result='pressure-runtime-result.txt';level=$null;mode='Pressure';output='Evidence/PressureEscape/pressure-final'},
    @{folder=(Join-Path $DemoEvidence 'police-final');result='police-runtime-result.txt';level=$null;mode='Police';output='Evidence/PressureEscape/police-final'},
    @{folder=(Join-Path $DemoEvidence 'streets-final');result='streets-runtime-result.txt';level=$null;mode='Streets';output='Evidence/PressureEscape/streets-final'},
    @{folder=(Join-Path $DemoEvidence 'legacy-final');result='runtime-result.txt';level=$null;mode='Legacy';output='Evidence/PressureEscape/legacy-final'}
)
foreach($route in $routes){
    $identityPath=Join-Path $route.folder 'build-identity.json'
    $identity=Get-Content -LiteralPath $identityPath -Raw | ConvertFrom-Json
    if($identity.assemblySha256 -ne $hash){throw "Evidence belongs to a different assembly: $identityPath"}
    # PowerShell 7 can deserialize ISO dates into DateTime; avoid locale string conversion.
    $testedAt=if($identity.testedAt -is [DateTime]){$identity.testedAt.ToUniversalTime()}else{[DateTimeOffset]::Parse($identity.testedAt,[Globalization.CultureInfo]::InvariantCulture).UtcDateTime}
    if($testedAt -lt $assemblyTime){throw "Test started before final assembly: $identityPath"}
    if($route.level -and $identity.level -ne $route.level){throw "Wrong foundation level: $identityPath"}
    if($route.mode -and $identity.mode -ne $route.mode){throw "Wrong runtime mode: $identityPath"}
    $result=Join-Path $route.folder $route.result
    if((Get-Content -LiteralPath $result -TotalCount 1) -ne 'PASS' -or (Get-Item -LiteralPath $result).LastWriteTimeUtc -lt $testedAt){throw "Missing fresh PASS: $result"}
    $log=Join-Path $route.folder 'player.log'
    if(-not(Test-Path -LiteralPath $log) -or (Get-Item -LiteralPath $log).LastWriteTimeUtc -lt $testedAt){throw "Missing fresh player log: $log"}
    if(Select-String -LiteralPath $log -Pattern 'NullReferenceException|InvalidOperationException|IndexOutOfRangeException|ArgumentException|MissingReferenceException|Assertion failed' -Quiet){throw "Runtime error in $log"}
    $shots=@(Get-ChildItem -LiteralPath $route.folder -Filter '*.png' -File | Where-Object {$_.LastWriteTimeUtc -ge $testedAt -and $_.Length -gt 0})
    if($shots.Count -eq 0){throw "No fresh rendered screenshots: $($route.folder)"}
    if($route.mode -eq 'Pressure'){
        $profilePath=Join-Path $route.folder 'pressure-profile.json'
        if(-not(Test-Path -LiteralPath $profilePath) -or (Get-Item -LiteralPath $profilePath).LastWriteTimeUtc -lt $testedAt){throw 'No fresh sustained-pressure profile.'}
        $profile=Get-Content -LiteralPath $profilePath -Raw | ConvertFrom-Json
        if($profile.frames -le 0 -or $profile.samples.Count -eq 0 -or $profile.processes -ne 1){throw 'Pressure profile lacks frame samples or reports multiple game processes.'}
    }
}
$documents=@('CREDITS.md','Assets/Resources/Architecture/README.md','Evidence/PressureEscape/validation-summary.json','Evidence/PressureEscape/pressure-final/pressure-profile.json','Assets/Resources/Audio/CREDITS.md','Assets/Resources/Audio/provenance.json','Assets/Resources/Audio/Kenney-Impact-License.txt','Assets/Resources/Audio/Kenney-Interface-License.txt','LICENSE','PRESSURE-AND-ESCAPE.md','README.md','VISION.md','WORLD.md','DESIGN.md','Evidence/VALIDATION.md','Docs/funstra-review-dossier-pressure-escape.html')
foreach($editorFile in Get-ChildItem -LiteralPath (Join-Path $DemoEvidence 'editor') -File){$documents+=('Evidence/PressureEscape/editor/'+$editorFile.Name)}
foreach($replayFile in Get-ChildItem -LiteralPath (Join-Path $DemoEvidence 'encounter-third') -File){$documents+=('Evidence/PressureEscape/encounter-third/'+$replayFile.Name)}
# Keep each reviewer's complete Pressure and Escape records alongside the consolidated dossier.
foreach($reviewer in @('dag-moller','marcus-webb','priya-raman','nell-okafor')){
    $records=@(Get-ChildItem -LiteralPath (Join-Path $project ('reviewer-agents/'+$reviewer)) -Filter 'pressure-escape*.md' -File)
    if(-not ($records | Where-Object {(Get-Content -LiteralPath $_.FullName -Raw).Contains($hash)})){throw "Reviewer records lack candidate identity: $reviewer"}
    if($records.Count -eq 0){throw "Pressure and Escape reviewer records missing: $reviewer"}
    foreach($record in $records){$documents+=('reviewer-agents/'+$reviewer+'/'+$record.Name)}
}
# Preserve every link inside the independent review records, including their full logs.
foreach($short in @('dag','marcus','priya','nell')){
    $reviewFolder=Join-Path $DemoEvidence ('review-'+$short)
    $reviewIdentity=Get-Content -LiteralPath (Join-Path $reviewFolder 'build-identity.json') -Raw | ConvertFrom-Json
    if($reviewIdentity.mode -ne 'Pressure'){throw "Reviewer evidence is not the Pressure route: $short"}
    if($reviewIdentity.assemblySha256 -ne $hash){throw "Reviewer evidence differs from candidate: $short"}
    $reviewedAt=if($reviewIdentity.testedAt -is [DateTime]){$reviewIdentity.testedAt.ToUniversalTime()}else{[DateTimeOffset]::Parse($reviewIdentity.testedAt,[Globalization.CultureInfo]::InvariantCulture).UtcDateTime}
    if($reviewedAt -lt $assemblyTime){throw "Reviewer test started before final assembly: $short"}
    if((Get-Content -LiteralPath (Join-Path $reviewFolder 'pressure-runtime-result.txt') -TotalCount 1) -ne 'PASS'){throw "Reviewer route did not pass: $short"}
    foreach($required in @('pressure-runtime-result.txt','player.log','pressure-profile.json')){
        $requiredPath=Join-Path $reviewFolder $required
        if(-not(Test-Path -LiteralPath $requiredPath) -or (Get-Item -LiteralPath $requiredPath).LastWriteTimeUtc -lt $reviewedAt){throw "Missing fresh reviewer artifact: $requiredPath"}
    }
    if(Select-String -LiteralPath (Join-Path $reviewFolder 'player.log') -Pattern 'NullReferenceException|InvalidOperationException|IndexOutOfRangeException|ArgumentException|MissingReferenceException|Assertion failed' -Quiet){throw "Runtime error in reviewer log: $short"}
    $reviewShots=@(Get-ChildItem -LiteralPath $reviewFolder -Filter '*.png' -File | Where-Object {$_.LastWriteTimeUtc -ge $reviewedAt -and $_.Length -gt 0})
    if($reviewShots.Count -eq 0){throw "No fresh reviewer screenshots: $short"}
    foreach($file in Get-ChildItem -LiteralPath $reviewFolder -File){$documents+=('Evidence/PressureEscape/review-'+$short+'/'+$file.Name)}
}
# Resolve local dossier assets/links without rewriting the original case file.
# Follow linked HTML/CSS dependencies; reviewer Markdown remains the original text.
$pending=[Collections.Generic.Queue[string]]::new()
$pending.Enqueue('Docs/funstra-review-dossier-pressure-escape.html')
$scanned=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
while($pending.Count -gt 0){
    $relative=$pending.Dequeue()
    if(-not $scanned.Add($relative)){continue}
    $source=Join-Path $project $relative
    $text=Get-Content -LiteralPath $source -Raw
    $references=@([regex]::Matches($text,'(?:href|src)\s*=\s*["'']([^"'']+)["'']') | ForEach-Object {$_.Groups[1].Value})
    $references+=@([regex]::Matches($text,'url\(\s*["'']?([^\)"'']+)["'']?\s*\)') | ForEach-Object {$_.Groups[1].Value.Trim()})
    foreach($reference in $references){
        if($reference -match '^(?:[a-zA-Z][a-zA-Z0-9+.-]*:|//|#)'){continue}
        $local=[Uri]::UnescapeDataString(($reference -split '[?#]',2)[0])
        if(-not $local){continue}
        $target=[IO.Path]::GetFullPath((Join-Path (Split-Path -Parent $source) $local))
        $prefix=$project.TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
        if(-not $target.StartsWith($prefix,[StringComparison]::OrdinalIgnoreCase)){throw "Dossier link leaves repository: $reference"}
        if(-not(Test-Path -LiteralPath $target -PathType Leaf)){throw "Dossier link has no local file: $reference"}
        if([IO.Path]::GetExtension($target) -eq '.log'){throw "Raw logs are not portable review assets; link a curated result instead: $reference"}
        $targetRelative=$target.Substring($prefix.Length).Replace('\','/')
        if($documents -notcontains $targetRelative){$documents+=$targetRelative}
        if([IO.Path]::GetExtension($target) -in @('.html','.css')){$pending.Enqueue($targetRelative)}
    }
}
foreach($name in $documents){if(-not(Test-Path -LiteralPath (Join-Path $project $name) -PathType Leaf)){throw "Required release document missing: $name"}}
if($ValidateOnly){Write-Output "PASS: Pressure and Escape packaging prerequisites; assembly SHA256 $hash. No archive written.";return}

$releaseDir=Join-Path $project 'Releases'
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
$release=Join-Path $releaseDir 'Funstra-pressure-escape-0.4.2-windows.zip'
if((Test-Path -LiteralPath $release) -and -not $Force){throw 'Pressure and Escape archive exists. Use -Force to replace it after validation.'}
$temporary=Join-Path $releaseDir ('Funstra-pressure-escape-'+[Guid]::NewGuid().ToString('N')+'.partial')
Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
try {
    $stream=[IO.File]::Open($temporary,[IO.FileMode]::CreateNew)
    $archive=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
    $entryNames=[Collections.Generic.HashSet[string]]::new([StringComparer]::OrdinalIgnoreCase)
    function Add-ReleaseFile([string]$source,[string]$name){
        $name=$name.Replace('\','/')
        if(-not $entryNames.Add($name)){return}
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($archive,$source,$name.Replace('\','/'),[IO.Compression.CompressionLevel]::Optimal) | Out-Null
    }
    function Add-ReleaseText([string]$name,[string]$text){
        $writer=[IO.StreamWriter]::new($archive.CreateEntry($name).Open())
        try {$writer.Write($text)} finally {$writer.Dispose()}
    }
    try {
        Get-ChildItem -LiteralPath $build -File -Recurse | Where-Object {$_.Extension -ne '.log'} | ForEach-Object {Add-ReleaseFile $_.FullName $_.FullName.Substring($build.Length+1)}
        foreach($name in $documents){Add-ReleaseFile (Join-Path $project $name) $name}
        Add-ReleaseFile $buildResult 'Evidence/build-result.txt'
        foreach($route in $routes){
            $folder=(Resolve-Path -LiteralPath $route.folder).Path.TrimEnd('\','/')
            Get-ChildItem -LiteralPath $folder -File -Recurse | Where-Object {$_.Extension -ne '.log'} | ForEach-Object {Add-ReleaseFile $_.FullName ($route.output+'/'+$_.FullName.Substring($folder.Length+1))}
        }
        $identity=[ordered]@{demo='Pressure and Escape increment';version='0.4.2';title='Pressure and Escape';assemblySha256=$hash;packagedAt=[DateTime]::UtcNow.ToString('o');method='Previously tested player, packaged without rebuilding. Guided review; human acceptance separate.'}
        Add-ReleaseText 'Evidence/pressure-escape-build-info.json' ($identity | ConvertTo-Json)
        Add-ReleaseText 'START-HERE.txt' "FUNSTRA 0.4.2 - PRESSURE AND ESCAPE`r`nExtract the whole archive, then run Funstra.exe. Keep its data folder and DLLs beside it.`r`nReview route and controls: PRESSURE-AND-ESCAPE.md`r`nFour guided reviews and CD decisions: Docs/funstra-review-dossier-pressure-escape.html`r`nAudio credits and source provenance: Assets/Resources/Audio/CREDITS.md`r`nValidation: Evidence/VALIDATION.md. Human acceptance is separate from agent review.`r`n"
    } finally {$archive.Dispose();$stream.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($temporary)
    try {
        $entry=$zip.GetEntry('Funstra_Data/Managed/Assembly-CSharp.dll')
        if(-not $entry){throw 'Packaged assembly is missing.'}
        $inputStream=$entry.Open();$sha=[Security.Cryptography.SHA256]::Create()
        try {$packagedHash=[BitConverter]::ToString($sha.ComputeHash($inputStream)).Replace('-','')} finally {$sha.Dispose();$inputStream.Dispose()}
        if($packagedHash -ne $hash){throw 'Packaged assembly differs from the tested candidate.'}
        foreach($name in @('Funstra.exe','UnityPlayer.dll','START-HERE.txt')+$documents){if(-not $zip.GetEntry($name)){throw "Package entry missing: $name"}}
    } finally {$zip.Dispose()}
    if((Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash -ne $hash){throw 'Player changed while packaging. Run validation against the final build.'}
    Move-Item -LiteralPath $temporary -Destination $release -Force:$Force
    Get-Item -LiteralPath $release | Select-Object FullName,Length
    Get-FileHash -LiteralPath $release -Algorithm SHA256
} finally {
    if(Test-Path -LiteralPath $temporary){Remove-Item -LiteralPath $temporary}
}
