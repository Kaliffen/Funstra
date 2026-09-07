param([ValidateSet('Full','Visual','Legacy')][string]$Mode='Full',[switch]$Visible,[switch]$ThirtyFPS,[int]$Width=1600,[int]$Height=900)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$evidence=Join-Path $project 'Evidence'
$player=Join-Path $project 'Build\KeepTheLightsOn\Funstra.exe'
$flag=if($Mode -eq 'Visual'){'--bandage-visual'}elseif($Mode -eq 'Legacy'){'--smoke-test'}else{'--bandage-test'}
$resultName=if($Mode -eq 'Visual'){'bandage-visual-result.txt'}elseif($Mode -eq 'Legacy'){'runtime-result.txt'}else{'bandage-runtime-result.txt'}
$log=Join-Path $evidence ('bandage-'+$Mode.ToLowerInvariant()+'-player.log')
$arguments=@($flag,'--evidence',('"'+$evidence+'"'),'-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0','-logFile',('"'+$log+'"'))
if($ThirtyFPS){$arguments+='--30fps'}
if($Mode -eq 'Visual' -or $Visible){$style='Normal'}else{$style='Hidden';$arguments+=@('-batchmode','-nographics')}
$started=Get-Date
$test=Start-Process -FilePath $player -ArgumentList $arguments -WorkingDirectory $project -WindowStyle $style -PassThru
if(-not $test.WaitForExit(420000)){Stop-Process -Id $test.Id;throw 'Demo validation timed out; inspect player log.'}
$result=Join-Path $evidence $resultName
if($test.ExitCode -ne 0){if(Test-Path -LiteralPath $result){Get-Content -LiteralPath $result};throw "Demo player exited $($test.ExitCode). See $log"}
if(-not(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTime -lt $started){throw 'Player did not produce fresh test evidence.'}
Get-Content -LiteralPath $result

