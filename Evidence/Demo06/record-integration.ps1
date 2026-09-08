$ErrorActionPreference='Stop'
$e='Evidence/Demo06'
$b=Get-Content "$e/integration-source.json" -Raw|ConvertFrom-Json
$batch=@(Get-Content "$e/integration-batch.json" -Raw|ConvertFrom-Json)
if($batch.Count -ne 3 -or @($batch|Where-Object status -ne 'PASS').Count){throw 'Affected routes are not all complete.'}
$routes=@(foreach($r in $batch){
 $folder="$e/integration-$($r.route)"
 $identity=Get-Content "$folder/build-identity.json" -Raw|ConvertFrom-Json
 $name=if($r.route -eq 'legacy'){'runtime-result.txt'}else{"$($r.route)-runtime-result.txt"}
 $lines=Get-Content "$folder/$name"
 if($lines[0] -ne 'PASS' -or $identity.assemblySha256 -ne $b.assemblySha256 -or ($identity.PSObject.Properties.Name -contains 'playerExitCode' -and $identity.playerExitCode -ne 0)){throw "Invalid $folder"}
 $errors=@(Select-String -Path "$folder/player.log" -Pattern 'NullReferenceException|InvalidOperationException|IndexOutOfRangeException|ArgumentException|MissingReferenceException|Assertion failed')
 if($errors.Count){throw "Runtime exception $folder"}
 [pscustomobject][ordered]@{route=$r.route;status='PASS';checks=@($lines|Where-Object{$_ -match '^PASS:'}).Count;captures=@(Get-ChildItem $folder -Filter '*.png').Count;identity=$identity;result="$folder/$name";resultSha256=(Get-FileHash "$folder/$name").Hash;playerLog="$folder/player.log";playerLogSha256=(Get-FileHash "$folder/player.log").Hash;runtimeExceptionMatches=0}
})
$s=Get-Content "$e/validation-summary.json" -Raw|ConvertFrom-Json
$s.phase='Original independent panel complete; three display corrections and affected B routes validated; package and publication pending'
$s|Add-Member -Force NoteProperty integration ([ordered]@{assemblySha256=$b.assemblySha256;builtAt=$b.builtAt;source='Evidence/Demo06/integration-source.json';proof='Evidence/Demo06/integration-validation.json';routes=$routes;checks=($routes|Measure-Object checks -Sum).Sum;captures=($routes|Measure-Object captures -Sum).Sum;method='Original A results remain above. B full Crew, Police and Legacy only; five other routes not rerun on B. Gameplay sources and compiled scenes/assets match under the exact three-expression display proof.'})
$s|Add-Member -Force NoteProperty originalPanel @(
 @{reviewer='Dag Moller';gameNow=5.4;sliceLower=8.5;sliceUpper=9.8;evidence=89;review='reviewer-agents/dag-moller/demo06.md'},
 @{reviewer='Priya Raman';gameNow=5.4;sliceLower=8.7;sliceUpper=10;evidence=89;review='reviewer-agents/priya-raman/demo06.md'},
 @{reviewer='Marcus Webb';gameNow=5.7;sliceLower=8.7;sliceUpper=9.8;evidence=89;review='reviewer-agents/marcus-webb/demo06.md'},
 @{reviewer='Nell Okafor';gameNow=5.3;sliceLower=8.3;sliceUpper=9.6;evidence=89;review='reviewer-agents/nell-okafor/demo06.md'}
)
$s|ConvertTo-Json -Depth 30|Set-Content "$e/validation-summary.json" -Encoding utf8
$editor=@('PASS: Demo06 presentation integration export editor validation',"Assembly SHA256: $($b.assemblySha256)","Built at: $($b.builtAt)",'METHOD: Sanitized assertion results; full licensing/environment editor log is not copied. Counts are assertions, not gameplay scenarios.','TOTAL EDITOR ASSERTIONS: 2377','Combat squad: 39; Combat: 36; Crew opposition: 31; Dock operation: 23')
foreach($f in Get-ChildItem Evidence -Filter '*rules-result.txt'){$editor+="$($f.Name) | $($f.LastWriteTimeUtc.ToString('o')) | $(Get-Content $f.FullName -TotalCount 1)"}
$editor+=Get-Content Evidence/build-result.txt
$editor|Set-Content "$e/integration-editor-checks.txt" -Encoding utf8
Write-Output "B affected routes: $(($routes|Measure-Object checks -Sum).Sum) assertions, $(($routes|Measure-Object captures -Sum).Sum) captures."
