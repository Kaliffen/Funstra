param([switch]$Visible)
$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$evidence = Join-Path $project 'Evidence'
$arguments = @('--smoke-test', '--evidence', ('"' + $evidence + '"'), '-screen-width', '1600', '-screen-height', '900', '-screen-fullscreen', '0', '-logFile', ('"' + (Join-Path $evidence 'player.log') + '"'))
if ($Visible) { $arguments += '--capture-screens' }
$style = if ($Visible) { 'Normal' } else { 'Hidden' }
$test = Start-Process -FilePath (Join-Path $project 'Build\Funstra.exe') -ArgumentList $arguments -WorkingDirectory $project -WindowStyle $style -PassThru
if (-not $test.WaitForExit(300000)) { Stop-Process -Id $test.Id; throw 'Player test timed out. See Evidence\player.log.' }
if ($test.ExitCode -ne 0) { throw "Player test failed ($($test.ExitCode)). See Evidence\runtime-result.txt and player.log." }
Get-Content -LiteralPath (Join-Path $evidence 'runtime-result.txt')
