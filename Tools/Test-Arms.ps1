param([switch]$Visible,[switch]$ThirtyFPS,[string]$EvidenceRoot,[string]$PlayerPath)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
if(-not $PlayerPath){$PlayerPath=Join-Path $project 'Build\PriceOfAGun\Funstra.exe'}
elseif(-not [IO.Path]::IsPathRooted($PlayerPath)){$PlayerPath=Join-Path $project $PlayerPath}
$PlayerPath=[IO.Path]::GetFullPath($PlayerPath)
if(-not $EvidenceRoot){$EvidenceRoot=Join-Path $project 'Evidence\Demo05\arms'}
elseif(-not [IO.Path]::IsPathRooted($EvidenceRoot)){$EvidenceRoot=Join-Path $project $EvidenceRoot}
$EvidenceRoot=[IO.Path]::GetFullPath($EvidenceRoot)
$assembly=Join-Path (Split-Path -Parent $PlayerPath) 'Funstra_Data\Managed\Assembly-CSharp.dll'
if(-not(Test-Path -LiteralPath $PlayerPath)-or -not(Test-Path -LiteralPath $assembly)){throw 'Arms validation requires an existing exported player and gameplay assembly.'}
New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
$arguments=@('--arms-test','--mute-tests','--capture-screens','--evidence',('"'+$EvidenceRoot+'"'),'-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-logFile',('"'+(Join-Path $EvidenceRoot 'player.log')+'"'))
if($ThirtyFPS){$arguments+='--30fps'}
$style=if($Visible){'Normal'}else{'Hidden'}
$lock=New-Object System.Threading.Mutex($false,'Local\FunstraFoundationValidation')
$owned=$false;$player=$null;$started=$null;$failure=$null;$exitCode=$null
try {
    $owned=$lock.WaitOne(0)
    if(-not $owned){throw 'Another Funstra validation is using the machine.'}
    $exeHash=(Get-FileHash -LiteralPath $PlayerPath).Hash
    $assemblyHash=(Get-FileHash -LiteralPath $assembly).Hash
    $started=Get-Date
    $player=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WorkingDirectory $project -WindowStyle $style -PassThru
    if(-not $player.WaitForExit(480000)){Stop-Process -Id $player.Id;throw 'Arms validation timed out after 480 seconds.'}
    $exitCode=$player.ExitCode
    $result=Join-Path $EvidenceRoot 'arms-runtime-result.txt'
    if(-not(Test-Path -LiteralPath $result)-or (Get-Item -LiteralPath $result).LastWriteTime -lt $started){throw 'No fresh arms runtime result.'}
    $report=Get-Content -LiteralPath $result -Raw
    Write-Output $report
    if($exitCode -ne 0 -or -not $report.StartsWith('PASS')){throw 'Arms player validation failed.'}
    if((Get-FileHash -LiteralPath $assembly).Hash -ne $assemblyHash -or (Get-FileHash -LiteralPath $PlayerPath).Hash -ne $exeHash){throw 'Reviewed player changed during validation.'}
} catch {$failure=$_.Exception.Message;throw}
finally {
    if($player -and -not $player.HasExited){Stop-Process -Id $player.Id}
    if($started){
        $ended=Get-Date
        [ordered]@{testedAt=$started.ToUniversalTime().ToString('o');endedAt=$ended.ToUniversalTime().ToString('o');elapsedSeconds=($ended-$started).TotalSeconds;mode='Arms';executable=$PlayerPath;executableSha256=$exeHash;assemblySha256=$assemblyHash;playerExitCode=$exitCode;failure=$failure;method='Muted guided exported player. Controller acquisition and explicitly stepped supply/combat fixtures are identified in the runtime result.'} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $EvidenceRoot 'build-identity.json') -Encoding utf8
    }
    if($owned){$lock.ReleaseMutex()};$lock.Dispose()
}
