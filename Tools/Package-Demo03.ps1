param()
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$build=Join-Path $project 'Build\KeepTheLightsOn'
$evidence=Join-Path $project 'Evidence'
foreach($name in @('bandage-runtime-result.txt','runtime-result.txt','bandage-visual-result.txt')) {
    $result=Join-Path $evidence $name
    if((Get-Content $result -TotalCount 1) -ne 'PASS'){throw "Validation not passed: $name"}
    if((Get-Item $result).LastWriteTimeUtc -lt (Get-Item (Join-Path $build 'Funstra_Data\Managed\Assembly-CSharp.dll')).LastWriteTimeUtc){throw "Validation predates final assembly: $name"}
}
$assembly=Join-Path $build 'Funstra_Data\Managed\Assembly-CSharp.dll'
$hash=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash
@("Funstra Demo 03 / Keep the Lights On / 0.3.0","Assembly-CSharp.dll SHA256: $hash","Packaged UTC: $([DateTime]::UtcNow.ToString('o'))") | Set-Content (Join-Path $evidence 'demo-03-build-info.txt')
$release=Join-Path $project 'Releases\Funstra-demo-03-windows.zip'
Add-Type -AssemblyName System.IO.Compression
$stream=[IO.File]::Open($release,[IO.FileMode]::Create)
$archive=[IO.Compression.ZipArchive]::new($stream,[IO.Compression.ZipArchiveMode]::Create)
function Add-ReleaseFile([string]$source,[string]$name) {
    $entry=$archive.CreateEntry($name.Replace('\','/'),[IO.Compression.CompressionLevel]::Optimal)
    $dest=$entry.Open();$inputFile=[IO.File]::OpenRead($source)
    try {$inputFile.CopyTo($dest)} finally {$inputFile.Dispose();$dest.Dispose()}
}
try {
    Get-ChildItem -LiteralPath $build -Recurse -File | ForEach-Object { Add-ReleaseFile $_.FullName $_.FullName.Substring($build.Length+1) }
    foreach($name in @('KEEP-THE-LIGHTS-ON.md','BED-AND-BANDAGE.md','README.md','VISION.md','WORLD.md','DESIGN.md')) {Add-ReleaseFile (Join-Path $project $name) $name}
    Add-ReleaseFile (Join-Path $evidence 'VALIDATION.md') 'Evidence/VALIDATION.md'
    Add-ReleaseFile (Join-Path $project 'Docs/funstra-review-dossier-demo03.html') 'Docs/funstra-review-dossier-demo03.html'
    Add-ReleaseFile (Join-Path $project 'Docs/RELEASE-DEMO03.md') 'Docs/RELEASE-DEMO03.md'
    foreach($name in @('build-result.txt','rules-result.txt','district-rules-result.txt','refuge-rules-result.txt','bandage-runtime-result.txt','runtime-result.txt','bandage-visual-result.txt','demo-03-build-info.txt')) {Add-ReleaseFile (Join-Path $evidence $name) ('Evidence/'+$name)}
    Get-ChildItem -LiteralPath $evidence -Filter 'C*.png' | ForEach-Object {Add-ReleaseFile $_.FullName ('Evidence/'+$_.Name)}
    foreach($name in @('13-mara-standing-runner.png','14-mara-standing-connected.png')) {Add-ReleaseFile (Join-Path $evidence $name) ('Evidence/'+$name)}
    $entry=$archive.CreateEntry('START-HERE.txt');$writer=[IO.StreamWriter]::new($entry.Open())
    try {$writer.Write("FUNSTRA DEMO 03 - KEEP THE LIGHTS ON`r`nExtract the entire archive, then run Funstra.exe.`r`nKeep the data folder and DLLs beside the executable.`r`nReview route: KEEP-THE-LIGHTS-ON.md`r`nStart at cyan REPAIR. F at clinic: refuge. P: pet Tally. Space: tactics.`r`nContinue imports Demo 02 without changing its save. Demo 03 uses lights-progress.json.`r`nImplementation and automated validation complete; human acceptance pending.")} finally {$writer.Dispose()}
} finally {$archive.Dispose();$stream.Dispose()}
Get-Item -LiteralPath $release | Select-Object FullName,Length
Get-FileHash -LiteralPath $release -Algorithm SHA256
