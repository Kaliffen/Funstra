$ErrorActionPreference = 'Stop'
$project = Split-Path -Parent $PSScriptRoot
$evidence = Join-Path $project 'Evidence'
$arguments = @('--visual-check', '--evidence', ('"' + $evidence + '"'), '-screen-width', '1600', '-screen-height', '900', '-screen-fullscreen', '0', '-logFile', ('"' + (Join-Path $evidence 'visual-player.log') + '"'))
# This command is explicitly for a visible review of the interactive player.
$test = Start-Process -FilePath (Join-Path $project 'Build\Funstra.exe') -ArgumentList $arguments -WorkingDirectory $project -WindowStyle Normal -PassThru
if (-not $test.WaitForExit(60000)) { Stop-Process -Id $test.Id; throw 'Visual capture timed out.' }
if ($test.ExitCode -ne 0) { throw "Visual check failed. See Evidence\visual-result.txt." }
Get-Content -LiteralPath (Join-Path $evidence 'visual-result.txt')
