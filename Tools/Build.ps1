param([string]$Unity = 'C:\Program Files\Unity\Hub\Editor\6000.4.0f1\Editor\Unity.exe')
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
if (-not (Test-Path -LiteralPath $Unity)) { throw "Unity Editor not found: $Unity" }
$arguments = @('-batchmode', '-nographics', '-quit', '-projectPath', ('"' + $project + '"'), '-buildTarget', 'StandaloneWindows64', '-executeMethod', 'FunstraBuild.Build', '-logFile', ('"' + (Join-Path $project 'Evidence\build.log') + '"'))
$build = Start-Process -FilePath $Unity -ArgumentList $arguments -WindowStyle Hidden -Wait -PassThru
if ($build.ExitCode -ne 0) { throw "Unity build failed ($($build.ExitCode)). See Evidence\build.log." }
Get-Content -LiteralPath (Join-Path $project 'Evidence\build-result.txt')
