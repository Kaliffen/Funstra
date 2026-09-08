<#
.SYNOPSIS
Package the reviewed, already-tested Nobody Gets Home Alone player. Never builds or publishes.
.DESCRIPTION
Requires the assembly SHA-256 recorded for the reviewed candidate and fresh,
matching Crew, approach, Arms, Residents, Streets, Police, Pressure and Legacy evidence. Screenshots and logs
are evidence, not a substitute for the four reviews or the CD's visual inspection.
Optional -ReviewedAssemblySha256 permits only the pinned three-expression UI integration,
with full original evidence and affected-route/addendum proof for the packaged assembly.
Publish the resulting archive through the documented GitHub release path. The
Site workflow enforces the shared latest-five policy; this script does not prune
releases, source tags, earlier local builds, saves or dossiers.
.EXAMPLE
pwsh Tools/Package-Crew.ps1 -ExpectedAssemblySha256 <reviewed-candidate-hash> -ValidateOnly
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)][ValidatePattern('^[0-9a-fA-F]{64}$')][string]$ExpectedAssemblySha256,
    [ValidatePattern('^[0-9a-fA-F]{64}$')][string]$ReviewedAssemblySha256,
    [string]$DemoEvidence,
    [switch]$ValidateOnly,
    [switch]$Force
)
$ErrorActionPreference='Stop'
Set-StrictMode -Version Latest
$project=Split-Path -Parent $PSScriptRoot
$build=Join-Path $project 'Build\NobodyGetsHomeAlone'
$evidence=Join-Path $project 'Evidence'
if(-not $DemoEvidence){$DemoEvidence=Join-Path $evidence 'Demo06'}
if(-not $ReviewedAssemblySha256){$ReviewedAssemblySha256=$ExpectedAssemblySha256}
$uiIntegration=$ReviewedAssemblySha256 -ne $ExpectedAssemblySha256
$integrationDocuments=@()
$assembly=Join-Path $build 'Funstra_Data\Managed\Assembly-CSharp.dll'
foreach($path in @($assembly,(Join-Path $build 'Funstra.exe'),(Join-Path $build 'UnityPlayer.dll'))){
    if(-not(Test-Path -LiteralPath $path -PathType Leaf)){throw "Required player file missing: $path"}
}
$hash=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash
if($hash -ne $ExpectedAssemblySha256){throw 'Player assembly differs from the identified tested candidate.'}
$assemblyTime=(Get-Item -LiteralPath $assembly).LastWriteTimeUtc
$buildResult=Join-Path $evidence 'build-result.txt'
if((Get-Content -LiteralPath $buildResult -TotalCount 1) -ne 'Succeeded' -or (Get-Item -LiteralPath $buildResult).LastWriteTimeUtc -lt $assemblyTime){throw 'No successful build result for the current assembly.'}


function ConvertTo-Utc($Value){
    if($Value -is [DateTime]){return $Value.ToUniversalTime()}
    return [DateTimeOffset]::Parse($Value,[Globalization.CultureInfo]::InvariantCulture).UtcDateTime
}
$reviewedTime=$assemblyTime
$coverageMethod='Full eight-route and four-reviewer evidence identify this packaged assembly.'
if($uiIntegration){
    # This exception is pinned to the actual reviewed Demo06 source, not a general
    # permission to replace arbitrary expressions or reuse another build's tests.
    $originalAssembly='3A3C13ED20B5E957CE62F53D8611702FB8D0080CD575C5FF76FAA0F2625985C3'
    $originalManifestHash='369FAB061C18146FA1E463263C1A72FC7020E79508A6D603E0F9D3DAC742F19E'
    $originalUiHash='7275201BDA27048EA9177DF0C09D77AC8090E4623A977340AD6FA452251E98CF'
    if($ReviewedAssemblySha256 -ne $originalAssembly){throw 'UI integration exception is limited to the identified Demo06 reviewed assembly.'}
    $originalPath=Join-Path $DemoEvidence 'candidate-source.json'
    if((Get-FileHash -LiteralPath $originalPath).Hash -ne $originalManifestHash){throw 'Original reviewed source manifest changed.'}
    $original=Get-Content -LiteralPath $originalPath -Raw | ConvertFrom-Json
    $integratedPath=Join-Path $DemoEvidence 'integration-source.json'
    $integrated=Get-Content -LiteralPath $integratedPath -Raw | ConvertFrom-Json
    $receipt=Get-Content -LiteralPath (Join-Path $DemoEvidence 'integration-validation.json') -Raw | ConvertFrom-Json
    $specPath=Join-Path $DemoEvidence 'ui-replacements.json'
    $spec=Get-Content -LiteralPath $specPath -Raw | ConvertFrom-Json
    $reviewedUiPath=Join-Path $DemoEvidence 'review-source/FunstraUI.cs'
    if($receipt.schemaVersion -ne 1 -or $receipt.presentationOnly -isnot [bool] -or -not $receipt.presentationOnly -or $receipt.reviewedAssemblySha256 -ne $ReviewedAssemblySha256 -or $receipt.assemblySha256 -ne $hash){throw 'Invalid UI integration receipt identity or scope.'}
    if(($receipt.affectedRoutes -join ',') -cne 'crew,police,legacy'){throw 'UI integration must name exactly the three affected full routes.'}
    if([string]::IsNullOrWhiteSpace($receipt.method)){throw 'UI integration receipt lacks its coverage method.'}
    if($receipt.integrationSourceSha256 -ne (Get-FileHash -LiteralPath $integratedPath).Hash -or $receipt.replacementSpecSha256 -ne (Get-FileHash -LiteralPath $specPath).Hash -or $receipt.reviewedUiSourceSha256 -ne $originalUiHash -or (Get-FileHash -LiteralPath $reviewedUiPath).Hash -ne $originalUiHash){throw 'UI integration proof files differ from their recorded hashes.'}
    if($original.assemblySha256 -ne $ReviewedAssemblySha256 -or $integrated.assemblySha256 -ne $hash -or $integrated.version -ne '0.6.0' -or $integrated.executableSha256 -ne (Get-FileHash -LiteralPath (Join-Path $build 'Funstra.exe')).Hash){throw 'Integrated source manifest differs from the packaged player.'}
    $reviewedTime=ConvertTo-Utc $original.builtAt
    $integratedTime=ConvertTo-Utc $integrated.builtAt
    if($integratedTime -le $reviewedTime -or [Math]::Abs(($integratedTime-$assemblyTime).TotalSeconds) -gt 2){throw 'Integrated build timestamp does not identify the current export.'}
    $buildLogPath=[IO.Path]::GetFullPath((Join-Path $project $integrated.buildLog))
    $projectPrefix=$project.TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
    if(-not $buildLogPath.StartsWith($projectPrefix,[StringComparison]::OrdinalIgnoreCase) -or -not(Test-Path -LiteralPath $buildLogPath -PathType Leaf)){throw 'Integrated build log is not a local recorded file.'}
    if((Get-Item -LiteralPath $buildLogPath).LastWriteTimeUtc -lt $assemblyTime -or (Get-Content -LiteralPath $buildLogPath -Raw) -notmatch 'Build Finished, Result: Success\.'){throw 'Integrated export has no fresh successful build log.'}
    $buildLines=@(Get-Content -LiteralPath $buildResult)
    if($buildLines -notcontains 'Errors: 0' -or $buildLines -notcontains 'Warnings: 0'){throw 'UI integration export has reported build errors or warnings.'}
    if($original.files.Count -ne 226 -or $integrated.files.Count -ne 226){throw 'UI integration source inventory must retain all 226 reviewed files.'}

    $sceneMetadataAllowed=$false
    $scenePath='Assets/Scenes/OldPort.unity'
    $reviewedScene=@($original.files | Where-Object {$_.path -ceq $scenePath})
    $integratedScene=@($integrated.files | Where-Object {$_.path -ceq $scenePath})
    if($reviewedScene.Count -ne 1 -or $integratedScene.Count -ne 1){throw 'Reviewed and integrated source must identify exactly one OldPort scene.'}
    if($reviewedScene[0].sha256 -ne $integratedScene[0].sha256){
        # BuildCrew regenerates editor object IDs. Without the old YAML, never
        # claim exact scene source equality: prove the entire compiled player
        # unchanged apart from the reviewed assembly and build GUID instead.
        if($receipt.generatedSceneSourceException -cne $scenePath){throw 'Scene source hash differs without the explicit generated-source exception.'}
        $reviewedPlayerPath=Join-Path $DemoEvidence 'reviewed-player-files.json'
        $integratedPlayerPath=Join-Path $DemoEvidence 'integrated-player-files.json'
        if($receipt.reviewedPlayerFilesSha256 -ne (Get-FileHash -LiteralPath $reviewedPlayerPath).Hash -or $receipt.integratedPlayerFilesSha256 -ne (Get-FileHash -LiteralPath $integratedPlayerPath).Hash){throw 'Player equivalence manifests differ from receipt hashes.'}
        $playerProofs=@(
            @{path=$reviewedPlayerPath;root=(Join-Path $project 'Build/NobodyGetsHomeAloneReviewed-3A3C');assembly=$ReviewedAssemblySha256},
            @{path=$integratedPlayerPath;root=$build;assembly=$hash}
        )
        $playerMaps=@()
        foreach($proof in $playerProofs){
            $manifest=Get-Content -LiteralPath $proof.path -Raw | ConvertFrom-Json
            if($manifest.schemaVersion -ne 1 -or $manifest.assemblySha256 -ne $proof.assembly -or $manifest.executableSha256 -ne $original.executableSha256){throw 'Player equivalence manifest lacks the correct reviewed/current executable identity.'}
            $entries=[Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
            $playerPrefix=[IO.Path]::GetFullPath($proof.root).TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
            foreach($file in $manifest.files){
                $target=[IO.Path]::GetFullPath((Join-Path $proof.root $file.path))
                if(-not $target.StartsWith($playerPrefix,[StringComparison]::OrdinalIgnoreCase) -or $file.path -cne $target.Substring($playerPrefix.Length).Replace('\','/')){throw 'Player equivalence file path is not canonical or leaves its root.'}
                $entries.Add($file.path,$file.sha256)
                if((Get-FileHash -LiteralPath $target).Hash -ne $file.sha256){throw "Actual preserved/current player differs from equivalence proof: $($file.path)"}
            }
            $files=@(Get-ChildItem -LiteralPath $proof.root -Recurse -File)
            if($files.Count -ne 145 -or $files.Count -ne $entries.Count){throw 'Player equivalence proof must enumerate all 145 files, without additions or omissions.'}
            foreach($file in $files){if(-not $entries.ContainsKey($file.FullName.Substring($playerPrefix.Length).Replace('\','/'))){throw 'Actual player file is missing from equivalence proof.'}}
            if($entries['Funstra_Data/Managed/Assembly-CSharp.dll'] -ne $proof.assembly -or $entries['Funstra.exe'] -ne $original.executableSha256){throw 'Preserved/current player DLL or executable does not match its known identity.'}
            if($entries['Funstra_Data/level0'] -ne '6122273E84A7D7C06BB62C29F2306DC156BB8FCF960CD311A581432AE8D7CF20'){throw 'Compiled OldPort scene differs from the reviewed player.'}
            $playerMaps+=,$entries
        }
        $changedPlayerFiles=@()
        foreach($entry in $playerMaps[0].GetEnumerator()){
            if(-not $playerMaps[1].ContainsKey($entry.Key)){throw 'Reviewed and current players have different file inventories.'}
            if($entry.Value -ne $playerMaps[1][$entry.Key]){
                $changedPlayerFiles+=$entry.Key
                if($entry.Key -cnotin @('Funstra_Data/Managed/Assembly-CSharp.dll','Funstra_Data/boot.config')){throw "Compiled runtime asset or engine file changed: $($entry.Key)"}
            }
        }
        if($changedPlayerFiles.Count -ne 2){throw 'Expected only assembly and boot GUID changes in this integration export.'}
        $reviewedBootPath=Join-Path $DemoEvidence 'review-source/boot.config'
        $integratedBootPath=Join-Path $DemoEvidence 'integration-source/boot.config'
        if($receipt.reviewedBootSha256 -ne $playerMaps[0]['Funstra_Data/boot.config'] -or $receipt.integratedBootSha256 -ne $playerMaps[1]['Funstra_Data/boot.config'] -or (Get-FileHash -LiteralPath $reviewedBootPath).Hash -ne $receipt.reviewedBootSha256 -or (Get-FileHash -LiteralPath $integratedBootPath).Hash -ne $receipt.integratedBootSha256){throw 'Preserved boot comparison bytes differ from actual player files.'}
        $bootUtf8=[Text.UTF8Encoding]::new($false,$true)
        $reviewedBoot=$bootUtf8.GetString([IO.File]::ReadAllBytes($reviewedBootPath))
        $integratedBoot=$bootUtf8.GetString([IO.File]::ReadAllBytes($integratedBootPath))
        $guidPattern='(?m)^build-guid=([0-9a-f]{32})(?=\r?$)'
        if([regex]::Matches($reviewedBoot,$guidPattern).Count -ne 1 -or [regex]::Matches($integratedBoot,$guidPattern).Count -ne 1 -or [regex]::Replace($reviewedBoot,$guidPattern,'build-guid=<GUID>') -cne [regex]::Replace($integratedBoot,$guidPattern,'build-guid=<GUID>')){throw 'boot.config differs beyond its single build-guid value.'}
        $sceneMetadataAllowed=$true
    }

    $originalFiles=[Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
    $integratedFiles=[Collections.Generic.Dictionary[string,string]]::new([StringComparer]::Ordinal)
    foreach($file in $original.files){$originalFiles.Add($file.path,$file.sha256)}
    foreach($file in $integrated.files){
        if(-not $originalFiles.ContainsKey($file.path)){throw "Unreviewed source file added: $($file.path)"}
        $integratedFiles.Add($file.path,$file.sha256)
        if($file.path -cne 'Assets/Scripts/FunstraUI.cs' -and -not($sceneMetadataAllowed -and $file.path -ceq $scenePath) -and $file.sha256 -ne $originalFiles[$file.path]){throw "Non-UI source changed: $($file.path)"}
        if((Get-FileHash -LiteralPath (Join-Path $project $file.path)).Hash -ne $file.sha256){throw "Current source differs from integrated freeze: $($file.path)"}
    }
    $actualFiles=@(foreach($directory in @('Assets','Packages','ProjectSettings')){Get-ChildItem -LiteralPath (Join-Path $project $directory) -File -Recurse})
    if($actualFiles.Count -ne 226){throw 'Current source inventory adds or removes reviewed files.'}
    foreach($file in $actualFiles){if(-not $integratedFiles.ContainsKey($file.FullName.Substring($projectPrefix.Length).Replace('\','/'))){throw "Current source is absent from integration proof: $($file.FullName)"}}
    if($spec.schemaVersion -ne 1 -or $spec.sourcePath -cne 'Assets/Scripts/FunstraUI.cs' -or $spec.reviewedSourceSha256 -ne $originalUiHash -or $spec.integratedSourceSha256 -ne $integratedFiles['Assets/Scripts/FunstraUI.cs'] -or $spec.replacements.Count -ne 3){throw 'Replacement specification is not the reviewed UI change.'}
    # Use code points for the malformed A literals so the script itself remains
    # independent of shell/file encodings. New source contains ASCII C# escapes.
    $bullet='"'+[char]0x00e2+[char]0x20ac+[char]0x00a2+'"'
    $dash='"'+[char]0x00e2+[char]0x20ac+[char]0x201d+' Mara"'
    $footer='Application.version=="0.5.0"?"0.5.0 / LOCAL DEMO CANDIDATE"'
    $allowed=@(
        @{id='actor-marker';old=$bullet;new='"\u2022"'},
        @{id='mara-attribution';old=$dash;new='"\u2014 Mara"'},
        @{id='title-footer';old=$footer;new=('Application.version=="0.6.0"?"0.6.0 / NOBODY GETS HOME ALONE":'+$footer)}
    )
    $utf8=[Text.UTF8Encoding]::new($false,$true)
    $expectedUi=$utf8.GetString([IO.File]::ReadAllBytes($reviewedUiPath))
    for($i=0;$i -lt 3;$i++){
        $replacement=$spec.replacements[$i];$allow=$allowed[$i]
        if($replacement.id -cne $allow.id -or $replacement.old -cne $allow.old -or $replacement.new -cne $allow.new -or $replacement.occurrences -ne 1){throw "Unapproved presentation replacement: $($allow.id)"}
        if([regex]::Matches($expectedUi,[regex]::Escape($allow.old)).Count -ne 1){throw "Reviewed source does not contain exactly one $($allow.id) expression."}
        $expectedUi=$expectedUi.Replace($allow.old,$allow.new)
    }
    $hasher=[Security.Cryptography.SHA256]::Create()
    try{$expectedUiHash=[BitConverter]::ToString($hasher.ComputeHash($utf8.GetBytes($expectedUi))).Replace('-','')}finally{$hasher.Dispose()}
    if($expectedUiHash -ne $integratedFiles['Assets/Scripts/FunstraUI.cs']){throw 'Integrated UI differs from the exact three approved replacements, including unchanged source bytes.'}
    $coverageMethod='Assembly A has eight full routes and four independent reviews. Packaged assembly B differs only by three proven presentation expressions; B has full Crew, Police and Legacy integration runs and Marcus/Priya rendered review addenda. Five unaffected routes were not rerun on B.'
    $integrationDocuments=@('Evidence/Demo06/integration-validation.json','Evidence/Demo06/integration-source.json','Evidence/Demo06/ui-replacements.json','Evidence/Demo06/review-source/FunstraUI.cs')
    if($sceneMetadataAllowed){
        $coverageMethod+=' Generated OldPort editor scene source has a different hash; no original YAML equality is claimed. All 145 A/B player files were verified: compiled scene, assets and engine files are identical; only the proved assembly and build-guid differ. The other 224 source hashes are unchanged.'
        $integrationDocuments+=@('Evidence/Demo06/reviewed-player-files.json','Evidence/Demo06/integrated-player-files.json','Evidence/Demo06/review-source/boot.config','Evidence/Demo06/integration-source/boot.config')
    }
}

$routes=@(
    @{folder=(Join-Path $DemoEvidence 'crew-final');result='crew-runtime-result.txt';level=$null;mode='Crew';output='Evidence/Demo06/crew-final'},
    @{folder=(Join-Path $DemoEvidence 'approach-final');result='crew-runtime-result.txt';level=$null;mode='Crew';output='Evidence/Demo06/approach-final'},
    @{folder=(Join-Path $DemoEvidence 'arms-final');result='arms-runtime-result.txt';level=$null;mode='Arms';output='Evidence/Demo06/arms-final'},
    @{folder=(Join-Path $DemoEvidence 'residents-final');result='residents-runtime-result.txt';level=$null;mode='Residents';output='Evidence/Demo06/residents-final'},
    @{folder=(Join-Path $DemoEvidence 'streets-final');result='streets-runtime-result.txt';level=$null;mode='Streets';output='Evidence/Demo06/streets-final'},
    @{folder=(Join-Path $DemoEvidence 'police-final');result='police-runtime-result.txt';level=$null;mode='Police';output='Evidence/Demo06/police-final'},
    @{folder=(Join-Path $DemoEvidence 'pressure-final');result='pressure-runtime-result.txt';level=$null;mode='Pressure';output='Evidence/Demo06/pressure-final'},
    @{folder=(Join-Path $DemoEvidence 'legacy-final');result='runtime-result.txt';level=$null;mode='Legacy';output='Evidence/Demo06/legacy-final'}
)
foreach($route in $routes){$route.assemblySha256=$ReviewedAssemblySha256;$route.builtAt=$reviewedTime;$route.integration=$false}
if($uiIntegration){
    foreach($name in @('crew','police','legacy')){
        $routes+=@{folder=(Join-Path $DemoEvidence ('integration-'+$name));result=if($name -eq 'legacy'){'runtime-result.txt'}else{$name+'-runtime-result.txt'};level=$null;mode=if($name -eq 'crew'){'Crew'}elseif($name -eq 'police'){'Police'}else{'Legacy'};output=('Evidence/Demo06/integration-'+$name);assemblySha256=$hash;builtAt=$assemblyTime;integration=$true}
    }
}
function Assert-ReportedPass([string[]]$Lines,[string[]]$Assertions,[string]$Source){
    foreach($assertion in $Assertions){
        if($Lines -cnotcontains ('PASS: '+$assertion)){throw "Required completed branch missing in ${Source}: $assertion"}
    }
}
foreach($route in $routes){
    $identityPath=Join-Path $route.folder 'build-identity.json'
    $identity=Get-Content -LiteralPath $identityPath -Raw | ConvertFrom-Json
    if($identity.assemblySha256 -ne $route.assemblySha256){throw "Evidence belongs to a different assembly: $identityPath"}
    # PowerShell 7 can deserialize ISO dates into DateTime; avoid locale string conversion.
    $testedAt=if($identity.testedAt -is [DateTime]){$identity.testedAt.ToUniversalTime()}else{[DateTimeOffset]::Parse($identity.testedAt,[Globalization.CultureInfo]::InvariantCulture).UtcDateTime}
    if($testedAt -lt $route.builtAt){throw "Test started before final assembly: $identityPath"}
    if($route.level -and $identity.level -ne $route.level){throw "Wrong foundation level: $identityPath"}
    if($route.mode -and $identity.mode -ne $route.mode){throw "Wrong runtime mode: $identityPath"}
    $result=Join-Path $route.folder $route.result
    if((Get-Content -LiteralPath $result -TotalCount 1) -ne 'PASS' -or (Get-Item -LiteralPath $result).LastWriteTimeUtc -lt $testedAt){throw "Missing fresh PASS: $result"}
    $reportLines=@(Get-Content -LiteralPath $result)
    # Existing runners share mode/result names with focused diagnostics. Require
    # completed production assertions from each promised route, not just PASS.
    if($route.output -eq 'Evidence/Demo06/approach-final'){
        if($reportLines -match '^METHOD: --crew-fight is a focused replay'){throw "Fight-only replay cannot satisfy all approaches: $result"}
        Assert-ReportedPass $reportLines @(
            'Unseen solo controller returns the same component to live Rell',
            'Unseen solo outcome persists without component respawn',
            'Unseen neri-pair controller returns the same component to live Rell',
            'Unseen neri-pair outcome persists without component respawn',
            'All three crew members actually spent their own finite ammunition in coordinated fight',
            'Owner/victim receipts prove protagonist and partner projectiles hit actual yard opponents',
            'Actual projectile hits establish hostile yard outcome',
            'Full-crew combat outcome reached a real terminal location or emergency recovery',
            'Full-crew combat outcome preserves integrated finite-state invariants',
            'Full-crew combat outcome reload verified: identities, custody, wounds and remaining ammunition'
        ) $result
        $outcomePath=Join-Path $route.folder 'crew-combat-outcome.json'
        if(-not(Test-Path -LiteralPath $outcomePath -PathType Leaf) -or (Get-Item -LiteralPath $outcomePath).LastWriteTimeUtc -lt $testedAt){throw 'No fresh full-crew combat outcome receipt.'}
        $combatOutcome=Get-Content -LiteralPath $outcomePath -Raw | ConvertFrom-Json
        if($combatOutcome.missionOutcome -cnotin @('component-returned','survivor-withdrawn-operation-unresolved','total-defeat-emergency-recovery')){throw 'Unrecognized full-crew combat terminal outcome.'}
        if($combatOutcome.reloadVerified -isnot [bool] -or -not $combatOutcome.reloadVerified -or $combatOutcome.componentReturned -isnot [bool] -or $combatOutcome.totalDefeat -isnot [bool]){throw 'Combat outcome lacks explicit verified reload and mission flags.'}
        if($combatOutcome.totalDefeat -ne ($combatOutcome.missionOutcome -ceq 'total-defeat-emergency-recovery')){throw 'Combat outcome contradicts its emergency recovery flag.'}
        if(($combatOutcome.missionOutcome -ceq 'component-returned' -and -not $combatOutcome.componentReturned) -or ($combatOutcome.missionOutcome -ceq 'survivor-withdrawn-operation-unresolved' -and $combatOutcome.componentReturned)){throw 'Combat mission outcome contradicts component return.'}
        foreach($savedOutcome in @($combatOutcome.beforeReload,$combatOutcome.afterReload)){
            if($null -eq $savedOutcome -or $savedOutcome.componentReturned -isnot [bool] -or $savedOutcome.componentReturned -ne $combatOutcome.componentReturned){throw 'Combat reload snapshots disagree with component return.'}
            if($savedOutcome.componentOwner -cnotin @('yard','player','neri','rell','rell-workshop') -or $savedOutcome.componentReturned -ne ($savedOutcome.componentOwner -ceq 'rell-workshop')){throw 'Combat snapshot has invalid or contradictory single-component custody.'}
            if($savedOutcome.actors.Count -ne 3 -or ($savedOutcome.actors.id -join ',') -cne 'player,neri,rell' -or $savedOutcome.selected -cnotin @('player','neri','rell')){throw 'Combat reload snapshot lacks the three stable crew identities.'}
            if($savedOutcome.playerAmmo.Count -ne 4 -or $savedOutcome.playerMagazines.Count -ne 4){throw 'Combat reload snapshot lacks all four protagonist ammunition inventories.'}
        }
        $beforeCombat=$combatOutcome.beforeReload | ConvertTo-Json -Depth 20 -Compress
        # Keep raw positions in the receipt. Existing Teleport adds .12 upward
        # controller clearance on resume; normalize only this bounded placement
        # in a comparison copy, never horizontal or carried-body coordinates.
        $comparison=$combatOutcome.afterReload | ConvertTo-Json -Depth 20 | ConvertFrom-Json
        $savedPlayer=$combatOutcome.beforeReload.actors[0]
        $resumedPlayer=$comparison.actors[0]
        $clearance=[double]$resumedPlayer.position.y-[double]$savedPlayer.position.y
        if([double]::IsNaN($clearance) -or [double]::IsInfinity($clearance) -or $clearance -lt 0 -or $clearance -gt .1201){throw 'Protagonist resume placement exceeds upward controller clearance.'}
        $playerCarried=@($combatOutcome.beforeReload.actors+$combatOutcome.afterReload.actors | Where-Object {$_.carrying -ceq 'player'}).Count -gt 0
        if($clearance -gt 0 -and $playerCarried){throw 'Carried protagonist placement must survive reload exactly.'}
        $resumedPlayer.position.y=$savedPlayer.position.y
        $afterCombat=$comparison | ConvertTo-Json -Depth 20 -Compress
        if($beforeCombat -cne $afterCombat){throw 'Combat identities, custody, wounds, positions or ammunition changed through reload beyond documented protagonist clearance.'}
        $outcomeLine='OUTCOME: Full-crew combat outcome '+$combatOutcome.missionOutcome+'; componentReturned='+$combatOutcome.componentReturned+'; custody='+$combatOutcome.beforeReload.componentOwner
        if($reportLines -cnotcontains $outcomeLine){throw 'Combat report and machine-readable outcome disagree.'}
    }
    if($route.output -in @('Evidence/Demo06/crew-final','Evidence/Demo06/integration-crew')){
        if($reportLines -match '^METHOD: --crew-fight is a focused replay|^METHOD: focused encounter replay from an earlier real deployment'){throw "Focused replay cannot satisfy the core crew route: $result"}
        Assert-ReportedPass $reportLines @(
            'Selection leaves independent positions, wounds, ammunition and magazines in place',
            'Actual reload preserves selected Neri, Rell gun custody, spent round and projectile owner/position',
            'Actual Neri controller carried protagonist around wall and through clinic doorway',
            'Actual save/reload preserves rescue, survivor health and finite clinic cost',
            'Abandonment and actual downed body location survive reload without respawn',
            'Actual reload retains independent repair partnership and finite unrecovered main pump',
            'Moving firing lane preserves both distinct rifle owners and the actual struck crew identity',
            'Recovered leader withdraws and cannot restore the lost report network'
        ) $result
    }
    if($route.integration -and $route.mode -in @('Police','Legacy')){
        $baselineName=if($route.mode -eq 'Police'){'police'}else{'legacy'}
        $baseline=@(Get-Content -LiteralPath (Join-Path (Join-Path $DemoEvidence ($baselineName+'-final')) $route.result) | Where-Object {$_ -cmatch '^PASS: '})
        $actualPasses=@($reportLines | Where-Object {$_ -cmatch '^PASS: '})
        if($baseline.Count -ne $actualPasses.Count -or (Compare-Object ($baseline | Sort-Object) ($actualPasses | Sort-Object) -CaseSensitive)){throw "Integration lacks the complete original $baselineName assertion set."}
    }
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
        if($reportLines -match '^METHOD: focused encounter replay from an earlier real deployment'){throw 'A focused pressure replay cannot satisfy the full stress route.'}
        if($profile.samples.Count -lt 76 -or [double]$profile.samples[0].seconds -gt 1 -or [double]$profile.samples[-1].seconds -lt 75){throw 'Pressure profile does not cover the full 75-second stress run from its start.'}
        $previousSecond=-1.0
        foreach($sample in $profile.samples){
            $second=[double]$sample.seconds
            if([double]::IsNaN($second) -or [double]::IsInfinity($second) -or $second -lt 0 -or $second -le $previousSecond -or ($previousSecond -ge 0 -and $second-$previousSecond -gt 1.1)){throw 'Pressure profile has invalid or missing one-second stress samples.'}
            $previousSecond=$second
        }
        Assert-ReportedPass $reportLines @(
            'Normal live dispatch delivers all six reinforcements while combat and traffic run',
            'The existing two trucks account for exactly six rifle reinforcements under the nine-officer ceiling',
            'Live dispatched rifle reinforcement produces immutable owner/player-hit evidence',
            'Every one of the nine officers fires through the live combat loop'
        ) $result
    }
}
$documents=@('CREDITS.md','Assets/Resources/Architecture/README.md','Evidence/Demo06/validation-summary.json','Evidence/Demo06/review-outcomes.json','Evidence/Demo06/pressure-final/pressure-profile.json','Assets/Resources/Audio/CREDITS.md','Assets/Resources/Audio/provenance.json','Assets/Resources/Audio/Kenney-Impact-License.txt','Assets/Resources/Audio/Kenney-Interface-License.txt','LICENSE','NOBODY-GETS-HOME-ALONE.md','README.md','VISION.md','WORLD.md','DESIGN.md','Evidence/VALIDATION.md','reviewer-agents/README.md','Docs/funstra-review-dossier-demo06.html')
$documents+='Evidence/demo06-plan.md'
$documents+='Docs/PIPELINE.md'
$documents+='Evidence/Demo06/candidate-source.json'
$documents+='Evidence/Demo06/editor-checks.txt'
$documents+='Evidence/Demo06/panel-brief.md'
$documents+=$integrationDocuments
$summary=Get-Content -LiteralPath (Join-Path $DemoEvidence 'validation-summary.json') -Raw | ConvertFrom-Json
if($summary.assemblySha256 -ne $ReviewedAssemblySha256){throw 'Validation summary differs from the tested candidate.'}
$ledger=Get-Content -LiteralPath (Join-Path $DemoEvidence 'review-outcomes.json') -Raw | ConvertFrom-Json
if($ledger.schemaVersion -ne 2 -or ($ledger.outcomes | Measure-Object -Property weight -Sum).Sum -ne $ledger.totalWeight){throw 'Invalid shared v2 outcome ledger.'}
$dossier=Get-Content -LiteralPath (Join-Path $project 'Docs/funstra-review-dossier-demo06.html') -Raw
if($dossier -notmatch [regex]::Escape($hash) -or $dossier -notmatch [regex]::Escape($ReviewedAssemblySha256)){throw 'Demo06 dossier does not identify both the reviewed and packaged assemblies.'}
foreach($reviewer in @('dag-moller','marcus-webb','priya-raman','nell-okafor')){
    foreach($name in @('demo06.md','demo06-meeting.md')){
        $relative='reviewer-agents/'+$reviewer+'/'+$name
        $record=Join-Path $project $relative
        if(-not(Test-Path -LiteralPath $record -PathType Leaf)){throw "Required reviewer record missing: $relative"}
        if((Get-Content -LiteralPath $record -Raw) -notmatch [regex]::Escape($ReviewedAssemblySha256)){throw "Reviewer record lacks candidate identity: $relative"}
        $documents+=$relative
    }
    $documents+=('reviewer-agents/'+$reviewer+'.md')
}
# Preserve every link inside the independent review records, including their full logs.
foreach($short in @('dag','marcus','priya','nell')){
    $reviewFolder=Join-Path $DemoEvidence ('review-'+$short)
    $reviewIdentity=Get-Content -LiteralPath (Join-Path $reviewFolder 'build-identity.json') -Raw | ConvertFrom-Json
    if($reviewIdentity.mode -ne 'Crew'){throw "Reviewer evidence is not the Crew route: $short"}
    if($reviewIdentity.assemblySha256 -ne $ReviewedAssemblySha256){throw "Reviewer evidence differs from candidate: $short"}
    $reviewedAt=if($reviewIdentity.testedAt -is [DateTime]){$reviewIdentity.testedAt.ToUniversalTime()}else{[DateTimeOffset]::Parse($reviewIdentity.testedAt,[Globalization.CultureInfo]::InvariantCulture).UtcDateTime}
    if($reviewedAt -lt $reviewedTime){throw "Reviewer test started before final assembly: $short"}
    if((Get-Content -LiteralPath (Join-Path $reviewFolder 'crew-runtime-result.txt') -TotalCount 1) -ne 'PASS'){throw "Reviewer route did not pass: $short"}
    $reviewResult=Join-Path $reviewFolder 'crew-runtime-result.txt'
    Assert-ReportedPass @(Get-Content -LiteralPath $reviewResult) @(
        'Selection leaves independent positions, wounds, ammunition and magazines in place',
        'Actual Neri controller carried protagonist around wall and through clinic doorway',
        'Abandonment and actual downed body location survive reload without respawn',
        'Actual reload retains independent repair partnership and finite unrecovered main pump',
        'Recovered leader withdraws and cannot restore the lost report network'
    ) $reviewResult
    foreach($required in @('crew-runtime-result.txt','player.log')){
        $requiredPath=Join-Path $reviewFolder $required
        if(-not(Test-Path -LiteralPath $requiredPath) -or (Get-Item -LiteralPath $requiredPath).LastWriteTimeUtc -lt $reviewedAt){throw "Missing fresh reviewer artifact: $requiredPath"}
    }
    if(Select-String -LiteralPath (Join-Path $reviewFolder 'player.log') -Pattern 'NullReferenceException|InvalidOperationException|IndexOutOfRangeException|ArgumentException|MissingReferenceException|Assertion failed' -Quiet){throw "Runtime error in reviewer log: $short"}
    $reviewShots=@(Get-ChildItem -LiteralPath $reviewFolder -Filter '*.png' -File | Where-Object {$_.LastWriteTimeUtc -ge $reviewedAt -and $_.Length -gt 0})
    if($reviewShots.Count -eq 0){throw "No fresh reviewer screenshots: $short"}
    foreach($file in Get-ChildItem -LiteralPath $reviewFolder -File){$documents+=('Evidence/Demo06/review-'+$short+'/'+$file.Name)}
}
if($uiIntegration){
    foreach($reviewer in @('marcus-webb','priya-raman')){
        $relative='reviewer-agents/'+$reviewer+'/demo06-replay.md'
        $record=Join-Path $project $relative
        $addendum=Get-Content -LiteralPath $record -Raw
        if((Get-Item -LiteralPath $record).LastWriteTimeUtc -lt $assemblyTime -or $addendum -notmatch [regex]::Escape($ReviewedAssemblySha256) -or $addendum -notmatch [regex]::Escape($hash)){throw "UI integration lacks a fresh two-assembly review addendum: $reviewer"}
        $captureReferences=[regex]::Matches($addendum,'integration-(crew|police|legacy)/[^\s)"<>]+\.png')
        if($addendum -notmatch '(?i)(render|screenshot|capture)' -or $captureReferences.Count -eq 0){throw "Review addendum lacks an explicit rendered integration capture reference: $reviewer"}
        foreach($reference in $captureReferences){
            $capturePath=[IO.Path]::GetFullPath((Join-Path $DemoEvidence $reference.Value))
            $captureRoot=[IO.Path]::GetFullPath((Join-Path $DemoEvidence ('integration-'+$reference.Groups[1].Value))).TrimEnd('\','/')+[IO.Path]::DirectorySeparatorChar
            if(-not $capturePath.StartsWith($captureRoot,[StringComparison]::OrdinalIgnoreCase) -or -not(Test-Path -LiteralPath $capturePath -PathType Leaf)){throw "Review references a missing or out-of-route integration capture: $($reference.Value)"}
            $captureFile=Get-Item -LiteralPath $capturePath
            if($captureFile.Length -eq 0 -or $captureFile.LastWriteTimeUtc -lt $assemblyTime){throw "Review references an empty or stale integration capture: $($reference.Value)"}
        }
        $documents+=$relative
    }
}
# Resolve local dossier assets/links without rewriting the original case file.
# Follow linked HTML/CSS dependencies; reviewer Markdown remains the original text.
$pending=[Collections.Generic.Queue[string]]::new()
$pending.Enqueue('Docs/funstra-review-dossier-demo06.html')
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
        $targetRelative=$target.Substring($prefix.Length).Replace('\','/')
        if($documents -notcontains $targetRelative){$documents+=$targetRelative}
        if([IO.Path]::GetExtension($target) -in @('.html','.css')){$pending.Enqueue($targetRelative)}
    }
}
foreach($name in $documents){if(-not(Test-Path -LiteralPath (Join-Path $project $name) -PathType Leaf)){throw "Required release document missing: $name"}}
if($ValidateOnly){Write-Output "PASS: Nobody Gets Home Alone packaging prerequisites; assembly SHA256 $hash; reviewed SHA256 $ReviewedAssemblySha256. $coverageMethod No archive written.";return}

$releaseDir=Join-Path $project 'Releases'
New-Item -ItemType Directory -Path $releaseDir -Force | Out-Null
$release=Join-Path $releaseDir 'Funstra-nobody-gets-home-alone-0.6.0-windows.zip'
if((Test-Path -LiteralPath $release) -and -not $Force){throw 'Nobody Gets Home Alone archive exists. Use -Force to replace it after validation.'}
$temporary=Join-Path $releaseDir ('Funstra-nobody-gets-home-alone-'+[Guid]::NewGuid().ToString('N')+'.partial')
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
            Get-ChildItem -LiteralPath $folder -File -Recurse | ForEach-Object {Add-ReleaseFile $_.FullName ($route.output+'/'+$_.FullName.Substring($folder.Length+1))}
        }
        $identity=[ordered]@{demo='Nobody Gets Home Alone increment';version='0.6.0';title='Nobody Gets Home Alone';assemblySha256=$hash;reviewedAssemblySha256=$ReviewedAssemblySha256;uiIntegration=$uiIntegration;coverage=$coverageMethod;packagedAt=[DateTime]::UtcNow.ToString('o');method='Previously tested player, packaged without rebuilding. Guided review; human acceptance separate.'}
        Add-ReleaseText 'Evidence/demo06-build-info.json' ($identity | ConvertTo-Json)
        Add-ReleaseText 'START-HERE.txt' "FUNSTRA 0.6.0 - NOBODY GETS HOME ALONE`r`nExtract the whole archive, then run Funstra.exe. Keep its data folder and DLLs beside it.`r`nReview route and controls: NOBODY-GETS-HOME-ALONE.md`r`nFour guided reviews and CD decisions: Docs/funstra-review-dossier-demo06.html`r`nAudio credits and source provenance: Assets/Resources/Audio/CREDITS.md`r`nValidation: Evidence/VALIDATION.md. Human acceptance is separate from agent review.`r`n"
    } finally {$archive.Dispose();$stream.Dispose()}
    $zip=[IO.Compression.ZipFile]::OpenRead($temporary)
    try {
        $entry=$zip.GetEntry('Funstra_Data/Managed/Assembly-CSharp.dll')
        if(-not $entry){throw 'Packaged assembly is missing.'}
        $inputStream=$entry.Open();$sha=[Security.Cryptography.SHA256]::Create()
        try {$packagedHash=[BitConverter]::ToString($sha.ComputeHash($inputStream)).Replace('-','')} finally {$sha.Dispose();$inputStream.Dispose()}
        if($packagedHash -ne $hash){throw 'Packaged assembly differs from the tested candidate.'}
        foreach($file in Get-ChildItem -LiteralPath $build -File -Recurse | Where-Object {$_.Extension -ne '.log'}){
            $name=$file.FullName.Substring($build.Length+1).Replace('\','/')
            $packedEntry=$zip.GetEntry($name)
            if(-not $packedEntry){throw "Missing player file: $name"}
            $packedStream=$packedEntry.Open();$fileSha=[Security.Cryptography.SHA256]::Create()
            try{$packedFileHash=[BitConverter]::ToString($fileSha.ComputeHash($packedStream)).Replace('-','')}finally{$packedStream.Dispose();$fileSha.Dispose()}
            if($packedFileHash -ne (Get-FileHash -LiteralPath $file.FullName -Algorithm SHA256).Hash){throw "Player file changed in archive: $name"}
        }
        foreach($name in @('Funstra.exe','UnityPlayer.dll','START-HERE.txt')+$documents){if(-not $zip.GetEntry($name)){throw "Package entry missing: $name"}}
    } finally {$zip.Dispose()}
    if((Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash -ne $hash){throw 'Player changed while packaging. Run validation against the final build.'}
    Move-Item -LiteralPath $temporary -Destination $release -Force:$Force
    [ordered]@{archive=$release;assemblySha256=$hash;reviewedAssemblySha256=$ReviewedAssemblySha256;uiIntegration=$uiIntegration;coverage=$coverageMethod;archiveSha256=(Get-FileHash -LiteralPath $release -Algorithm SHA256).Hash;bytes=(Get-Item -LiteralPath $release).Length;verifiedAt=[DateTime]::UtcNow.ToString('o');method='All packaged player files compared with source by SHA256; no rebuild or publication. Logs excluded from player payload.'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $DemoEvidence 'package-verification.json') -Encoding utf8
    Get-Content -LiteralPath (Join-Path $DemoEvidence 'package-verification.json')
} finally {
    if(Test-Path -LiteralPath $temporary){Remove-Item -LiteralPath $temporary}
}
