# One serialized final-candidate run. Stop on the first failure; do not overwrite evidence.
$ErrorActionPreference='Stop'
$project=[IO.Path]::GetFullPath((Join-Path $PSScriptRoot '../..'))
Set-Location -LiteralPath $project
$player=Join-Path $project 'Build/NobodyGetsHomeAlone/Funstra.exe'
$assembly=Join-Path (Split-Path $player) 'Funstra_Data/Managed/Assembly-CSharp.dll'
$expected=(Get-Content (Join-Path $PSScriptRoot 'candidate-source.json') -Raw | ConvertFrom-Json).assemblySha256
$routes=@(
    @{name='crew';script='Tools/Test-Crew.ps1';flags=@{}},
    @{name='approach';script='Tools/Test-Crew.ps1';flags=@{ApproachOnly=$true}},
    @{name='pressure';script='Tools/Test-Streets.ps1';flags=@{Mode='Pressure';WithCrew=$true}},
    @{name='arms';script='Tools/Test-Arms.ps1';flags=@{}},
    @{name='residents';script='Tools/Test-Residents.ps1';flags=@{}},
    @{name='streets';script='Tools/Test-Streets.ps1';flags=@{Mode='Streets'}},
    @{name='police';script='Tools/Test-Streets.ps1';flags=@{Mode='Police'}},
    @{name='legacy';script='Tools/Test-Streets.ps1';flags=@{Mode='Legacy'}}
)
$records=[Collections.Generic.List[object]]::new()
foreach($route in $routes){
    if((Get-FileHash -LiteralPath $assembly).Hash -ne $expected){throw 'Candidate changed before route.'}
    $folder=Join-Path $PSScriptRoot ($route.name+'-final')
    if(Test-Path -LiteralPath $folder){throw "Refusing to overwrite evidence: $folder"}
    New-Item -ItemType Directory -Path $folder | Out-Null
    $record=[ordered]@{route=$route.name;startedAt=[DateTime]::UtcNow.ToString('o');endedAt=$null;status='running';assemblySha256=$expected;error=$null}
    $records.Add($record)
    $records | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'final-batch.json') -Encoding utf8
    try {
        $common=@{Visible=$true;ThirtyFPS=$true;PlayerPath=$player;EvidenceRoot=$folder}
        $flags=$route.flags
        & (Join-Path $project $route.script) @common @flags | Set-Content (Join-Path $folder 'runner-output.txt') -Encoding utf8
        if((Get-FileHash -LiteralPath $assembly).Hash -ne $expected){throw 'Candidate changed during route.'}
        $record.status='PASS'
        Write-Output ($route.name+': PASS')
    } catch {$record.status='FAIL';$record.error=$_.Exception.Message;throw}
    finally {
        $record.endedAt=[DateTime]::UtcNow.ToString('o')
        $records | ConvertTo-Json -Depth 5 | Set-Content (Join-Path $PSScriptRoot 'final-batch.json') -Encoding utf8
    }
}

