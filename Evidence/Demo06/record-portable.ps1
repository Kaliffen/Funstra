$ErrorActionPreference='Stop'
$root=(Get-Location).Path
$e='Evidence/Demo06'
$p=Get-Content "$e/package-verification.json" -Raw|ConvertFrom-Json
$identity=Get-Content "$e/portable-crew/build-identity.json" -Raw|ConvertFrom-Json
$result=Get-Content "$e/portable-crew/crew-runtime-result.txt"
if($result[0] -ne 'PASS' -or $identity.playerExitCode -ne 0 -or $identity.assemblySha256 -ne $p.assemblySha256){throw 'Portable runtime identity/result failed'}
if(@($result|Where-Object{$_ -match '^PASS:'}).Count -ne 141){throw 'Portable Crew route incomplete'}
if((Get-FileHash -LiteralPath $p.archive).Hash -ne $p.archiveSha256){throw 'Archive changed after packaging'}
$v=Get-Content "$e/portable-verification.json" -Raw|ConvertFrom-Json
$v.runtime='PASS: full guided Crew route,141 assertions and17 captures'
$v|Add-Member -Force NoteProperty identity $identity
$v|Add-Member -Force NoteProperty result 'Evidence/Demo06/portable-crew/crew-runtime-result.txt'
$v|ConvertTo-Json -Depth 8|Set-Content "$e/portable-verification.json" -Encoding utf8
$s=Get-Content "$e/validation-summary.json" -Raw|ConvertFrom-Json
$s.phase='Reviewed, integrated and portable-tested; source publication and live release/site verification pending'
$s|Add-Member -Force NoteProperty package $p
$s|Add-Member -Force NoteProperty portable $v
$s|ConvertTo-Json -Depth 30|Set-Content "$e/validation-summary.json" -Encoding utf8
$path=Join-Path $root 'Docs/funstra-review-dossier-demo06.html'
$html=[IO.File]::ReadAllText($path)
$html=$html.Replace('B routes and scoped replays pass · delivery pending','Portable checked · publication pending')
$old='<div class="status"><strong>Delivery pending.</strong> This is the local prepublication review dossier. Package integrity, extracted portable-player verification, public release and website retention checks remain separate gates. No Demo06 publication success is claimed here; the established public release remains 0.4.2 until actual delivery is verified. Owner visual acceptance remains unclaimed.</div>'
$new='<div class="status" id="delivery-receipt"><strong>Portable verified; publication pending.</strong> All 496 extracted files match the archive. The extracted B player passed a fresh full Crew route: 141 assertions and17 captures. <a href="../Evidence/Demo06/package-verification.json">Package receipt</a> · <a href="../Evidence/Demo06/portable-verification.json">Extraction and runtime receipt</a>. Archive:177810627 bytes; SHA-256 <span class="hash">462F4D42B90242ED735A95FBCA21597E1976D15F5A8C116BD7F30362B7C29A48</span>. The embedded archive dossier is the earlier packaging snapshot; this online/source dossier carries later delivery receipts. Public release, website and retention verification are still pending. Owner gameplay and visual acceptance remain separate.</div>'
if(-not $html.Contains($old)){throw 'Dossier delivery marker missing'}
[IO.File]::WriteAllText($path,$html.Replace($old,$new),[Text.UTF8Encoding]::new($false))
$validation=Join-Path $root 'Evidence/VALIDATION.md'
$text=[IO.File]::ReadAllText($validation)
$text=$text.Replace('Package/extracted-player and public delivery receipts are pending at this preparation checkpoint.','The byte-preserving archive is177810627 bytes, SHA-256 `462F4D42B90242ED735A95FBCA21597E1976D15F5A8C116BD7F30362B7C29A48`. All496 extracted files match; the portable player passed a fresh141-assertion/17-capture full Crew route. [Package receipt](Demo06/package-verification.json) · [Portable receipt](Demo06/portable-verification.json). Public delivery verification remains pending at this checkpoint.')
[IO.File]::WriteAllText($validation,$text,[Text.UTF8Encoding]::new($false))
Write-Output 'Portable verification recorded; archive unchanged.'
