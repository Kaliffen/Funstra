param(
    [ValidateRange(0,4)][int]$Level=0,
    [switch]$Visible,
    [switch]$ThirtyFPS,
    [int]$Width=1280,
    [int]$Height=720,
    [string]$PlayerPath,
    [string]$EvidenceRoot
)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
if(-not $PlayerPath){$PlayerPath=Join-Path $project 'Build\StreetsWorthFightingFor\Funstra.exe'}
if(-not $EvidenceRoot){$EvidenceRoot=Join-Path $project 'Evidence\Foundation'}
$PlayerPath=[IO.Path]::GetFullPath($PlayerPath)
$EvidenceRoot=[IO.Path]::GetFullPath($EvidenceRoot)
if(-not(Test-Path -LiteralPath $PlayerPath)){throw "Player not found: $PlayerPath"}
$levels=if($Level -eq 0){1..4}else{@($Level)}
$lock=New-Object System.Threading.Mutex($false,'Local\FunstraFoundationValidation')
$owned=$false
try {
    $owned=$lock.WaitOne(0)
    if(-not $owned){throw 'Another foundation validation is using this machine. Serialize rendered runs.'}
    foreach($number in $levels){
        $folder=Join-Path $EvidenceRoot ('level-'+$number)
        New-Item -ItemType Directory -Force -Path $folder | Out-Null
        $result=Join-Path $folder ('foundation-'+$number+'-result.txt')
        $log=Join-Path $folder 'player.log'
        $arguments=@('--mute-tests','--test-level',$number,'--foundation-test','--evidence',('"'+$folder+'"'),'-screen-width',$Width,'-screen-height',$Height,'-screen-fullscreen','0','-logFile',('"'+$log+'"'))
        if($ThirtyFPS){$arguments+='--30fps'}
        $style=if($Visible){'Normal'}else{'Hidden'}
        $started=Get-Date
        $player=Start-Process -FilePath $PlayerPath -ArgumentList $arguments -WorkingDirectory $project -WindowStyle $style -PassThru
        if(-not $player.WaitForExit(240000)){Stop-Process -Id $player.Id;throw "Level $number timed out. Inspect $log"}
        if(-not(Test-Path -LiteralPath $result) -or (Get-Item -LiteralPath $result).LastWriteTime -lt $started){throw "Level $number produced no fresh result. Inspect $log"}
        $report=Get-Content -LiteralPath $result -Raw
        Write-Output $report
        if($player.ExitCode -ne 0 -or -not $report.StartsWith('PASS')){throw "Level $number failed. Inspect $log"}
        if(Select-String -LiteralPath $log -Pattern 'NullReferenceException|InvalidOperationException|IndexOutOfRangeException|ArgumentException' -Quiet){throw "Level $number logged a runtime exception."}
        $identity=[ordered]@{
            executable=$PlayerPath
            sha256=(Get-FileHash -LiteralPath $PlayerPath -Algorithm SHA256).Hash
            testedAt=$started.ToUniversalTime().ToString('o')
            level=$number
            visible=[bool]$Visible
            width=$Width
            height=$Height
            frameCap=if($ThirtyFPS){30}else{60}
            method='Guided production controller and simulation; explicit fixture setups. Not human input or minimum hardware validation.'
        }
        $assembly=Join-Path (Split-Path -Parent $PlayerPath) 'Funstra_Data\Managed\Assembly-CSharp.dll'
        if(Test-Path -LiteralPath $assembly){$identity.assemblySha256=(Get-FileHash -LiteralPath $assembly -Algorithm SHA256).Hash}
        $identity | ConvertTo-Json | Set-Content -LiteralPath (Join-Path $folder 'build-identity.json') -Encoding utf8
    }
} finally {
    if($owned){$lock.ReleaseMutex()}
    $lock.Dispose()
}
