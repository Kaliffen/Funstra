$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$e='Evidence/Demo06'
$a=Get-Content "$e/candidate-source.json" -Raw | ConvertFrom-Json
$bRoot=Join-Path $root 'Build/NobodyGetsHomeAlone'
$aRoot=Join-Path $root 'Build/NobodyGetsHomeAloneReviewed-3A3C'
$bDll=Join-Path $bRoot 'Funstra_Data/Managed/Assembly-CSharp.dll'
$bHash=(Get-FileHash $bDll).Hash
$b=[ordered]@{version='0.6.0';assemblySha256=$bHash;executableSha256=(Get-FileHash "$bRoot/Funstra.exe").Hash;builtAt=(Get-Item $bDll).LastWriteTimeUtc.ToString('o');buildLog='Evidence/demo06-build-integration.log';sourceBaseCommit=$a.sourceBaseCommit;sourceWasDirty=$true;method='Three UI expressions corrected after original panel. Generated scene editor identifiers differ; compiled scene and every runtime asset remain byte-identical. Full original A routes and fresh affected B routes are identified separately.';files=@($a.files|ForEach-Object{[ordered]@{path=$_.path;sha256=(Get-FileHash -LiteralPath $_.path).Hash}})}
$b|ConvertTo-Json -Depth 8|Set-Content "$e/integration-source.json" -Encoding utf8
foreach($entry in @(@{root=$aRoot;name='reviewed';hash=$a.assemblySha256},@{root=$bRoot;name='integrated';hash=$bHash})){
 $files=@(Get-ChildItem -LiteralPath $entry.root -File -Recurse | Sort-Object FullName | ForEach-Object{[ordered]@{path=[IO.Path]::GetRelativePath($entry.root,$_.FullName).Replace('\','/');sha256=(Get-FileHash -LiteralPath $_.FullName).Hash}})
 [ordered]@{schemaVersion=1;assemblySha256=$entry.hash;executableSha256=(Get-FileHash (Join-Path $entry.root 'Funstra.exe')).Hash;files=$files}|ConvertTo-Json -Depth 8|Set-Content "$e/$($entry.name)-player-files.json" -Encoding utf8
}
New-Item -ItemType Directory -Force "$e/integration-source"|Out-Null
Copy-Item -LiteralPath "$aRoot/Funstra_Data/boot.config" -Destination "$e/review-source/boot.config"
Copy-Item -LiteralPath "$bRoot/Funstra_Data/boot.config" -Destination "$e/integration-source/boot.config"
[ordered]@{schemaVersion=1;reviewedAssemblySha256=$a.assemblySha256;assemblySha256=$bHash;presentationOnly=$true;integrationSourceSha256=(Get-FileHash "$e/integration-source.json").Hash;replacementSpecSha256=(Get-FileHash "$e/ui-replacements.json").Hash;reviewedUiSourceSha256=(Get-FileHash "$e/review-source/FunstraUI.cs").Hash;affectedRoutes=@('crew','police','legacy');method='Original A full eight-route validation and four independent original reviews are retained. B has three display-expression changes and fresh full Crew, Police and Legacy routes; five unaffected routes are not claimed rerun on B. The generated source scene hash differs, but complete player comparison proves all compiled scenes/assets unchanged; only the identified DLL and build-guid-only boot differ.';generatedSceneSourceException='Assets/Scenes/OldPort.unity';reviewedPlayerFilesSha256=(Get-FileHash "$e/reviewed-player-files.json").Hash;integratedPlayerFilesSha256=(Get-FileHash "$e/integrated-player-files.json").Hash;reviewedBootSha256=(Get-FileHash "$e/review-source/boot.config").Hash;integratedBootSha256=(Get-FileHash "$e/integration-source/boot.config").Hash}|ConvertTo-Json -Depth 6|Set-Content "$e/integration-validation.json" -Encoding utf8
$r=Get-Content "$e/run-final-routes.ps1" -Raw
$r=$r.Replace("'candidate-source.json'","'integration-source.json'").Replace("'final-batch.json'","'integration-batch.json'")
$r=[regex]::Replace($r,'(?s)\$routes=@\(.*?\r?\n\)',@'
$routes=@(
    @{name='crew';script='Tools/Test-Crew.ps1';flags=@{}},
    @{name='police';script='Tools/Test-Streets.ps1';flags=@{Mode='Police'}},
    @{name='legacy';script='Tools/Test-Streets.ps1';flags=@{Mode='Legacy'}}
)
'@)
$r=$r.Replace('($route.name+''-final'')','(''integration-''+$route.name)')
$r|Set-Content "$e/run-integration-routes.ps1" -Encoding utf8
Write-Output "B assembly: $bHash"
Write-Output "Frozen source files: $($b.files.Count)"
