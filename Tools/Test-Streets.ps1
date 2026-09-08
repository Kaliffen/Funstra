param([ValidateSet('Streets','Legacy','Visual','Environment','Police','Pressure')][string]$Mode='Streets',[switch]$Visible,[switch]$ThirtyFPS,[string]$EvidenceRoot,[string]$PlayerPath,[string]$ReplaySnapshot)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
if(-not $PlayerPath){$PlayerPath=Join-Path $project $(if($Mode -eq 'Pressure'){'Build\PressureEscape\Funstra.exe'}elseif($Mode -eq 'Police'){'Build\PoliceResponse\Funstra.exe'}else{'Build\StreetsWorthFightingFor\Funstra.exe'})}
if(-not $EvidenceRoot){$EvidenceRoot=Join-Path $project ('Evidence\Demo04\'+$Mode.ToLowerInvariant())}
$EvidenceRoot=[IO.Path]::GetFullPath($EvidenceRoot)
New-Item -ItemType Directory -Force -Path $EvidenceRoot | Out-Null
$flag=if($Mode -eq 'Pressure'){'--pressure-test'}elseif($Mode -eq 'Police'){'--police-test'}elseif($Mode -eq 'Environment'){'--environment-test'}elseif($Mode -eq 'Streets'){'--streets-test'}elseif($Mode -eq 'Visual'){'--bandage-visual'}else{'--smoke-test'}
$name=if($Mode -eq 'Pressure'){'pressure-runtime-result.txt'}elseif($Mode -eq 'Police'){'police-runtime-result.txt'}elseif($Mode -eq 'Environment'){'environment-runtime-result.txt'}elseif($Mode -eq 'Streets'){'streets-runtime-result.txt'}elseif($Mode -eq 'Visual'){'bandage-visual-result.txt'}else{'runtime-result.txt'}
$arguments=@($flag,'--mute-tests','--capture-screens','--evidence',('"'+$EvidenceRoot+'"'),'-screen-width','1280','-screen-height','720','-screen-fullscreen','0','-logFile',('"'+(Join-Path $EvidenceRoot 'player.log')+'"'))
if($ReplaySnapshot){if($Mode -ne 'Pressure'){throw 'ReplaySnapshot requires Pressure mode'};$arguments+=@('--pressure-replay',('"'+[IO.Path]::GetFullPath($ReplaySnapshot)+'"'))}
if($ThirtyFPS){$arguments+='--30fps'}
$style=if($Visible){'Normal'}else{'Hidden'}
$lock=New-Object System.Threading.Mutex($false,'Local\FunstraFoundationValidation')
$owned=$false
try {
    $owned=$lock.WaitOne(0)
    if(-not $owned){throw 'Another Funstra validation is using the machine.'}
    $started=Get-Date
    $player=Start-Process -FilePath $playerPath -ArgumentList $arguments -WorkingDirectory $project -WindowStyle $style -PassThru
    if(-not $player.WaitForExit(480000)){Stop-Process -Id $player.Id;throw 'Streets validation timed out.'}
    $result=Join-Path $EvidenceRoot $name
    if(-not(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTime -lt $started){throw 'No fresh runtime result.'}
    $report=Get-Content -LiteralPath $result -Raw
    Write-Output $report
    if($player.ExitCode -ne 0 -or -not $report.StartsWith('PASS')){throw 'Streets player validation failed.'}
    [ordered]@{testedAt=$started.ToUniversalTime().ToString('o');mode=$(if($ReplaySnapshot){'PressureReplay'}else{$Mode});executable=$playerPath;assemblySha256=(Get-FileHash -LiteralPath (Join-Path (Split-Path -Parent $PlayerPath) 'Funstra_Data\Managed\Assembly-CSharp.dll')).Hash;method='Guided exported player; direct setups and controller travel identified in result.'} | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $EvidenceRoot 'build-identity.json') -Encoding utf8
} finally {if($player -and -not $player.HasExited){Stop-Process -Id $player.Id};if($owned){$lock.ReleaseMutex()};$lock.Dispose()}
