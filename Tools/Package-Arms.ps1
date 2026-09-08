param([Parameter(Mandatory)][ValidatePattern('^[0-9a-fA-F]{64}$')][string]$ExpectedAssemblySha256)
$ErrorActionPreference='Stop'
$project=Split-Path -Parent $PSScriptRoot
$build=Join-Path $project 'Build/PriceOfAGun'
$evidence=Join-Path $project 'Evidence/Demo05'
$assembly=Join-Path $build 'Funstra_Data/Managed/Assembly-CSharp.dll'
$hash=(Get-FileHash -LiteralPath $assembly).Hash
if($hash -ne $ExpectedAssemblySha256){throw 'Candidate differs from the tested assembly.'}
$routes=@(
    @{folder='arms';result='arms-runtime-result.txt';mode='Arms'},
    @{folder='streets';result='streets-runtime-result.txt';mode='Streets'},
    @{folder='police';result='police-runtime-result.txt';mode='Police'},
    @{folder='legacy';result='runtime-result.txt';mode='Legacy'}
)
foreach($route in $routes){
    $root=Join-Path $evidence ('final/'+$route.folder)
    $id=Get-Content -LiteralPath (Join-Path $root 'build-identity.json') -Raw | ConvertFrom-Json
    if($id.assemblySha256 -ne $hash -or $id.mode -ne $route.mode){throw "Wrong candidate evidence: $root"}
    if((Get-Content -LiteralPath (Join-Path $root $route.result) -TotalCount 1) -ne 'PASS'){throw "Missing passing route: $root"}
    $when=if($id.testedAt -is [DateTime]){$id.testedAt.ToUniversalTime()}else{[DateTimeOffset]::Parse($id.testedAt).UtcDateTime}
    if($when -lt (Get-Item -LiteralPath $assembly).LastWriteTimeUtc){throw "Stale evidence: $root"}
    if(Select-String -LiteralPath (Join-Path $root 'player.log') -Pattern 'NullReferenceException|InvalidOperationException|IndexOutOfRangeException|ArgumentException|MissingReferenceException|Assertion failed' -Quiet){throw "Runtime error: $root"}
}
$release=Join-Path $project 'Releases/Funstra-the-price-of-a-gun-0.5.0-windows.zip'
if(Test-Path -LiteralPath $release){throw 'Local candidate package already exists; preserve it and identify a new candidate.'}
New-Item -ItemType Directory -Force -Path (Split-Path -Parent $release) | Out-Null
Add-Type -AssemblyName System.IO.Compression.FileSystem
$zip=[IO.Compression.ZipFile]::Open($release,[IO.Compression.ZipArchiveMode]::Create)
try {
    function Add-CandidateFile([string]$source,[string]$name){
        [IO.Compression.ZipFileExtensions]::CreateEntryFromFile($zip,$source,$name.Replace('\','/'),[IO.Compression.CompressionLevel]::Optimal)|Out-Null
    }
    foreach($file in Get-ChildItem -LiteralPath $build -File -Recurse){Add-CandidateFile $file.FullName $file.FullName.Substring($build.Length+1)}
    foreach($relative in @('THE-PRICE-OF-A-GUN.md','LICENSE','CREDITS.md','Assets/Resources/Architecture/README.md','Assets/Resources/Audio/CREDITS.md','Assets/Resources/Audio/provenance.json','Assets/Resources/Audio/Kenney-Impact-License.txt','Assets/Resources/Audio/Kenney-Interface-License.txt','Evidence/Demo05/validation-summary.json')){
        Add-CandidateFile (Join-Path $project $relative) $relative
    }
    $writer=[IO.StreamWriter]::new($zip.CreateEntry('START-HERE.txt').Open())
    try{$writer.Write("FUNSTRA / THE PRICE OF A GUN / LOCAL DEMO 0.5.0`r`nExtract the entire archive, then run Funstra.exe with its data folder and DLLs alongside it.`r`nControls and route: THE-PRICE-OF-A-GUN.md`r`nThis is the local demo candidate. It has not been published or independently panel-reviewed.`r`n")}finally{$writer.Dispose()}
}finally{$zip.Dispose()}
$zip=[IO.Compression.ZipFile]::OpenRead($release)
try{
    foreach($file in Get-ChildItem -LiteralPath $build -File -Recurse){
        $name=$file.FullName.Substring($build.Length+1).Replace('\','/')
        $entry=$zip.GetEntry($name);if(-not $entry){throw "Missing player file: $name"}
        $stream=$entry.Open();$sha=[Security.Cryptography.SHA256]::Create()
        try{$packed=[BitConverter]::ToString($sha.ComputeHash($stream)).Replace('-','')}finally{$stream.Dispose();$sha.Dispose()}
        if($packed -ne (Get-FileHash -LiteralPath $file.FullName).Hash){throw "Player file changed in archive: $name"}
    }
}finally{$zip.Dispose()}
if((Get-FileHash -LiteralPath $assembly).Hash -ne $hash){throw 'Build changed during packaging.'}
[ordered]@{archive=$release;assemblySha256=$hash;archiveSha256=(Get-FileHash -LiteralPath $release).Hash;bytes=(Get-Item -LiteralPath $release).Length;verifiedAt=[DateTime]::UtcNow.ToString('o');method='All player files compared byte for byte through SHA256; no rebuild and no publication.'}|ConvertTo-Json|Set-Content -LiteralPath (Join-Path $evidence 'package-verification.json') -Encoding utf8
Get-Content -LiteralPath (Join-Path $evidence 'package-verification.json')
